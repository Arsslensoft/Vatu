
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
    public sealed class FileListAttribute : System.Attribute
    {
        public FileListAttribute(Type concreteType)
        {

        }

        public int MaximumElements { get; set; }
    }
  
    public enum Target
    {
        flat,
        tiny,
        vexe
    }
   public class Settings
    {

       [Option('v', "verbose", HelpText = "Print details during execution.",DefaultValue=false)]
         public bool Verbose { get; set; }



       [Option('t', "target", Required = true, DefaultValue = Target.flat,
HelpText = "Target output")]
       public Target Target { get; set; }



        [Option('e', "entry", Required = false, DefaultValue = "___vatu_entry",
 HelpText = "Entry point name")]
        public string EntryPoint { get; set; }

        [Option( "origin", Required = false, DefaultValue = 0u,
HelpText = "Origin")]
        public uint Origin { get; set; }

               [Option('a', "align", Required = false, DefaultValue = 4u,
   HelpText = "Align")]
        public uint Align { get; set; }

               [OptionArray('l', "lib", Required = true, HelpText = "Object files.")]
               public string[] Libraries { get; set; }

            

       [Option("int", Required = false, DefaultValue = false,
HelpText = "Interrupts install before entry point")]
       public bool IsInterrupt { get; set; }

   
       [Option('o', "out", Required = true, DefaultValue = "",
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
                help.Heading = "Vatu Linker";
                return help;

            }
    }
}
