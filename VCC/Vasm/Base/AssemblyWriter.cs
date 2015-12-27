using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vasm
{
    public class AssemblyWriter : StreamWriter
    {

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
        public AssemblyWriter(string file) : base(file,false)
        {

        }
        public AssemblyWriter(Stream str, Encoding enc)
            : base(str, enc)
        {

        }
    }
}
