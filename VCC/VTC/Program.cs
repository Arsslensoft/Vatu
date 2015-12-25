using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Settings(); 
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
              
            }
            Console.Read();
        }
    }
}
