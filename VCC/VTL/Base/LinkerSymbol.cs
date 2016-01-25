using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
    public class LinkerSymbol<T> where T : struct
    {
        public LinkerSymbol(string name, T adr)
        {
            Address = adr;
            Name = name;
        }
        public string Name { get; set; }
        public T Address { get; set; }
    }
}
