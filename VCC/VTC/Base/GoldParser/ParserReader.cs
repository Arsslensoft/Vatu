using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VTC.Base.GoldParser
{
    public sealed class ParserStringReader : ParserReader
    {
        public ParserStringReader(string text)
            : base(new MemoryStream(Encoding.UTF8.GetBytes(text)))
        {

        }
        public ParserStringReader(Stream str)
            : base(str)
        {

        }
    }
   public  class ParserReader : StreamReader
    {
       public string Filename { get; set; }
       public ParserReader(Stream str)
           : base(str)
       {

       }
       public ParserReader(string file)
           : base(file)
       {

       }
    }
}
