using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	public class FunctionSpecifier : SimpleToken
    {
        public Specifiers Specs { get; set; }

        SimpleToken tt;
        [Rule(@"<Func Spec>  ::= isolated")]
        [Rule(@"<Func Spec>  ::= entry")]
        public FunctionSpecifier(SimpleToken t)
        {
            tt = t;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (tt.Name == "entry")
                Specs = Specifiers.Entry;

            else if (tt.Name == "isolated")
                Specs = Specifiers.Isolated;

            return this;
        }
    }

	
	
}