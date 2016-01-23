using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	 [Terminal("StringLiteral")]
    public class StringLiteral : Literal
    {
     
        public StringLiteral(string value) 
            : base(value)
        {
            _value = new StringConstant(value.Remove(0,1).Remove(value.Length -2,1), CompilerContext.TranslateLocation(position));

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
        }
      
    }
    
	
}