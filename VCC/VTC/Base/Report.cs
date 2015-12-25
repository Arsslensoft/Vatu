using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public abstract class Report
    {
        protected static int error_count;
        protected static int mark_count;
        protected static bool quiet;
        /* Current file being processed */
        protected static string file_path;

        public Report()
        {
            error_count = 0;
            quiet = false;
        }

        protected string GetListing(string listing)
        {
            if (listing == null)
                return "no listing file";
            return listing;
        }

        public int ErrorCount
        {
            get { return error_count; }
        }

        public bool Quiet
        {
            get { return quiet; }
            set { quiet = value; }
        }

        public string FilePath
        {
            get { return file_path; }
            set { file_path = value; }
        }


        public void Error(string message)
        {
            Error(Location.Null, message);
        }


        public void Warning(string message)
        {
            Warning(Location.Null, message);
        }

        // Abstract Functions list

        public abstract void Error(Location location, string message);
        public abstract void Error(int code,Location location, string message);

        public abstract void Warning(Location location, string message);

        public abstract void Message(string message);

        public abstract void AssembleFile(string file, string listing,
                          string target, string output);

    }


    public class InternalErrorException : Exception
    {
        public InternalErrorException()
            : base("Internal error")
        {
        }

        public InternalErrorException(string message)
            : base(message)
        {
        }
    }
}
