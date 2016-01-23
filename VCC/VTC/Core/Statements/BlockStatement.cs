using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class BlockStatement : NormalStatment
    {

        private Block _bloc;

        [Rule("<Normal Stm> ::= <Block>")]
        public BlockStatement(Block b)
        {
            _bloc = b;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _bloc = (Block)_bloc.DoResolve(rc);
            return _bloc;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _bloc.Resolve(rc);
            return base.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            _bloc.Emit(ec);
            return base.Emit(ec);
        }
        public override Reachability MarkReachable(Reachability rc)
        {
             base.MarkReachable(rc);
             return _bloc.MarkReachable(rc);
        }
        public override bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _bloc.DoFlowAnalysis(fc);
        }
    }
    
	
}