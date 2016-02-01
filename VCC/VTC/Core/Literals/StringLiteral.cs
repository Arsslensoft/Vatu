using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	 [Terminal("StringLiteral")]
    public class StringLiteral : Literal
    {

         public string StrVal { get; set; }
        public StringLiteral(string value) 
            : base(value)
         {
             if (value.StartsWith("@"))
             {
                 StrVal = value.Remove(0, 2);
                 StrVal = StrVal.Remove(StrVal.Length - 1, 1);
                 _value = new StringConstant(StrVal, CompilerContext.TranslateLocation(position),true);
             }
             else
             {
                 StrVal = value.Remove(0, 1);
                 StrVal = StrVal.Remove(StrVal.Length - 1, 1);
                 _value = new StringConstant(StrVal, CompilerContext.TranslateLocation(position));
             }
         

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
        }
      
    }
    
	
}