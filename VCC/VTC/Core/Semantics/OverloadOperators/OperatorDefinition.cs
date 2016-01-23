using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
  public  class OperatorDefinition : Literal
    {

      public OperatorLiteralBinary Binary { get; set; }
      public OperatorLiteralUnary Unary { get; set; }
      public bool IsBinary
      {
          get { return Binary != null; }
      }

   [Rule(@"<Operator Def>      ::=  OperatorLiteralBinary")]
       public OperatorDefinition(OperatorLiteralBinary bin) : base("")
       {
           Binary = bin;

       }
        [Rule(@"<Operator Def>      ::=  OperatorLiteralUnary")]
   public OperatorDefinition(OperatorLiteralUnary un)
       : base("")
       {
           Unary = un;
       }
   
    }
}
