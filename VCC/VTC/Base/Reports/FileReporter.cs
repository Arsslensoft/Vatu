using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VTC
{
    public class FileReporter : Report
    {
        private StreamWriter str;


        public FileReporter(string file)
            : base()
        {
            str = new StreamWriter(file, false);
        }

        public override void AssembleFile(string file, string listing,
                                  string target, string output)
        {
            if (quiet)
                return;
            str.WriteLine("Assembling '{0}' , {1}, to {2} --> '{3}'", file,
                               GetListing(listing), target, output);
        }

        public override void Error(Location location, string message)
        {
            error_count++;
            str.WriteLine("Error : '{0}' in line {1} :{2}", location.FullPath, location.Row, message);
        }
        public override void Error(int code, Location location, string message)
        {
            error_count++;
            str.WriteLine("Error VC{3}: '{0}' in line {1} :{2}", location.FullPath, location.Row, message, code.ToString("0000"));
        }
        public override void Warning(Location location, string message)
        {
            string location_str = " : ";
            if (!location.IsNull)
                location_str = " (" + location.Row + ", " + location.Column + ") : ";

            str.WriteLine(String.Format("{0}{1}Warning -- {2}",
                    (file_path != null ? file_path : ""), location_str, message));
        }

        public override void Message(string message)
        {
            if (quiet)
                return;
            str.WriteLine(message);
        }


    }
}
