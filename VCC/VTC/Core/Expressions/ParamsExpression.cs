using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core
{


    public class ParamsExpression : VariableExpression
    {
        [Rule(@"<Var Expr>     ::= params")]
        public ParamsExpression(SimpleToken t)
            : base("params")
        {
            position = t.position;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!rc.CurrentMethod.IsVariadic)
                ResolveContext.Report.Error(0, Location, "Params expression can't be used with non variadic functions");
            return base.DoResolve(rc);
        }
    }
}
