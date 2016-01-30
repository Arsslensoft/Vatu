using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	 [Terminal("OperatorLiteralUnary")]
    public class OperatorLiteralUnary : Literal
    {
        public VTC.Base.GoldParser.Grammar.Symbol Sym
        {
            get { return symbol; }
        }
        public OperatorLiteralUnary(string value)
            : base("5")
        {
            string id = value;
          _value = new StringConstant(id, Location);
           
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
        }


    }
    
	
	
}