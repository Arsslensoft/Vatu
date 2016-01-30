using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class FunctionExtensionDefinition : Definition
		{
			public TypeSpec ExtendedType { get; set; }
            public bool IsExtended = true;
			public bool Static = false;
			TypeToken tt;
			[Rule(@"<Func Ext>  ::= ~extends <Type>")]
			public FunctionExtensionDefinition(TypeToken t)
		  {
			  tt = t;
		  }
			[Rule(@"<Func Ext>  ::= static ~extends <Type>")]
			public FunctionExtensionDefinition(SimpleToken st,TypeToken t)
			{
				tt = t; Static = true;
			}
            [Rule(@"<Func Ext>  ::= ")]
            public FunctionExtensionDefinition()
            {
                tt = null;
                IsExtended = false;
            }
			public override SimpleToken DoResolve(ResolveContext rc)
			{
                if (tt != null)
                {
                    tt = (TypeToken)tt.DoResolve(rc);
                    ExtendedType = tt.Type;

                    return this;
                }
                return null;
			}
		}

	
}