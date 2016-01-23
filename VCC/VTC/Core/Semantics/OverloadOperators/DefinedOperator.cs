using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{

   public class DefinedOperator : SimpleToken
    {
       public BinaryOperatorDefinition Binary { get; set; }
       public UnaryOperatorDefinition Unary { get; set; }
       public bool IsBinary
       {
           get { return Binary != null; }
       }

        [Rule(@"<Operator>       ::=  <Binary Operator>")]
       public DefinedOperator(BinaryOperatorDefinition bin)
       {
           Binary = bin;

       }
        [Rule(@"<Operator>       ::=  <Unary Operator>")]
       public DefinedOperator(UnaryOperatorDefinition un)
       {
           Unary = un;
       }
    }
}
