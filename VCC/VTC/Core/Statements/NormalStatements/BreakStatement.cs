using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class BreakStatement : NormalStatment
    {
        string Exit {get;set;}
        [Rule("<Normal Stm> ::= ~break ~';'")]
        public BreakStatement()
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if ((rc.CurrentScope & ResolveScopes.Case) == ResolveScopes.Case)
            {
                if (rc.EnclosingIf == null)
                    ResolveContext.Report.Error(37, Location, "Break must be used inside a case statement");
                else Exit = rc.EnclosingIf.ExitIf.Name;
            }
            else if (rc.EnclosingLoop == null)
                ResolveContext.Report.Error(37, Location, "Break must be used inside a loop statement");
            else
            {
                Exit = rc.EnclosingLoop.ExitLoop.Name;
                rc.EnclosingLoop.HasBreak = true;
            }
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
         
            return FlowState.Unreachable;
        }
        public override bool Emit(EmitContext ec)
        {
            if(Exit != null)
            ec.EmitInstruction(new Jump() { DestinationLabel = Exit });
            return true;
        }
 
       
    }
    
	
}