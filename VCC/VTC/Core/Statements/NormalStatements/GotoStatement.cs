using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	 public class GotoStatement : NormalStatment
    {
        public Label DestinationLabel { get; set; }
        public bool Switch { get; set; }
        int CaseId;
        private Identifier _id;

        [Rule("<Normal Stm> ::= ~goto Id ~';'")]
        public GotoStatement(Identifier id)
        {
            Switch = false;
            _id = id;

        }

        [Rule("<Normal Stm> ::= ~goto ~case DecLiteral ~';'")]
        public GotoStatement(DecLiteral id)
        {
            Switch = true;
            CaseId = int.Parse(id.Value.GetValue().ToString());
            if (CaseId < 0)
                CaseId = -1;
            _id = null;

        }
        [Rule("<Normal Stm> ::= ~goto ~default ~';'")]
        public GotoStatement()
        {
            Switch = true;
            CaseId = -1;
            _id = null;

        }
       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if(_id != null)
               DestinationLabel = new Label(_id.Name);
            if (Switch && rc.EnclosingSwitch != null)
            {
                if (CaseId >= 0)
                   DestinationLabel= rc.EnclosingSwitch.ResolveCase(CaseId,false);
                else DestinationLabel = rc.EnclosingSwitch.ResolveCase(0,true);
            }
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            ec.EmitInstruction(new Vasm.x86.Jump() { DestinationLabel = this.DestinationLabel.Name });

            return true;
        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
    }
    
}