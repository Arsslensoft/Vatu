using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VJay;

namespace VCC
{
    [Terminal("StringLiteral")]
    public class StringLiteral : StringConstant
    {
        private readonly string _value;
        public StringLiteral(string value, Location loc) 
            : base(value,loc)
        {
            _value = value.Substring(1, value.Length - 2);
        }


      
    }
}
