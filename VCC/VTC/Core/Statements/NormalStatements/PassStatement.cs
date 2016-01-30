using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class PassStatement : NormalStatment
    {
        string Exit { get; set; }
        [Rule("<Normal Stm> ::= ~pass ~';'")]
        public PassStatement()
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
          
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
     
            return true;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
    }
   
	
}