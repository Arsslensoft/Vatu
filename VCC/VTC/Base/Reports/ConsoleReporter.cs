using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public class ConsoleReporter : Report
    {



        public ConsoleReporter()
            : base()
        {

        }

        public override void AssembleFile(string file, string listing,
                                  string target, string output)
        {
            if (quiet)
                return;
            Console.WriteLine("Assembling '{0}' , {1}, to {2} --> '{3}'", file,
                               GetListing(listing), target, output);
        }

        public override void Error(Location location, string message)
        {
            error_count++;
            if (!location.IsNull)
            {



                Console.Error.WriteLine("Error:VC{4}:{0}:{1},{2}:{3}", FilePath, location.Row, location.Column, message, "0000");
            }
            else Console.Error.WriteLine("Error:0000:default:0,0:{0}", message);
        }
        public override void Error(int code, Location location, string message)
        {
            error_count++;
            if (!location.IsNull)
            {



                Console.Error.WriteLine("Error:{4}:{0}:{1},{2}:{3}", FilePath, location.Row, location.Column, message, code.ToString("0000"));
            }
            else Console.Error.WriteLine("Error:{1}:default:0,0:{0}", message, code.ToString("0000"));
        }
        public override void Warning(Location location, string message)
        {
            if (CompilerContext.CompilerOptions.WarningsAreErrors)
            {
                Error(0, location, message);
                return;
            }
            if (!location.IsNull)
                Console.Error.WriteLine("Warning:0:{0}:{1},{2}:{3}", FilePath, location.Row, location.Column, message);

            else Console.Error.WriteLine("Warning:0:default:0,0:{0}", message);
        }

        public override void Message(string message)
        {
            if (quiet)
                return;
            Console.WriteLine(message);
        }


    }
}
