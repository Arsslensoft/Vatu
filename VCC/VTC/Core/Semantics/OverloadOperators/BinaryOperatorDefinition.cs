using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class BinaryOperatorDefinition : SimpleToken
    {

       public BinaryOp Operator { get; set; }
       [Rule("<Binary Operator> ::= '=='")]
       [Rule("<Binary Operator> ::=  '!='")]
       [Rule("<Binary Operator> ::=  '<='")]
       [Rule("<Binary Operator> ::=  '>='")]
       [Rule("<Binary Operator> ::=  '>'")]
       [Rule("<Binary Operator> ::=  '<'")]
       [Rule("<Binary Operator> ::=  '+'")]
       [Rule("<Binary Operator> ::=  '-'")]
       [Rule("<Binary Operator> ::=  '*'")]
       [Rule("<Binary Operator> ::=  '/'")]
       [Rule("<Binary Operator> ::=  '%'")]
       [Rule("<Binary Operator> ::=  '^'")]
       [Rule("<Binary Operator> ::=  '&'")]
       [Rule("<Binary Operator> ::=  '|'")]
       [Rule("<Binary Operator> ::=  '<<'")]
       [Rule("<Binary Operator> ::=  '>>'")]
       [Rule("<Binary Operator> ::=  '<~'")]
       [Rule("<Binary Operator> ::=  '~>'")]
       public BinaryOperatorDefinition(BinaryOp op)
       {
           Operator = op;
       }

      
    }
}
