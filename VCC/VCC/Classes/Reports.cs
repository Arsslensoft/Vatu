﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VCC
{
    public class ConsoleReporter : Report
    {

        private static int error_count;
        private static int mark_count;
        private static bool quiet;
        /* Current file being processed */
        private static string file_path;

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
            Console.Error.WriteLine("Error in file '{0}' in line {1} :{2}", location.SourceFile.Name, location.Row, message);
        }

        public override void Warning(Location location, string message)
        {
            string location_str = " : ";
            if (!location.IsNull)
                location_str = " (" + location.Row + ", " + location.Column + ") : ";

            Console.Error.WriteLine(String.Format("{0}{1}Warning -- {2}",
                    (file_path != null ? file_path : ""), location_str, message));
        }

        public override void Message(string message)
        {
            if (quiet)
                return;
            Console.WriteLine(message);
        }


    }
    public class FileReporter : Report
    {
        private StreamWriter str;
        private static int error_count;
        private static int mark_count;
        private static bool quiet;
        /* Current file being processed */
        private static string file_path;

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
            str.WriteLine("Error in file '{0}' in line {1} :{2}", location.SourceFile.Name, location.Row, message);
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