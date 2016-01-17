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
            if (ctx.ResolveAndEmit())
            {
                Console.WriteLine("Compilation succeeded - {0} Optimizations performed", Optimizer.Optimizations);
                Compile(ctx.Options.Output, ctx.Options.OutputBinary);
            }
        }
        static void Main(string[] args)
        {
            var options = new Settings(); 
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
                CompilerContext ctx = new CompilerContext(options);
                Compile(ctx);
             
            }
            Console.Read();
        }
        static void Compile(string outsrc, string outbin)
        {
          
            Process compiler = new Process();
            compiler.StartInfo.FileName = "nasm.exe";
            compiler.StartInfo.Arguments = string.Format(" \"{0}\" -f bin -o \"{1}\"", outsrc, outbin);
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.Start();

            Console.WriteLine(compiler.StandardOutput.ReadToEnd());

            compiler.WaitForExit();
        }
    }
}
