using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class NextStatement : NormalStatment
    {
        string Exit { get; set; }
        [Rule("<Normal Stm> ::= ~next ~';'")]
        public NextStatement()
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (rc.EnclosingIf == null)
                ResolveContext.Report.Error(37, Location, "Next must be used inside a if statement");
            else if(rc.EnclosingIf.Else != null)
                Exit = rc.EnclosingIf.Else.Name;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (Exit != null)
                ec.EmitInstruction(new Jump() { DestinationLabel = Exit });
            return true;
        }
        public override Reachability MarkReachable(Reachability rc)
        {

            return base.MarkReachable(rc);
        }
    }
    
	
}