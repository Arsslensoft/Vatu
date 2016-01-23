using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	public class ArrayVariableDefinition : Definition
    {
        public int Size { get; set; }


        Expr _expr;
        [Rule(@"<Array>    ::= ~'[' <Expression> ~']'")]
        public ArrayVariableDefinition(Expr expr)
        {
            _expr = expr;
        }

        [Rule(@"<Array>    ::= ~'[]'")]
        public ArrayVariableDefinition()
        {
            _expr = null;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bool conv = false;
            if (_expr != null)
            _expr = (Expr)_expr.DoResolve(rc);

            if (_expr != null && _expr is ConstantExpression)
                Size = int.Parse((((ConstantExpression)_expr).ConvertImplicitly(rc, BuiltinTypeSpec.Int, ref conv)).GetValue().ToString());
            else Size = 0;

            if (Size < 0)
                ResolveContext.Report.Error(47, Location, "Invalid array size");
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
      if (_expr != null)
            return _expr.Resolve(rc);
      return true;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
   
}