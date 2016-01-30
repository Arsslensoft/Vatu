using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	[Terminal("NullLiteral")]
    public class NullLiteral : Literal
    {
      
        public NullLiteral(string value)
            : base(value)
        {
            _value = new NullConstant(CompilerContext.TranslateLocation(position));
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
        }


    }
    
	
}