
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public sealed class FileListAttribute : System.Attribute
    {
        public FileListAttribute(Type concreteType)
        {

        }

        public int MaximumElements { get; set; }
    }
    public enum Platform
    {
        Intel16,
        x86
    }
    public enum Target
    {
        flat,
        tiny,
        vexe,
        obj
    }
   public class Settings
    {

       [Option('v', "verbose", HelpText = "Print details during execution.",DefaultValue=false)]
         public bool Verbose { get; set; }

       [Option('g', "debug", HelpText = "Debug option", DefaultValue = false)]
       public bool Debug { get; set; }


       [Option('p', "platform", Required = false, DefaultValue = Platform.Intel16,
HelpText = "Platform")]
       public Platform Platform { get; set; }

       [Option('t', "target", Required = false, DefaultValue = Target.flat,
HelpText = "Target output")]
       public Target Target { get; set; }

        [Option( "optimize", Required = false, DefaultValue = false,
   HelpText = "Enable optimizations")]
        public bool Optimize { get; set; }

        [Option('p', "pplevel", Required = false, DefaultValue = 1,
 HelpText = "Preprocessing Level")]
        public int PreprocessLevel { get; set; }


               [Option('z', "optimizelevel", Required = false, DefaultValue = 2,
   HelpText = "Optimizations Level")]
        public int OptimizeLevel { get; set; }

            [Option("Werror", Required = false, DefaultValue = false,
   HelpText = "Enable optimizations")]
               public bool WarningsAreErrors { get; set; }

            [Option( "Wall", Required = false, DefaultValue = false,
    HelpText = "Warn all")]
            public bool WarnAll { get; set; }

            [Option("W", Required = false, DefaultValue = false,
  HelpText = "Enable Warnings.")]
            public bool Warn { get; set; }




       [OptionArray('i', "include", HelpText = "Include directory.")]
        public string[] Includes { get; set; }

       [OptionArray( "sym", HelpText = "Symbols.")]
       public string[] Symbols { get; set; }

       [OptionArray('l', "library", HelpText = "Object files directory.")]
       public string[] Libraries { get; set; }

       [OptionArray('s', "source", Required = true, HelpText = "Source files.")]
       public string[] Sources { get; set; }

       [Option('a', "asm", Required = false, DefaultValue = "",
HelpText = "Output assembly file")]
       public string Output { get; set; }
       
       [Option("boot", Required = false,  DefaultValue = false,
HelpText = "Bootloader")]
       public bool BootLoader { get; set; }

       [Option("int", Required = false, DefaultValue = false,
HelpText = "Interrupts definition")]
       public bool IsInterrupt { get; set; }

   
       [Option('o', "out", Required = false, DefaultValue = "",
HelpText = "Output file")]
       public string OutputBinary { get; set; }


       [Option("flow", Required = false, DefaultValue = false,
HelpText = "Do flow analysis")]
       public bool Flow { get; set; }
            [ParserState]
            public IParserState LastParserState { get; set; }
            [HelpOption]
            public string GetUsage()
            {
                var help =          HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

                help.Copyright = "Copyright (c) 2015 Arsslensoft Research. All rights reserved";
                help.AdditionalNewLineAfterOption = false;
                help.Heading = "Vatu Compiler";
                return help;

            }
    }
}
