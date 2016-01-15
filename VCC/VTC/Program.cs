using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Vasm.Optimizer;

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
                {
                    Console.WriteLine("Compilation succeeded - {0} Optimizations performed", Optimizer.Optimizations);
                    Process.Start("nasm", string.Format(" \"{0}\" -f bin -o \"{1}\"", options.Output, options.OutputBinary));
                }
            }
            Console.Read();
        }
    }
}
