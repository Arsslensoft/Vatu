using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
{
    public class Namespace
    {
    
        public string Name { get; set; }
        public static Namespace Default = new Namespace("@global@");
        public bool IsDefault { get { return Name == "@global@"; } }
        public Namespace(string name)
        {
            Name = name;
      
        }
        public string Normalize()
        {
            return Name.Replace(".", "_");
        }
        public override string ToString()
        {
            return "[Namespace: "+Name+"]";
        }
    }
}
