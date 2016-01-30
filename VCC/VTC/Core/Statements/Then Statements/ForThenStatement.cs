using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ForThenStatement : ThenStatement
    {

        public ForStatement For { get; set; }

        [Rule("<Then Stm>   ::=  ~for ~'('  <PARAM EXPR> ~';' <Expression> ~';'  <PARAM EXPR> ~')' <Then Stm>")]
        public ForThenStatement(ParameterSequence<Expr> init, Expr cond, ParameterSequence<Expr> inc, ThenStatement stmt)
        {
            For = new ForStatement(init, cond, inc, stmt);


        }
       public override bool Resolve(ResolveContext rc)
        {
            return For.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            return For.DoResolve(rc);
        }
    
        public override bool Emit(EmitContext ec)
        {
            return For.Emit(ec);
        }
   
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return For.DoFlowAnalysis(fc);
        }
    }
    
	
}