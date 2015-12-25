
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
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


        [Option('o', "optimize", Required = false, DefaultValue = false,
   HelpText = "Enable optimizations")]
        public bool Optimize { get; set; }

               [Option('l', "optimizelevel", Required = false, DefaultValue = 2,
   HelpText = "Optimizations Level")]
        public int OptimizeLevel { get; set; }

            [Option('w', "warnaserror", Required = false, DefaultValue = false,
   HelpText = "Enable optimizations")]
               public bool WarningsAreErrors { get; set; }


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
