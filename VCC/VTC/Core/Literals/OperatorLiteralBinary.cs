using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	[Terminal("OperatorLiteralBinary")]
    public class OperatorLiteralBinary : Literal
    {
        public bsn.GoldParser.Grammar.Symbol Sym
        {
            get { return symbol; }
        }
        public OperatorLiteralBinary(string value)
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