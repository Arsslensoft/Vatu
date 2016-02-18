using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	public class RegisterExpressionStatement : NormalStatment
    {

        private RegisterExpr _expr;

        [Rule("<Normal Stm> ::= <Register Expression> ~';' ")]
        public RegisterExpressionStatement(RegisterExpr b)
        {
            _expr = b;

        }
       
       public override bool Resolve(ResolveContext rc)
        {


            return _expr.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {

            _expr = (RegisterExpr)_expr.DoResolve(rc);
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