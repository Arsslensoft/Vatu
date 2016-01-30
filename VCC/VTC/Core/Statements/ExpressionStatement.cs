using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	public class ExpressionStatement : NormalStatment
    {

        private Expr _expr;

        [Rule("<Normal Stm> ::= <Expression> ~';' ")]
        public ExpressionStatement(Expr b)
        {
            _expr = b;

        }
   
       public override bool Resolve(ResolveContext rc)
        {


            return _expr.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
         
            _expr = (Expr)_expr.DoResolve(rc);
            if (!_expr.AcceptStatement)
                ResolveContext.Report.Error(0,Location,"This kind of expressions cannot be used as an expression statement");

            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            return _expr.Emit(ec);
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _expr.DoFlowAnalysis(fc);
        }
    }

}