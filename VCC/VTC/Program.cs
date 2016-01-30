﻿using System;
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
        static bool Compile(CompilerContext ctx)
        {
            if (ctx.Options.OutputFiles.Length != ctx.Options.AssemblyOutput.Length || (ctx.Options.AssemblyOutput.Length != ctx.Options.Sources.Length))
            {
                ResolveContext.Report.Error("Sources, Assemblies and outputs must have the same count");
                return false;
            }
            if (ctx.ResolveAndEmit())
            {
                
                Console.WriteLine("Compilation succeeded - {0} Optimizations performed", Optimizer.Optimizations);
                if (ctx.Options.Target == Target.flat || ctx.Options.Target == Target.tiny || ctx.Options.Target == Target.vexe)
                {
                    for(int i = 0; i < ctx.Options.OutputFiles.Length; i++)
                        Compile(ctx.Options.AssemblyOutput[i], ctx.Options.OutputFiles[i], "-f bin");

                }
                else
                {
                    for (int i = 0; i < ctx.Options.OutputFiles.Length; i++)
                        Compile(ctx.Options.AssemblyOutput[i], ctx.Options.OutputFiles[i], "-f elf");
                }
                return true;
            }
            return false;
        }
        static int Main(string[] args)
        {
            var options = new Settings(); 
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
                CompilerContext ctx = new CompilerContext(options);
                if (Compile(ctx))
                    return 0;
              
             
            }

       
            return 1;
        }
        static void Compile(string outsrc, string outbin,string target)
        {
          
            Process compiler = new Process();
            compiler.StartInfo.FileName = "nasm.exe";
            compiler.StartInfo.Arguments = string.Format(" \"{0}\" {2} -o \"{1}\"", outsrc, outbin,target);
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.Start();

            Console.WriteLine(compiler.StandardOutput.ReadToEnd());

            compiler.WaitForExit();
        }
    }
}
