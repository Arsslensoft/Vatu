using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
    class Program
    {
        static int Main(string[] args)
        {Linker lnk=null;
             var options = new Settings();
             if (CommandLine.Parser.Default.ParseArguments(args, options))
             {
                 try
                 {
                     if (options.Target == Target.flat)
                         lnk = new FlatLinker(options);
                     else if (options.Target == Target.tiny)
                         lnk = new TinyDosLinker(options);
                     else
                         lnk = new VatuExecutableLinker(options);


                 
                     List<Link<uint>> links = lnk.DoMatchSymbols();
                     lnk.RelocateAll(links);
                     lnk.Link();
                     lnk.Close();
                 }
                 catch (Exception ex)
                 {
                     if(ex.Message.StartsWith("VL00"))
                              Console.WriteLine(ex.Message);
                     else Console.WriteLine("VL0000:global:"+ex.Message);
                     return 1;
                 }
                 return 0;
             }
             return 1;
        }
    }
}
