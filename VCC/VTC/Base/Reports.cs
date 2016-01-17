using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VTC.Core;

namespace VTC
{
    public enum ErrorType
    {
        Error,
        Warning
    };

    /// <summary>
    /// Simple Errors Store
    /// </summary>
    public class Error
    {
        public int ID;
        public string Description;
        public int Line;
        public ErrorType Type;
        public string File;
        public int Col;
        public Error() { }
        public Error(int errid, string desc, int line,int col,string file, ErrorType errtype)
        {
            File = file;
            Col = col;
            ID = errid;
            Description = desc;
            Line = line;
            Type = errtype;
        }
    }
    public class ListReporter : Report
    {


        public List<Error> Errors;
        public ListReporter()
            : base()
        {
            Errors  = new List<Error>();
            quiet = true;
        }

        public override void AssembleFile(string file, string listing,
                                  string target, string output)
        {
            if (quiet)
                return;
        
        }

        public override void Error(Location location, string message)
        {
            error_count++;
            if (!location.IsNull)
                Errors.Add(new VTC.Error(0,message,location.Row,location.Column,FilePath, ErrorType.Error));
            else Errors.Add(new VTC.Error(0, message, 0,0, "default",ErrorType.Error));
        }
        public override void Error(int code, Location location, string message)
        {
            error_count++;
            if (!location.IsNull)
                Errors.Add(new VTC.Error(code, message, location.Row, location.Column, FilePath, ErrorType.Error));
            else Errors.Add(new VTC.Error(code, message, 0, 0, "default", ErrorType.Error));
        }
        public override void Warning(Location location, string message)
        {
            if (!location.IsNull)
                Errors.Add(new VTC.Error(0, message, location.Row, location.Column, FilePath, ErrorType.Warning));
            else Errors.Add(new VTC.Error(0, message, 0, 0, "default", ErrorType.Warning));
        }

        public override void Message(string message)
        {
            if (quiet)
                return;
       
        }


    }
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


          
               Console.Error.WriteLine("Error:VC{4}:{0}:{1},{2}:{3}",FilePath, location.Row, location.Column, message, "0000");
            }
            else Console.Error.WriteLine("Error:VC0000:default:0,0:{0}",  message);
        }
        public override void Error(int code,Location location, string message)
        {
            error_count++;
            if (!location.IsNull)
            {



                Console.Error.WriteLine("Error:VC{4}:{0}:{1},{2}:{3}", FilePath, location.Row, location.Column, message, code.ToString("0000"));
            }
            else Console.Error.WriteLine("Error:VC{1}:{0}", message, code.ToString("0000"));
        }
        public override void Warning(Location location, string message)
        {
            if (!location.IsNull)
                Console.Error.WriteLine("Warning:{0}:{1},{2}:{3}", FilePath, location.Row, location.Column, message);

            else Console.Error.WriteLine("Warning:{0}", message);
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
        public override void Error(int code,Location location, string message)
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