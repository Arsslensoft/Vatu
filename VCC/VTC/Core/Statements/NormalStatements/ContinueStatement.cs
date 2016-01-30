using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ContinueStatement : NormalStatment
    {
        string Condition { get; set; }
        [Rule("<Normal Stm> ::= ~continue ~';'")]
        public ContinueStatement()
        {

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if ((rc.CurrentScope & ResolveScopes.Case) == ResolveScopes.Case)
            {
                if (rc.EnclosingIf == null)
                    ResolveContext.Report.Error(37, Location, "Continue must be used inside a case statement");
                else Condition = rc.EnclosingIf.Else.Name;
            }else if (rc.EnclosingLoop == null)
                ResolveContext.Report.Error(37, Location, "Continue must be used inside a loop statement");
            else Condition = rc.EnclosingLoop.LoopCondition.Name;
            return this;
        }
    
        public override bool Emit(EmitContext ec)
        {
            if (Condition != null)
                ec.EmitInstruction(new Jump() { DestinationLabel = Condition });
            return true;
        }
    }
    
	
}