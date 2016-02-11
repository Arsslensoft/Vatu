using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core.Expressions
{
  public  class TypedExpressions : Expr
    {
     
      TypeToken _expr;

      [Rule("<Value> ::= ~type ~'(' <Type> ~')' ")]
      public TypedExpressions(TypeToken exp)
      {
          _expr = exp; // _checked = (tok.Name == "checked");
        
      }

      public override SimpleToken DoResolve(ResolveContext rc)
      {
          _expr = (TypeToken)_expr.DoResolve(rc);
          Type = BuiltinTypeSpec.Type;
          
          return this;
      }
      public override bool Emit(EmitContext ec)
      {
          ec.EmitPush(_expr.Type.TypeDescriptor);
          return true;
      }
      public override bool EmitToStack(EmitContext ec)
      {
          return Emit(ec);
      }

      public override bool Resolve(ResolveContext rc)
      {
          return _expr.Resolve(rc);
      }
    }
}
