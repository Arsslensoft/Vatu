using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{

    [Terminal("IncludeLiteral")]
    public class IncludeLiteral : Literal
    {
        public string IncludeFile { get; set; }
        public IncludeLiteral(string value)
            : base(value)
        {
           
            IncludeFile = value.Remove(0, 1).Remove(value.Length - 2, 1);
        }

     

    }
}
