
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
   public class Settings
    {

       [Option('v', "verbose", HelpText = "Print details during execution.",DefaultValue=false)]
         public bool Verbose { get; set; }


       [Option('p', "platform", Required = false, DefaultValue = Platform.Intel16,
HelpText = "Enable optimizations")]
       public Platform Platform { get; set; }


        [Option( "optimize", Required = false, DefaultValue = false,
   HelpText = "Enable optimizations")]
        public bool Optimize { get; set; }

               [Option('z', "optimizelevel", Required = false, DefaultValue = 2,
   HelpText = "Optimizations Level")]
        public int OptimizeLevel { get; set; }

            [Option('w', "warnaserror", Required = false, DefaultValue = false,
   HelpText = "Enable optimizations")]
               public bool WarningsAreErrors { get; set; }
          
       [OptionArray('i', "include", HelpText = "Include directory.")]
        public string[] Includes { get; set; }

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

       [Option("flat", Required = false, DefaultValue = false,
HelpText = "Flat output")]
       public bool IsFlat { get; set; }

       [Option("ovrl", Required = false, DefaultValue = true,
HelpText = "Enable method overload")]
       public bool Overload { get; set; }


       [Option('o', "out", Required = false, DefaultValue = "",
HelpText = "Output file")]
       public string OutputBinary { get; set; }
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
