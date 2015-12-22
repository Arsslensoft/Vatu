using Vasm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestAssembler
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string line in File.ReadAllLines(@"C:\Users\Arsslen Idadi\Desktop\OPCODES.txt"))
            {
                foreach (string tok in line.Split('|'))
                {
                   // Console.WriteLine(" [Rule(@\"<OPCODES> ::= "+tok.Trim()+"\")]");
                    Console.WriteLine(" [Terminal(\"" + tok.Trim() + "\")]");
                }
            }
            Console.Read();
        }
    }
}
