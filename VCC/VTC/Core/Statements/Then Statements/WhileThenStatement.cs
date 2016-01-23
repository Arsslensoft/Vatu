using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	public class WhileThenStatement : ThenStatement
    {
        public WhileStatement While { get; set; }
 

        [Rule("<Then Stm>   ::=  ~while ~'(' <Expression> ~')' <Then Stm>")]
        public WhileThenStatement(Expr exp, ThenStatement stmt)
        {
            While = new WhileStatement(exp, (Statement)stmt);


        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return While.DoResolve(rc);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return While.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return While.Emit(ec);
        }

        public override Reachability MarkReachable(Reachability rc)
        {
            base.MarkReachable(rc);
            return While.MarkReachable(rc);
        }
        public override bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return While.DoFlowAnalysis(fc);
        }
    }
    
}