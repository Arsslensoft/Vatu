using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core.Expressions
{
  public  class CheckedExpressions : Expr
    {
      Expr _expr;
      bool _checked = false;
      [Rule("<Value> ::= checked ~'(' <Expression> ~')' ")]
      [Rule("<Value> ::= unchecked ~'(' <Expression> ~')' ")]
      public CheckedExpressions(SimpleToken tok,Expr exp)
      {
          _expr = exp;
          _checked = (tok.Name == "checked");
      }
    }
}
