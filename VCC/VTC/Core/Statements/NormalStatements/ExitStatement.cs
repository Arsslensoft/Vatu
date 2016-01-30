using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	public class ExitStatement : NormalStatment
    {
        string Exit { get; set; }
        int lvl;
        [Rule("<Normal Stm> ::= ~exit ~';'")]
        public ExitStatement()
        {
            lvl = -1;
        }
        [Rule("<Normal Stm> ::= ~exit DecLiteral ~';'")]
        public ExitStatement(DecLiteral decl)
        {
            lvl = int.Parse(decl.Value.GetValue().ToString());
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (rc.EnclosingLoop == null)
                ResolveContext.Report.Error(37, Location, "Exit must be used inside a if statement");
            else
            {
                if (lvl == -1)
                {
                    ILoop enc = rc.EnclosingLoop;
                    while (enc != null)
                    {
                        Exit = enc.ExitLoop.Name;
                        enc = enc.ParentLoop;
                    }
                }
                else
                {
                    ILoop enc = rc.EnclosingLoop;
                    while (enc != null && lvl > 0)
                    {
                        Exit = enc.ExitLoop.Name;
                        enc = enc.ParentLoop;
                        lvl--;
                    }
                    if (lvl > 0 && enc == null)
                        ResolveContext.Report.Error(37, Location, "Exit level must be below the enclosing loop count statement");
                }
            }
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return FlowState.Unreachable;
        }
        public override bool Emit(EmitContext ec)
        {
            if (Exit != null)
                ec.EmitInstruction(new Jump() { DestinationLabel = Exit });
            return true;
        }
        
    }
    
}