using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public class ListReporter : Report
    {


        public List<Error> Errors;
        public ListReporter()
            : base()
        {
            Errors = new List<Error>();
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
                Errors.Add(new VTC.Error(0, message, location.Row, location.Column, FilePath, ErrorType.Error));
            else Errors.Add(new VTC.Error(0, message, 0, 0, "default", ErrorType.Error));
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
}
