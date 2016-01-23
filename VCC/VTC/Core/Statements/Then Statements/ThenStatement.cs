using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ThenStatement : NormalStatment
    {
        NormalStatment ns;
        public ThenStatement()
        {

        }
        [Rule(@"<Then Stm>   ::=  <Normal Stm>")]
        public ThenStatement(NormalStatment normal)
        {
            ns = normal;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (ns != null)
                return ns.DoResolve(rc);
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ns != null)
                return ns.Emit(ec);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (ns != null)
                return ns.Resolve(rc);
            else return true;
        }

        public override Reachability MarkReachable(Reachability rc)
        {
            if (ns != null)
                return ns.MarkReachable(rc);

            return base.MarkReachable(rc);
        }
        public override bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (ns != null)
                return ns.DoFlowAnalysis(fc);
            return base.DoFlowAnalysis(fc);
        }
    }

	
}