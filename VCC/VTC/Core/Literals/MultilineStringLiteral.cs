using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class MultilineStringLiteral : Literal
    {

      [Rule("<Multiline String Literals> ::= StringLiteral")]
       public MultilineStringLiteral(StringLiteral sl)
           :base("")
       {
           _value=  (StringConstant)sl.Value;
       }
        [Rule("<Multiline String Literals> ::= StringLiteral <Multiline String Literals>")]
       public MultilineStringLiteral(StringLiteral sl,MultilineStringLiteral msl)
           : base("")
       {
           string val = sl.Value.GetValue().ToString() + msl.Value.GetValue().ToString();
           _value = new StringConstant(val, sl.Location);

       }

        public override SimpleToken DoResolve(ResolveContext rc)
        {

            return _value.DoResolve(rc);
        }
    }
}
