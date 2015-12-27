using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    class Program
    {
        static void Compile(CompilerContext ctx)
        {

        }
        static void Main(string[] args)
        {
            var options = new Settings(); 
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
                CompilerContext ctx = new CompilerContext(options);
                if (ctx.ResolveAndEmit())
                    Console.WriteLine("Compilation succeeded");
            }
            Console.Read();
        }
    }
}
