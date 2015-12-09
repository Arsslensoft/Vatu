using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace VCC
{
    class Program
    {
        static void Run(string input)
        {

        
            // grab an execution context to be used for the entire 'session'

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.C-ANSI.cgt"))
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
                    if (stmts != null)
                    {
                        foreach (Declaration stmt in stmts)
                        {
                            // stmt.Execute(ctx);
                        }
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
        static void Init()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.C-ANSI.cgt"))
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
            Console.WriteLine("By Abd Al Moez Bouraoui and Arsslen Idadi");
            Init();
            Console.WriteLine("Runing..");
            string input = "void main() {int a = 0; uint c = 45;}";
            Run(input);



            Console.Read();

        }
    }
}
