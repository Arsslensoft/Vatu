using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class LoopStatement : BaseStatement, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }
        public bool HasBreak { get; set; }


        Statement _stmt;
 
        [Rule(@"<Statement>        ::= ~loop <Statement>")]
        public LoopStatement( Statement stmt)
        {

            _stmt = stmt;
        }

     
       public override bool Resolve(ResolveContext rc)
        {
            return _stmt.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CurrentScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.LOOP);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;

            _stmt = (Statement)_stmt.DoResolve(rc);
        

            rc.CurrentScope &= ~ResolveScopes.Loop;
            // exit current loop
            rc.EnclosingLoop = ParentLoop;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
     
            ec.MarkLabel(EnterLoop);

            _stmt.Emit(ec);
            ec.MarkLabel(LoopCondition);
            ec.EmitInstruction(new Jump() {  DestinationLabel = EnterLoop.Name });
            ec.MarkLabel(ExitLoop);
            return true;
        }
       

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {

            FlowState ok = FlowState.Valid;
          ok &= _stmt.DoFlowAnalysis(fc);
 


            if (!HasBreak)
                return FlowState.Unreachable;
            return ok;
        }
    }
    
	
}