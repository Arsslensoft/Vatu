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

      public override SimpleToken DoResolve(ResolveContext rc)
      {
          
          if (!_checked)
          {
              rc.CreateNewState();
              rc.CurrentGlobalScope &= ~ResolveScopes.CheckedArithmetics; // uncheked
              _expr = (Expr)_expr.DoResolve(rc);
              rc.RestoreOldState();
          }
          else
          {
              rc.CreateNewState();
              rc.CurrentGlobalScope |= ResolveScopes.CheckedArithmetics; // cheked
              _expr = (Expr)_expr.DoResolve(rc);
              rc.RestoreOldState();
          }
          return _expr;
      }
      public override bool Resolve(ResolveContext rc)
      {
          return _expr.Resolve(rc);
      }
    }
}
