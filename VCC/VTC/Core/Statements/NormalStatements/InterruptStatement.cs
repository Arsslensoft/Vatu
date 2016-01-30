using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class InterruptStatement : NormalStatment
    {
 

        private ushort itr;
          
        [Rule("<Normal Stm> ::= ~interrupt <Integral Const> ~';' ")]
        public InterruptStatement(Literal b)
        {
            itr = ushort.Parse(b.Value.GetValue().ToString());

        }
       
       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {

            return this;
        }
        public override bool Emit(EmitContext ec)
        {
       
            ec.EmitInstruction(new Vasm.x86.INT() { DestinationValue = itr});
            return true;
        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
    }
    
	
}