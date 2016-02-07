using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vasm.Optimizer;

namespace VTC
{
    class Program
    {
        static bool Compile(CompilerContext ctx)
        {
            string asmolvl = "x";
            if (ctx.Options.OptimizeLevel <= 2)
                asmolvl = ctx.Options.OptimizeLevel.ToString();

            if (ctx.Options.OutputFiles.Length != ctx.Options.AssemblyOutput.Length || (ctx.Options.AssemblyOutput.Length != ctx.Options.Sources.Length))
            {
                ResolveContext.Report.Error("Sources, Assemblies and outputs must have the same count");
                return false;
            }
            if (ctx.ResolveAndEmit())
            {
                bool ok = true;
                if (ctx.Options.Target == Target.flat || ctx.Options.Target == Target.tiny || ctx.Options.Target == Target.vexe)
                {
                    for(int i = 0; i < ctx.Options.OutputFiles.Length; i++)
                       ok= Compile(ctx.Options.AssemblyOutput[i], ctx.Options.OutputFiles[i], "-f bin -O"+asmolvl);

                }
                else
                {
                    for (int i = 0; i < ctx.Options.OutputFiles.Length; i++)
                        ok = Compile(ctx.Options.AssemblyOutput[i], ctx.Options.OutputFiles[i], "-f elf -O" + asmolvl);
                }


                if (Optimizer.Optimizations > 0)
                    Console.WriteLine("Compilation {0} - {1} Optimizations performed",ok?"succeeded":"failed", Optimizer.Optimizations);
                else Console.WriteLine("Compilation {0}", ok ? "succeeded" : "failed");
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
      static  Regex sp = new Regex(@"^\s*(?<file>.*):(?<line>\d*):(?<kind>.*):(?<message>.*)");
       static void ParseNasmErrors(string stderr)
        {
            foreach (string ln in stderr.Split('\n'))
            {
              MatchCollection  mc =  sp.Matches(ln);
              foreach (Match m in mc)
              {
                  if (m.Success)
                  {
                      Location loc = new Location(int.Parse(m.Groups["line"].Value), 0, 0);
                      loc.FullPath = m.Groups["file"].Value;
                      if (m.Groups["kind"].Value == " error")
                          ResolveContext.Report.Error(0, loc, m.Groups["message"].Value);
                      else ResolveContext.Report.Warning(loc, m.Groups["message"].Value);
                  }
              }
            }
        }
        static bool Compile(string outsrc, string outbin,string target)
        {
          
            Process compiler = new Process();
            compiler.StartInfo.FileName = "nasm.exe";
            compiler.StartInfo.Arguments = string.Format(" \"{0}\" {2} -o \"{1}\"", outsrc, outbin,target);
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.Start();

            string err = compiler.StandardError.ReadToEnd
                ();
          
            compiler.WaitForExit();
            ParseNasmErrors(err);
            return compiler.ExitCode == 0;
        }
    }
}
