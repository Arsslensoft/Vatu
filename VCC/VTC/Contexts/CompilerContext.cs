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
using System.Threading;
using Vasm;
using VTC.Core;

namespace VTC
{
  
    public sealed class CompilerContext
    {
        internal static bool EntryPointFound = false;
        internal static Settings CompilerOptions { get; set; }
    
        public Settings Options { get; set; }
        public string TempSrcFile { get; set; }
        public List<DependencyParsing> DependencyCache;
        public List<CompiledSource> CompiledSources { get; set; }
        public FIFOSemaphore ParallelSupervisor { get; set; }
   
        int compilers = 0;
        List<ParallelCompiledSource> _compiled =new List<ParallelCompiledSource>();
  
        public CompilerContext(Settings opt)
        {
            compilers= opt.Sources.Length;
            CompilerOptions = opt;
            EntryPointFound = false;
            Options = opt;
            TempSrcFile = Path.GetTempFileName();
            DependencyCache = new List<DependencyParsing>();
            CompiledSources = new List<CompiledSource>();
            ParallelSupervisor = new FIFOSemaphore(opt.ParallelThreads);
            InitGrammar();
         
        }
        public CompilerContext(string file)
        {
            Options = new Settings();

            Options.IsInterrupt = true;
            Options.Sources = new string[1] { file };
            CompilerOptions = Options;
            EntryPointFound = false;
            DependencyCache = new List<DependencyParsing>();
            CompiledSources = new List<CompiledSource>();
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
        public AsmContext CreateAsmContext(CompiledSource src)
        {
            return new AsmContext(src.Asmw);
        }
        public EmitContext CreateEmit(AsmContext actx)
        {
            return new EmitContext(actx);
        }
        public EmitContext CreateEmit(CompiledSource src)
        {
            return new EmitContext(src.Asmw);
        }
        public void PrepareEmit(EmitContext ec)
        {
            ec.ag.IsFlat = Options.Target == Target.flat || Options.Target == Target.tiny;
            ec.ag.IsInterruptOverload = Options.IsInterrupt;
            ec.ag.OLevel = Options.OptimizeLevel;
            ec.ag.IsVTExec = Options.Target == Target.vexe;
            ec.ag.IsLibrary = Options.Target == Target.obj;
        }
     
      
      

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
        public bool IncludeFile(string file,CompiledSource csrc)
        {
            DependencyParsing dep = new DependencyParsing();
            dep.File = file;

            dep.File =FixInclude(dep.File, Path.GetDirectoryName(ResolveContext.Report.FilePath));
            string path = Path.GetDirectoryName(dep.File);
            if (!Paths.Contains(path))
                Paths.Add(path);
            if (DependencyCache.Contains(dep))
            {
                dep = DependencyCache[DependencyCache.IndexOf(dep)];
                return ResolveDependency(csrc, DependencyCache.IndexOf(dep));
            }
            else
            {
                dep.InputStream = new StreamReader(File.OpenRead(dep.File));
                DependencyCache.Add(dep);
                return ResolveDependency(csrc, DependencyCache.Count - 1);
            }
         
        }
        #endregion


        public bool ResolveDependency(CompiledSource src,int idx)
        {
            DependencyParsing dep = DependencyCache[idx];
            if (dep.Parsed)
            {
                // transfer
                src.DefaultDependency.RootCtx.FillKnownByKnown(dep.RootCtx.Resolver);
                src.DefaultDependency.DependsOn.Add(dep);
                return true;

            }
            lock (DependencyCache[idx])
            {

                src.DefaultDependency.DependsOn.Add(dep);
                string oldf = ResolveContext.Report.FilePath;
                ResolveContext.Report.FilePath = dep.File;
                bool ok = true;
                try
                {



                    var processor = new SemanticProcessor<SimpleToken>(dep.InputStream, actions);

                    ParseMessage parseMessage = processor.ParseAll();


                    if (parseMessage == ParseMessage.Accept)
                    {


                        CompilationUnit cunit = processor.CurrentToken as CompilationUnit;
                        // includes
                        foreach (IncludeDeclaration incl in cunit.Includes)
                            IncludeFile(incl.IncludeFile, src);
                        // global decls
                        var globals = cunit.Globals;
                        ResolveContext RootCtx = null;
                        List<Declaration> Resolved = new List<Declaration>();
                        List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                        // transfer





                        ok &= ResolveSemanticTree(src, cunit, ref RootCtx, ref Resolved, ref ResolveCtx);
                        if (ok) // Emit
                        {
                            dep.Declarations = Resolved;
                            dep.ResolveCtx = ResolveCtx;
                            dep.RootCtx = RootCtx;
                            // transfer
                            src.DefaultDependency.RootCtx.FillKnownByKnown(RootCtx.Resolver);

                        }

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
        }
        public bool ResolveSemanticTree(CompiledSource src,CompilationUnit cunit,ref ResolveContext RootCtx, ref List<Declaration> Resolved, ref List<ResolveContext> ResolveCtx,bool isdef=false)
        {

            foreach (Global gb in cunit.Globals)
            {
             
                
                ResolveContext old_ctx = RootCtx;


                RootCtx = ResolveContext.CreateRootContext(gb.Used, gb.Namespace, gb.Declarations);
                Global stmts = (Global)gb.DoResolve(RootCtx);
            

                if (isdef)
                    src.DefaultDependency.RootCtx = RootCtx;
                else RootCtx.FillKnownByKnown(src.DefaultDependency.RootCtx.Resolver);

                // includes
                foreach (IncludeDeclaration incl in cunit.Includes)
                    IncludeFile(incl.IncludeFile, src);

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
                            md.Resolve(childctx);
                            MethodDeclaration d = (MethodDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);

                            // Flow Analysis
                            if (Options.Flow)
                            {
                                FlowAnalysisContext fc = new FlowAnalysisContext( d);
                                fc.DoFlowAnalysis(childctx);
                            }
                        }
                        else if (vstmt is OperatorDeclaration)
                        {
                            OperatorDeclaration md = (OperatorDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            md.Resolve(childctx);
                            OperatorDeclaration d = (OperatorDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);

                            // Flow Analysis
                            if (Options.Flow)
                            {
                                FlowAnalysisContext fc = new FlowAnalysisContext( d);
                                fc.DoFlowAnalysis(childctx);
                            }
                        }
                        else if (vstmt is InterruptDeclaration)
                        {
                            InterruptDeclaration md = (InterruptDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            md.Resolve(childctx);
                            InterruptDeclaration d = (InterruptDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);


                            // Flow Analysis
                            if (Options.Flow)
                            {
                                FlowAnalysisContext fc = new FlowAnalysisContext( d);
                                fc.DoFlowAnalysis(childctx);
                            }
                        }
                        else if (vstmt is StructDeclaration)
                        {
                            StructDeclaration md = (StructDeclaration)vstmt;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            md.Resolve(childctx);
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
                            md.Resolve(childctx);
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
                            md.Resolve(childctx);
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
        public bool PreprocessSources()
        {

            if (Options.Includes != null)
                Paths.AddRange(Options.Includes);

            // init default paths

            int i = 0;

            foreach (string src in Options.Sources)
            {

                if (File.Exists(src))
                {
                    if (!Paths.Contains(Path.GetDirectoryName(src)))
                        Paths.Add(Path.GetDirectoryName(src));

                    DependencyParsing dep = new DependencyParsing();
                    dep.File = src;
                     CompiledSource csrc = new CompiledSource();
                     csrc.DefaultDependency = dep;
                     if (!DependencyCache.Contains(dep))
                         DependencyCache.Add(dep);


                    if (!CompiledSources.Contains(csrc))
                    {
                        csrc.DefaultDependency.InputStream = new StreamReader(File.OpenRead(dep.File));
                        if (Options.AssemblyOutput != null && Options.AssemblyOutput.Length > 0)
                           csrc.Asmw = new AssemblyWriter(Options.AssemblyOutput[i]);

                        CompiledSources.Add(csrc);


                    }

                }
                i++;
            }


            return true;
        }
        public bool ResolveAndEmit()
        {
            bool ok = PreprocessSources();
           

            try{


                foreach (CompiledSource csrc in CompiledSources) // compile all sources
                {
                    Thread thr = new Thread(new ParameterizedThreadStart(ParallelCompile));
                    ParallelCompiledSource pc=     new ParallelCompiledSource(csrc);
                    thr.Start(pc);
                 
                }

                while (compilers > 0)
                    Thread.Sleep(100);
                    foreach (ParallelCompiledSource pc in _compiled)
                        ok &= pc.Result;
     
                
            }
            catch(Exception ex){
                ok = false;
              Console.WriteLine("Error: " + ex.Message);
              Console.WriteLine(ex.StackTrace);
            }
            return ok;
        }
        bool lock_taken = false;
        public void ParallelCompile(object pco)
        {



            int idx = 0;
            ParallelCompiledSource pc = (ParallelCompiledSource)pco;
            lock (_compiled)
            {
                idx = _compiled.Count;
                _compiled.Add(pc);

            }
         

       
            ParallelSupervisor.Acquire();
            try
            {

                _compiled[idx].Result = Compile(pc);
                
            }
            catch (Exception ex)
            {
                _compiled[idx].Result = false;
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }


            compilers--;
            ParallelSupervisor.Release();
     
        }
        public bool Compile(ParallelCompiledSource pc)
        {
        
            bool ok = true;
              pc.Time.Reset();
              pc.Time.Start();
                    ResolveContext.Report.FilePath = pc.Source.DefaultDependency.File;

                    var processor = new SemanticProcessor<SimpleToken>(pc.Source.DefaultDependency.InputStream, actions);

                    ParseMessage parseMessage = processor.ParseAll();
                
               
                    if (parseMessage == ParseMessage.Accept)
                    {
                        CompilationUnit cunit = processor.CurrentToken as CompilationUnit;


                        ResolveContext RootCtx = null;
                        List<Declaration> Resolved = new List<Declaration>();
                        List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                        List<Declaration> RResolved = new List<Declaration>();
                        List<ResolveContext> RResolveCtx = new List<ResolveContext>();

                        ok &= ResolveSemanticTree(pc.Source, cunit, ref RootCtx, ref Resolved, ref ResolveCtx, true);


                        if (ok) // Emit & Fill Dependencies
                        {
                            // Fill Dependencies
                            foreach (DependencyParsing dep in pc.Source.DefaultDependency.DependsOn)
                            {
                                foreach (ResolveContext rctx in dep.ResolveCtx)
                                    RResolveCtx.Add(rctx);

                                // decls
                                foreach (Declaration decl in dep.Declarations)
                                    RResolved.Add(decl);
                            }
                            Resolved.InsertRange(0, RResolved.ToArray());
                            ResolveCtx.InsertRange(0, RResolveCtx.ToArray());

                            EmitContext ec = CreateEmit(pc.Source);
                            int i = 0;
                            foreach (Declaration stmt in Resolved)
                            {
                                ec.SetCurrentResolve(ResolveCtx[i]);
                                stmt.Emit(ec);
                                i++;
                            }
                            PrepareEmit(ec);

                            ec.Emit();

                            pc.Source.Asmw.Flush();
                            pc.Source.Asmw.Close();
                            //   Build();
                        }
                    }
                    else
                    {
                        ok = false;
                        IToken token = processor.CurrentToken;
                        ReportParserMessage(parseMessage, token, processor.GetExpectedTokens());

                    }
                    pc.Time.Stop();
                    pc.Source.CompileTime = pc.Time.Elapsed;
                    if (Options.Verbose)
                        Console.WriteLine(Path.GetFileName(pc.Source.DefaultDependency.File) + " compiled in " + pc.Source.CompileTime);
                  
  
                    return ok;
        }
        public bool Resolve()
        {
              bool ok = PreprocessSources();
    
            try{
                Stopwatch st = new Stopwatch();
                foreach (CompiledSource csrc in CompiledSources) // compile all sources
                {
                    st.Reset();
                    st.Start();
                    ResolveContext.Report.FilePath = csrc.DefaultDependency.File;
                    var processor = new SemanticProcessor<SimpleToken>(csrc.DefaultDependency.InputStream, actions);

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

                        ok &= ResolveSemanticTree(csrc,cunit, ref RootCtx, ref Resolved, ref ResolveCtx, true);


                        csrc.DefaultDependency.ResolveCtx = RResolveCtx;
                        csrc.DefaultDependency.Declarations = RResolved;
                    }
                    else
                    {
                        ok = false;
                        IToken token = processor.CurrentToken;
                        ReportParserMessage(parseMessage, token, processor.GetExpectedTokens());
                    }
                    st.Stop();
                    csrc.CompileTime = st.Elapsed;
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
            foreach (CompiledSource s in CompiledSources)
                CloseDep(s.DefaultDependency);

       
            
        }

        public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
        {
            return new Location(li.Line, li.Column, li.Index);
        }
    }
}
