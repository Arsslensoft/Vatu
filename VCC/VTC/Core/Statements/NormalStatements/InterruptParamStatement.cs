using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class InterruptParamStatement : NormalStatment
    {
 

        private ushort itr;
        private List<Expr> _params;
        ParameterSequence<Expr> _p;
        [Rule("<Normal Stm> ::= ~interrupt <Integral Const> ~'(' <PARAM EXPR> ~')' ~';'")]
        public InterruptParamStatement(Literal b,ParameterSequence<Expr> p)
        {
            itr = ushort.Parse(b.Value.GetValue().ToString());
            _params = new List<Expr>();
            _p = p;
        }
       
       public override bool Resolve(ResolveContext rc)
        {
            for (int i = 0; i < _params.Count; i++)
                _params[i].Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            foreach (Expr e in _p)
                _params.Add((Expr)e.DoResolve(rc));
            
            if (_params.Count > 4)
                ResolveContext.Report.Error(0, Location, "Interrupt max parameters count is 4");

  
        
            return this;
        }
         
        public override bool Emit(EmitContext ec)
        {
            for (int i = 0; i < _params.Count; i++)
                _params[i].EmitToStack(ec);

            if (_params.Count == 4)
                ec.EmitPop(EmitContext.D);
            if (_params.Count >= 3)
                ec.EmitPop(EmitContext.C);
            if (_params.Count >= 2)
                ec.EmitPop(EmitContext.B);
            if (_params.Count >= 1)
                ec.EmitPop(EmitContext.A);

            ec.EmitInstruction(new Vasm.x86.INT() { DestinationValue = itr});
            return true;
        }
       

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            for (int i = 0; i < _params.Count; i++)
                _params[i].DoFlowAnalysis(fc);
            return base.DoFlowAnalysis(fc);
        }
    }
    
	
}