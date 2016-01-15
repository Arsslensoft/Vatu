using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
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
        public AssemblyWriter Asmw { get; set; }
        public Settings Options { get; set; }
        public string TempSrcFile { get; set; }
        public List<DependencyParsing> InputSources;
        public CompilerContext(Settings opt)
        {
            Options = opt;
            TempSrcFile = Path.GetTempFileName();
            InputSources = new List<DependencyParsing>();
            Asmw = new AssemblyWriter(opt.Output);
            InitGrammar();
        }
        CompiledGrammar grammar; SemanticTypeActions<SimpleToken> actions;
        public void InitGrammar()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VTC.VATU.cgt");
          


                // compile/run each statement one entry at a time
                 grammar = CompiledGrammar.Load(stream);
                 actions = new SemanticTypeActions<SimpleToken>(grammar);

            
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
            ec.ag.IsFlat = Options.IsFlat;
            ec.ag.IsInterruptOverload = Options.IsInterrupt;
            ec.ag.OLevel = Options.OptimizeLevel;
        }
     
        public bool Build()
        {
         Process p =   Process.Start("nasm", string.Format("{0} -f bin -o {1}", Options.Output, Options.Output.Replace(".asm",".bin")));
         p.WaitForExit();
            return true;

        }
        public DependencyParsing DefaultDependency { get; set; }
        public bool Preprocess()
        {
            if(Options.Includes != null)
                 Preprocessor.Paths.AddRange(Options.Includes);
            // compiler symbols
            if(Options.Symbols != null)
            {
                foreach (string sym in Options.Symbols)
                    Preprocessor.Symbols.Add(sym, true);
            }


            // init default paths
       
     
            foreach (string src in Options.Sources)
            {
                  
                if(File.Exists(src))
                {
                    if(!Preprocessor.Paths.Contains(Path.GetDirectoryName(src)))
                             Preprocessor.Paths.Add(Path.GetDirectoryName(src));
          
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
               

                    var processor = new SemanticProcessor<SimpleToken>(dep.InputStream, actions);

                    ParseMessage parseMessage = processor.ParseAll();
                    if (parseMessage == ParseMessage.Accept)
                    {
                        var globals = processor.CurrentToken as GlobalSequence<Global>;
                        ResolveContext RootCtx = null;
                        List<Declaration> Resolved = new List<Declaration>();
                        List<ResolveContext> ResolveCtx = new List<ResolveContext>();


                        ok &= ResolveSemanticTree(globals, ref RootCtx, ref Resolved, ref ResolveCtx);
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
                        ResolveContext.Report.Error(0, CompilerContext.TranslateLocation(token.Position), "Syntax error '" + token.Symbol.Name + "' expected");

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
        public bool ResolveSemanticTree(GlobalSequence<Global> globals,ref ResolveContext RootCtx, ref List<Declaration> Resolved, ref List<ResolveContext> ResolveCtx,bool isdef=false)
        {
            foreach (Global stmts in globals)
            {

                ResolveContext old_ctx = RootCtx;


                RootCtx = ResolveContext.CreateRootContext(stmts.Used, stmts.Namespace, stmts.Declarations);
                if (isdef)
                    DefaultDependency.RootCtx = RootCtx;
                if (old_ctx != null)
                    RootCtx.FillKnownByKnown(old_ctx.Resolver);
                if (stmts != null)
                { 
                    // PreProcess
                    List<Declaration> Preprocessed = new List<Declaration>();
                    foreach (Declaration stmt in stmts.Declarations)
                    {
                        if (stmt.BaseDeclaration is PreprocessorDeclaration)
                        {
                            DeclarationSequence<Declaration> declsofpp = null;
                           bool ok =  (stmt.BaseDeclaration as PreprocessorDeclaration).Preprocess(this,ref declsofpp);
                           if (!ok)
                               return false;

                            if (declsofpp != null)
                                foreach (Declaration d in declsofpp)
                                    Preprocessed.Add(d);
                        }
                        else Preprocessed.Add(stmt);
                    }
                    //// fill default
                    //if (isdef)
                    //{
                    //    foreach (DependencyParsing dep in DefaultDependency.DependsOn)
                    //        RootCtx.FillKnownByKnown(dep.RootCtx.Resolver);
                    //}
                 
                    foreach (Declaration stmt in Preprocessed)
                    {
                        
                        if (stmt.BaseDeclaration is MethodDeclaration)
                        {
                            MethodDeclaration md = (MethodDeclaration)stmt.BaseDeclaration;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            stmt.Resolve(childctx);
                            MethodDeclaration d = (MethodDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else if (stmt.BaseDeclaration is OperatorDeclaration)
                        {
                            OperatorDeclaration md = (OperatorDeclaration)stmt.BaseDeclaration;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            stmt.Resolve(childctx);
                            OperatorDeclaration d = (OperatorDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else if (stmt.BaseDeclaration is InterruptDeclaration)
                        {
                            InterruptDeclaration md = (InterruptDeclaration)stmt.BaseDeclaration;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            stmt.Resolve(childctx);
                            InterruptDeclaration d = (InterruptDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else if (stmt.BaseDeclaration is StructDeclaration)
                        {
                            StructDeclaration md = (StructDeclaration)stmt.BaseDeclaration;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            stmt.Resolve(childctx);
                            StructDeclaration d = (StructDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else if (stmt.BaseDeclaration is UnionDeclaration)
                        {
                            UnionDeclaration md = (UnionDeclaration)stmt.BaseDeclaration;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            stmt.Resolve(childctx);
                            UnionDeclaration d = (UnionDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else if (stmt.BaseDeclaration is EnumDeclaration)
                        {
                            EnumDeclaration md = (EnumDeclaration)stmt.BaseDeclaration;
                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                            stmt.Resolve(childctx);
                            EnumDeclaration d = (EnumDeclaration)md.DoResolve(childctx);
                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                            RootCtx.UpdateFather(childctx);
                            Resolved.Add(d);
                            ResolveCtx.Add(childctx);
                        }
                        else
                        {
                            RootCtx.IsInTypeDef = stmt.IsTypeDef;
                            RootCtx.IsInStruct = stmt.IsStruct;
                            RootCtx.IsInUnion = stmt.IsUnion;
                            stmt.Resolve(RootCtx);
                            ResolveCtx.Add(RootCtx);
                            Declaration d = (Declaration)stmt.DoResolve(RootCtx);
                            Resolved.Add(d);
                        }
                    }

                }

            }
            return (ResolveContext.Report.ErrorCount == 0);
        }
        public bool ResolveAndEmit()
        {
         
            bool ok = Preprocess();
            ResolveContext.Report.FilePath = DefaultDependency.File;
            try{
           

                var processor = new SemanticProcessor<SimpleToken>(InputSources[0].InputStream, actions);

                ParseMessage parseMessage = processor.ParseAll();
                if (parseMessage == ParseMessage.Accept)
                {
                    var globals = processor.CurrentToken as GlobalSequence<Global>;
                    ResolveContext RootCtx = null;
                    List<Declaration> Resolved = new List<Declaration>();
                    List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                    List<Declaration> RResolved = new List<Declaration>();
                    List<ResolveContext> RResolveCtx = new List<ResolveContext>();

                    ok &= ResolveSemanticTree(globals,ref RootCtx,ref Resolved,ref ResolveCtx, true);


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
                       ResolveContext.Report.Error(0, CompilerContext.TranslateLocation(token.Position), "Syntax error '" + token.Symbol.Name +"' expected");

                }
            
            }
            catch(Exception ex){
                ok = false;
              Console.WriteLine("Error: " + ex.Message);
              Console.WriteLine(ex.StackTrace);
            }
            return ok;
        }

        public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
        {
            return new Location(li.Line, li.Column, li.Index);
        }
    }
}
