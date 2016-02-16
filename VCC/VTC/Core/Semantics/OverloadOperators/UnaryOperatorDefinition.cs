using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class UnaryOperatorDefinition : SimpleToken
    {

       public UnaryOp Operator { get; set; }
       [Rule(@"<Unary Operator> ::= '++'")]
       [Rule(@"<Unary Operator> ::= '--'")]
       [Rule(@"<Unary Operator> ::= '[]'")]
       [Rule(@"<Unary Operator> ::= '~'")]
       [Rule(@"<Unary Operator> ::= '?!'")]
       [Rule(@"<Unary Operator> ::= '??'")]
       [Rule(@"<Unary Operator> ::= new")]
       [Rule(@"<Unary Operator> ::= delete")]
       public UnaryOperatorDefinition(UnaryOp op)
       {
           Operator = op;
       }

    }
}
