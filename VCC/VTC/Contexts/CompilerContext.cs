using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vasm;
using VTC.Core;

namespace VTC
{
    public class DependencyParsing : IEquatable<DependencyParsing>
    {
        
        public ResolveContext RootCtx { get; set; }
        public List<ResolveContext> ResolveCtx { get; set; }
        public List<Declaration> Declarations { get; set; }
        public bool Parsed { get; set; }
        public string File { get; set; }
        public List<DependencyParsing> DependsOn = new List<DependencyParsing>();
        public StreamReader InputStream { get; set; }

        public bool Equals(DependencyParsing dep)
        {
            return dep.File == File;
        }
    }
    public class CompilerContext
    {
        internal static bool EntryPointFound = false;
        internal static Settings CompilerOptions { get; set; }
        public AssemblyWriter Asmw { get; set; }
        public Settings Options { get; set; }
        public string TempSrcFile { get; set; }
        public List<DependencyParsing> InputSources;
       
        public CompilerContext(Settings opt)
        {
            CompilerOptions = opt;
            EntryPointFound = false;
            Options = opt;
            TempSrcFile = Path.GetTempFileName();
            InputSources = new List<DependencyParsing>();
            Asmw = new AssemblyWriter(opt.Output);
            InitGrammar();
        }
        public CompilerContext(string file)
        {
            Options = new Settings();

            Options.IsInterrupt = true;
            Options.Sources = new string[1] { file };
            CompilerOptions = Options;
            EntryPointFound = false;
            InputSources = new List<DependencyParsing>();
            BuiltinTypeSpec.ResetBuiltins();
            InitGrammar();
        }
        CompiledGrammar grammar; SemanticTypeActions<SimpleToken> actions;
        public void InitGrammar()
        {
          //  Stopwatch st = new Stopwatch(); st.Start();
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VTC.VATU.egt");
          


                // compile/run each statement one entry at a time
                 grammar = CompiledGrammar.Load(stream);
                 actions = new SemanticTypeActions<SimpleToken>(grammar);

            //     st.Stop();
              //   Console.WriteLine("Init : " + st.Elapsed);
        }
        public AsmContext CreateAsmContext()
        {
            return new AsmContext(Asmw);
        }
        public EmitContext CreateEmit(AsmContext actx)
        {
            return new EmitContext(actx);
        }
        public EmitContext CreateEmit()
        {
            return new EmitContext(Asmw);
        }
        public void PrepareEmit(EmitContext ec)
        {
            ec.ag.IsFlat = Options.Target == Target.flat || Options.Target == Target.tiny;
            ec.ag.IsInterruptOverload = Options.IsInterrupt;
            ec.ag.OLevel = Options.OptimizeLevel;
            ec.ag.IsVTExec = Options.Target == Target.vexe;
            ec.ag.IsLibrary = Options.Target == Target.obj;
        }
     
        public bool Build()
        {
         Process p =   Process.Start("nasm", string.Format("{0} -f bin -o {1}", Options.Output, Options.Output.Replace(".asm",".bin")));
         p.WaitForExit();
            return true;

        }
        public DependencyParsing DefaultDependency { get; set; }

        public void ReportParserMessage(ParseMessage pm, IToken token, ReadOnlyCollection<Symbol> expected)
        {
            string err = "[P&Lex] ";
            if (pm == ParseMessage.LexicalError)
                err = "Lexical error ";
          
           else if (pm == ParseMessage.InternalError)
                err = "Internal error ";
            else if (pm == ParseMessage.SyntaxError)
                err = "Syntax error ";
            else if (pm == ParseMessage.BlockError)
                err = "Block error ";
            else err = "Group error ";
            string expects = "";
            if (expected != null && expected.Count > 0)
            {

                foreach (Symbol e in expected)
                    expects += ", " + e.Name;
                if (expects.Length > 0)
                    expects = expects.Remove(0, 2);

            }

            ResolveContext.Report.Error(0, CompilerContext.TranslateLocation(token.Position), err + expects + " expected");
        
        }

        #region Includes
        public string FixInclude(string file, string currentdir)
        {
            if (!Directory.Exists(currentdir))
                currentdir = "";
            string tryout = Path.GetFullPath(Path.Combine(currentdir, file));
            if (File.Exists(tryout))
                return tryout;
            foreach (string path in Paths)
            {
                tryout = Path.Combine(path, file);
                if (File.Exists(tryout))
                    return tryout;
            }
            return file;
        }
        internal List<string> Paths = new List<string>();
        public bool IncludeFile(string file)
        {
            DependencyParsing dep = new DependencyParsing();
            dep.File = file;

            dep.File =FixInclude(dep.File, Path.GetDirectoryName(ResolveContext.Report.FilePath));
            string path = Path.GetDirectoryName(dep.File);
            if (!Paths.Contains(path))
                Paths.Add(path);
            if (InputSources.Contains(dep))
            {
                dep = InputSources[InputSources.IndexOf(dep)];
                return ResolveDependency(InputSources.IndexOf(dep));
            }
            else
            {
                dep.InputStream = new StreamReader(File.OpenRead(dep.File));
                InputSources.Add(dep);
                return ResolveDependency(InputSources.Count - 1);
            }
         
        }
        #endregion


        public bool ResolveDependency(int idx)
        {
            DependencyParsing dep = InputSources[idx];
            if (dep.Parsed)
                return true;
            DefaultDependency.DependsOn.Add(dep);
            string oldf = ResolveContext.Report.FilePath;
            ResolveContext.Report.FilePath = dep.File;
            bool ok = true;
            try
            {

               // Stopwatch st = new Stopwatch(); st.Start();
          
                    var processor = new SemanticProcessor<SimpleToken>(dep.InputStream, actions);
                 
                    ParseMessage parseMessage = processor.ParseAll();

                //    st.Stop();
                 //   Console.WriteLine("["+dep.File+"] Parse time : " + st.Elapsed);
                    if (parseMessage == ParseMessage.Accept)
                    {


                        CompilationUnit cunit = processor.CurrentToken as CompilationUnit;
                        // includes
                        foreach (IncludeDeclaration incl in cunit.Includes)
                            IncludeFile(incl.IncludeFile);
                        // global decls
                        var globals = cunit.Globals;
                        ResolveContext RootCtx = null;
                        List<Declaration> Resolved = new List<Declaration>();
                        List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                          // transfer
                        
                       
                          
                        

                        ok &= ResolveSemanticTree(cunit, ref RootCtx, ref Resolved, ref ResolveCtx);
                        if (ok) // Emit
                        {
                            dep.Declarations = Resolved;
                            dep.ResolveCtx = ResolveCtx;
                            dep.RootCtx = RootCtx;
                            // transfer
                            DefaultDependency.RootCtx.FillKnownByKnown(RootCtx.Resolver);
                          
                        }
                        //{
                        //    EmitContext ec = CreateEmit();
                        //    int i = 0;
                        //    foreach (Declaration stmt in Resolved)
                        //    {
                        //        ec.SetCurrentResolve(ResolveCtx[i]);
                        //        stmt.Emit(ec);
                        //        i++;
                        //    }
                        //    PrepareEmit(ec);

                        //    ec.Emit();

                        //    Asmw.Flush();
                        //    Asmw.Close();
                        //    Build();
                        //}
                    }
                    else
                    {
                        ok = false;
                        IToken token = processor.CurrentToken;
                        ReportParserMessage(parseMessage, token, processor.GetExpectedTokens());
                    }
                
            }
            catch (Exception ex)
            {
                ok = false;
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            dep.Parsed = ok;
            ResolveContext.Report.FilePath = oldf;
            return ok;
        }
        public bool ResolveSemanticTree(CompilationUnit cunit,ref ResolveContext RootCtx, ref List<Declaration> Resolved, ref List<ResolveContext> ResolveCtx,bool isdef=false)
        {

            foreach (Global gb in cunit.Globals)
            {
             
                
                ResolveContext old_ctx = RootCtx;


                RootCtx = ResolveContext.CreateRootContext(gb.Used, gb.Namespace, gb.Declarations);
                Global stmts = (Global)gb.DoResolve(RootCtx);
            

                if (isdef)
                    DefaultDependency.RootCtx = RootCtx;
                else RootCtx.FillKnownByKnown(DefaultDependency.RootCtx.Resolver);

                // includes
                foreach (IncludeDeclaration incl in cunit.Includes)
                    IncludeFile(incl.IncludeFile);

                if (old_ctx != null)
                    RootCtx.FillKnownByKnown(old_ctx.Resolver);
                if (stmts != null)
                { 
                    // PreProcess
                    
                    foreach (Declaration sstmt in stmts.Declarations)
                    {
                        Declaration vstmt = sstmt.BaseDeclaration;
                        if (vstmt.BaseDeclaration != null)
                            vstmt = vstmt.BaseDeclaration;

                        if (vstmt is MethodDeclaration)
                        {
                            MethodDeclaration md = (MethodDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            vstmt.Resolve(childctx);
                            MethodDeclaration d = (MethodDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);

                            // Flow Analysis
                            if (Options.Flow)
                            {
                                FlowAnalysisContext fc = new FlowAnalysisContext(childctx.Resolver.KnownLocalVars.Count, d);
                                fc.DoFlowAnalysis(childctx);
                            }
                        }
                        else if (vstmt is OperatorDeclaration)
                        {
                            OperatorDeclaration md = (OperatorDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            vstmt.Resolve(childctx);
                            OperatorDeclaration d = (OperatorDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);

                            // Flow Analysis
                            if (Options.Flow)
                            {
                                FlowAnalysisContext fc = new FlowAnalysisContext(childctx.Resolver.KnownLocalVars.Count, d);
                                fc.DoFlowAnalysis(childctx);
                            }
                        }
                        else if (vstmt is InterruptDeclaration)
                        {
                            InterruptDeclaration md = (InterruptDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            vstmt.Resolve(childctx);
                            InterruptDeclaration d = (InterruptDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);


                            // Flow Analysis
                            if (Options.Flow)
                            {
                                FlowAnalysisContext fc = new FlowAnalysisContext(childctx.Resolver.KnownLocalVars.Count, d);
                                fc.DoFlowAnalysis(childctx);
                            }
                        }
                        else if (vstmt is StructDeclaration)
                        {
                            StructDeclaration md = (StructDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            vstmt.Resolve(childctx);
                            StructDeclaration d = (StructDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else if (vstmt is UnionDeclaration)
                        {
                            UnionDeclaration md = (UnionDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            vstmt.Resolve(childctx);
                            UnionDeclaration d = (UnionDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else if (vstmt is EnumDeclaration)
                        {
                            EnumDeclaration md = (EnumDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            vstmt.Resolve(childctx);
                            EnumDeclaration d = (EnumDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else
                        {
                            RootCtx.IsInTypeDef = vstmt.IsTypeDef;
                            RootCtx.IsInStruct = vstmt.IsStruct;
                            RootCtx.IsInUnion = vstmt.IsUnion;
                            vstmt.Resolve(RootCtx);
                            ResolveCtx.Add(RootCtx);
                            Declaration d = (Declaration)vstmt.DoResolve(RootCtx);
                            Resolved.Add(d);
                        }
                    }

                }

            }
            return (ResolveContext.Report.ErrorCount == 0);
        }
        public bool Preprocess()
        {

            if (Options.Includes != null)
                Paths.AddRange(Options.Includes);

            // init default paths


            foreach (string src in Options.Sources)
            {

                if (File.Exists(src))
                {
                    if (!Paths.Contains(Path.GetDirectoryName(src)))
                        Paths.Add(Path.GetDirectoryName(src));

                    DependencyParsing dep = new DependencyParsing();
                    dep.File = src;

                    if (!InputSources.Contains(dep))
                    {
                        dep.InputStream = new StreamReader(File.OpenRead(dep.File));
                        InputSources.Add(dep);
                        DefaultDependency = dep;
                    }

                }

            }


            return true;
        }
        public bool ResolveAndEmit()
        {
          bool ok = Preprocess();
           
            ResolveContext.Report.FilePath = DefaultDependency.File;
            try{

           //     Stopwatch st = new Stopwatch(); st.Start();
                var processor = new SemanticProcessor<SimpleToken>(InputSources[0].InputStream, actions);

                ParseMessage parseMessage = processor.ParseAll();
           //     st.Stop();
           //     Console.WriteLine("[" + DefaultDependency.File + "] Parse time : " + st.Elapsed);
                if (parseMessage == ParseMessage.Accept)
                {
                    CompilationUnit cunit = processor.CurrentToken as CompilationUnit;
                 
                   
                    ResolveContext RootCtx = null;
                    List<Declaration> Resolved = new List<Declaration>();
                    List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                    List<Declaration> RResolved = new List<Declaration>();
                    List<ResolveContext> RResolveCtx = new List<ResolveContext>();

                    ok &= ResolveSemanticTree(cunit, ref RootCtx, ref Resolved, ref ResolveCtx, true);


                    if (ok) // Emit & Fill Dependencies
                    { 
                        // Fill Dependencies
                        foreach (DependencyParsing dep in DefaultDependency.DependsOn)
                        {
                         foreach (ResolveContext rctx in dep.ResolveCtx)
                             RResolveCtx.Add(rctx);
                         
                            // decls
                            foreach (Declaration decl in dep.Declarations)
                                RResolved.Add( decl);
                        }
                        Resolved.InsertRange(0, RResolved.ToArray());
                        ResolveCtx.InsertRange(0, RResolveCtx.ToArray());

                        EmitContext ec = CreateEmit();
                        int i = 0;
                        foreach (Declaration stmt in Resolved)
                        {
                            ec.SetCurrentResolve(ResolveCtx[i]);
                            stmt.Emit(ec);
                            i++;
                        }
                        PrepareEmit(ec);
                        
                        ec.Emit();

                        Asmw.Flush();
                        Asmw.Close();
                     //   Build();
                    }
                }
                else{ ok = false;
                       IToken token = processor.CurrentToken;
                       ReportParserMessage(parseMessage, token, processor.GetExpectedTokens());

                }
            
            }
            catch(Exception ex){
                ok = false;
              Console.WriteLine("Error: " + ex.Message);
              Console.WriteLine(ex.StackTrace);
            }
            return ok;
        }
        public bool Resolve()
        {
              bool ok = Preprocess();
            ResolveContext.Report.FilePath = DefaultDependency.File;
            try{
             
                var processor = new SemanticProcessor<SimpleToken>(InputSources[0].InputStream, actions);

                ParseMessage parseMessage = processor.ParseAll();
        
                if (parseMessage == ParseMessage.Accept)
                {
                    CompilationUnit cunit = processor.CurrentToken as CompilationUnit;
                 
                    // global decls
                   
                    ResolveContext RootCtx = null;
                    List<Declaration> Resolved = new List<Declaration>();
                    List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                    List<Declaration> RResolved = new List<Declaration>();
                    List<ResolveContext> RResolveCtx = new List<ResolveContext>();

                    ok &= ResolveSemanticTree(cunit, ref RootCtx, ref Resolved, ref ResolveCtx, true);
                  
                 
                    DefaultDependency.ResolveCtx = RResolveCtx;
                    DefaultDependency.Declarations = RResolved;
                }
                else
                {
                    ok = false;
                    IToken token = processor.CurrentToken;
                    ReportParserMessage(parseMessage, token, processor.GetExpectedTokens());
                }

            }
            catch (Exception ex)
            {
                ok = false;
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return ok;
        }
        void CloseDep(DependencyParsing d)
        {
            if (d == null)
                return;
            foreach (DependencyParsing p in d.DependsOn)
                CloseDep(p);
            
            d.InputStream.Close();
        }
      
        public void Close()
        {
            CloseDep(DefaultDependency);

       
            
        }

        public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
        {
            return new Location(li.Line, li.Column, li.Index);
        }
    }
}
