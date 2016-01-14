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

    public class CompilerContext
    {
        public AssemblyWriter Asmw { get; set; }
        public Settings Options { get; set; }
        public string TempSrcFile { get; set; }
        public StreamReader InputSource { get; set; }
        public CompilerContext(Settings opt)
        {
            Options = opt;
            TempSrcFile = Path.GetTempFileName();
            
            Asmw = new AssemblyWriter(opt.Output);
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
       
            string tmp = Path.ChangeExtension( Options.Output,".ppvt");
            foreach (string src in Options.Sources)
            {
                  
                if(File.Exists(src))
                {
                    if(!Preprocessor.Paths.Contains(Path.GetDirectoryName(src)))
                             Preprocessor.Paths.Add(Path.GetDirectoryName(src));
                string code  = File.ReadAllText(src);
                File.WriteAllText(tmp, code);
                }
           
            }
            string outpfile = tmp + ".ppvt";
            Preprocessor.PreprocessInclude(tmp, outpfile, Options.PreprocessLevel);

            InputSource = new StreamReader(File.OpenRead(outpfile));
            return true;
        }
        public bool ResolveSemanticTree(GlobalSequence<Global> globals,ref ResolveContext RootCtx, ref List<Declaration> Resolved, ref List<ResolveContext> ResolveCtx)
        {
            foreach (Global stmts in globals)
            {

                ResolveContext old_ctx = RootCtx;


                RootCtx = ResolveContext.CreateRootContext(stmts.Used, stmts.Namespace, stmts.Declarations);
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
                            (stmt.BaseDeclaration as PreprocessorDeclaration).Preprocess(ref declsofpp);
                            if (declsofpp != null)
                                foreach (Declaration d in declsofpp)
                                    Preprocessed.Add(d);
                        }
                        else Preprocessed.Add(stmt);
                    }
                  
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
            try{
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VTC.VATU.cgt"))
            {


                // compile/run each statement one entry at a time
                CompiledGrammar grammar = CompiledGrammar.Load(stream);
                var actions = new SemanticTypeActions<SimpleToken>(grammar);


                var processor = new SemanticProcessor<SimpleToken>(InputSource, actions);

                ParseMessage parseMessage = processor.ParseAll();
                if (parseMessage == ParseMessage.Accept)
                {
                    var globals = processor.CurrentToken as GlobalSequence<Global>;
                    ResolveContext RootCtx = null;
                    List<Declaration> Resolved = new List<Declaration>();
                    List<ResolveContext> ResolveCtx = new List<ResolveContext>();


                    ok &= ResolveSemanticTree(globals,ref RootCtx,ref Resolved,ref ResolveCtx);
                    if (ok) // Emit
                    {
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
                            Console.WriteLine("At index: {0} [{1}] {2},{3} {4}", token.Position.Index, parseMessage, token.Position.Line, token.Position.Column, token.Symbol.Name);
                }
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
            return new Location(li.Line, li.Column);
        }
    }
}
