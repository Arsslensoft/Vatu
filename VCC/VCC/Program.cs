using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vasm;


namespace VCC.Core
{
    class Program
    {
        static void Run(string input)
        {

        
            // grab an execution context to be used for the entire 'session'
            using (AssemblyWriter asmw = new AssemblyWriter(Console.OpenStandardOutput(), Encoding.ASCII))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.VATU.cgt"))
                {


                    // compile/run each statement one entry at a time
                    CompiledGrammar grammar = CompiledGrammar.Load(stream);
                    var actions = new SemanticTypeActions<SimpleToken>(grammar);


                    var processor = new SemanticProcessor<SimpleToken>(new StringReader(input), actions);

                    ParseMessage parseMessage = processor.ParseAll();
                    if (parseMessage == ParseMessage.Accept)
                    {

                        Console.WriteLine("Ok.\n");

                        var stmts = processor.CurrentToken as DeclarationSequence<Declaration>;
                        ResolveContext RootCtx = ResolveContext.CreateRootContext(stmts);
                        if (stmts != null)
                        {
                            List<Declaration> Resolved = new List<Declaration>();
                            List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                            foreach (Declaration stmt in stmts)
                            {

                                if (stmt.BaseDeclaration is MethodDeclaration)
                                {
                                    MethodDeclaration md = (MethodDeclaration)stmt.BaseDeclaration;
                                    ResolveContext childctx = RootCtx.CreateAsChild(md);
                                    stmt.Resolve(childctx);
                                    MethodDeclaration d = (MethodDeclaration)md.DoResolve(childctx);
                                   // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                    RootCtx.UpdateFather(childctx);
                                    Resolved.Add(d);
                                    ResolveCtx.Add(childctx);
                                }
                                else if (stmt.BaseDeclaration is StructDeclaration)
                                {
                                    StructDeclaration md = (StructDeclaration)stmt.BaseDeclaration;
                                    ResolveContext childctx = RootCtx.CreateAsChild(md);
                                    stmt.Resolve(childctx);
                                    StructDeclaration d = (StructDeclaration)md.DoResolve(childctx);
                                    // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                    RootCtx.UpdateFather(childctx);
                                    Resolved.Add(d);
                                    ResolveCtx.Add(childctx);
                                }
                                else
                                {
                                    RootCtx.IsInTypeDef = stmt.IsTypeDef;
                                    RootCtx.IsInStruct = stmt.IsStruct;
                                    stmt.Resolve(RootCtx);
                                    ResolveCtx.Add(RootCtx);
                                    Declaration d = (Declaration)stmt.DoResolve(RootCtx);
                                    Resolved.Add(d);
                                }
                            }
                          int i = 0;
                          EmitContext ec = new EmitContext(asmw);
                            foreach (Declaration stmt in Resolved)
                            {
                             ec.SetCurrentResolve( ResolveCtx[i]);
                                stmt.Emit(ec);
                                i++;
                            }
                            Console.WriteLine("Asm Code : ");
                            ec.Emit();
                        }
                    }
                    else
                    {


                        IToken token = processor.CurrentToken;
                        Console.WriteLine("At index: {0} [{1}]", token.Position.Index, parseMessage);
                        //Console.WriteLine(string.Format("{0} {1}", "^".PadLeft(token.Position.Index + 1), parseMessage));
                    }

                }
            }
        }
        static void Init()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.VATU.cgt"))
            {
                CompiledGrammar grammar = CompiledGrammar.Load(stream);
                var actions = new SemanticTypeActions<SimpleToken>(grammar);
                try
                {
                    actions.Initialize(true);
                  
                }
                catch (InvalidOperationException ex)
                {
                    Console.Write(ex.Message);
                    Console.ReadKey(true);
                    return;
                }



            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Copyright (c) 2015 Arsslensoft Research. All rights reserved");
        
            Init();
            Console.WriteLine("Runing..");
           // string input = "int GLOBALVAR = 789; void main() {int a = 14599845; ushort v = 32762; bool x = false; char c = 5;}";
            string input = "struct CD { int cda;}; struct ABC {int ab; struct CD bc;}; int a(int x) {} struct ABC ez; entry void main(int g){int b; b +=2;}";
            Run(input);



            Console.Read();

        }
    }
}
