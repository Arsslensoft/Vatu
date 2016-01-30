using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	[Terminal("BooleanLiteral")]
    public class BooleanLiteral : Literal
    {
       
        public BooleanLiteral(string value)
            : base(value)
        {
            _value = new BoolConstant(bool.Parse(value), CompilerContext.TranslateLocation(position));
        }



    }
   
	
	
}