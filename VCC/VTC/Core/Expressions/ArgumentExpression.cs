using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    /// <summary>
    /// <Arg>
    /// </summary>
    public class ArgumentExpression : Expr
    {
        public Expr argexpr;
        [Rule(@"<Arg>       ::= <Expression> ")]
        public ArgumentExpression(Expr expr)
        {
            argexpr = expr;
        }

        [Rule(@"<Arg>       ::= ")]
        public ArgumentExpression()
        {

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (argexpr != null)
            {
                argexpr = (Expr)argexpr.DoResolve(rc);
                Type = argexpr.Type;
                return argexpr;
            }

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (argexpr != null)
                return argexpr.Resolve(rc);
            else return false;
        }
        public override bool Emit(EmitContext ec)
        {
            if (argexpr != null)
                return argexpr.Emit(ec);
            return true;
        }
        public override bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (argexpr != null)
                return argexpr.DoFlowAnalysis(fc);
            else return base.DoFlowAnalysis(fc);
        }

    }
}
