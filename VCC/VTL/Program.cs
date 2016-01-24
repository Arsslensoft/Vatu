using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
    class Program
    {
        static int Main(string[] args)
        {
             var options = new Settings();
             if (CommandLine.Parser.Default.ParseArguments(args, options))
             {
                 try
                 {
                     Linker lnk = new Linker(options.OutputBinary, options.Libraries, options.Origin, options);
                     List<Link<uint>> links = lnk.DoMatchSymbols();
                     lnk.RelocateAll(links);
                     lnk.Link();
                     lnk.Close();
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.Message);
                 }
                 return 0;
             }
             return 1;
        }
    }
}
