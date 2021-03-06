using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class WhileStatement : BaseStatement, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }
        public bool HasBreak { get; set; }
     

        Expr _expr;
       Statement _stmt;

        [Rule("<Statement>    ::=  ~while ~'(' <Expression> ~')' <Statement>")]
        public WhileStatement(Expr exp, Statement stmt)
        {
            _expr = exp;
            _stmt = stmt;


        }

       public override bool Resolve(ResolveContext rc)
        {
            return _expr.Resolve(rc) && _stmt.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CreateNewState();
            rc.CurrentGlobalScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.WHILE);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;
       

            _expr = (Expr)_expr.DoResolve(rc);

            if (_expr.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(30, Location, "Loop condition must be a boolean expression");

            _stmt = (Statement)_stmt.DoResolve(rc);


            rc.RestoreOldState();
            // exit current loop
            rc.EnclosingLoop = ParentLoop;
     
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            if (_expr.current is ConstantExpression || _expr is ConstantExpression)
                EmitConstantLoop(ec);
            else
            {
                ec.EmitInstruction(new Jump() { DestinationLabel = LoopCondition.Name });

                ec.MarkLabel(EnterLoop);

                _stmt.Emit(ec);

                ec.MarkLabel(LoopCondition);
                // emit expression branchable
                _expr.EmitBranchable(ec, EnterLoop, true);
                // exit
                ec.MarkLabel(ExitLoop);
            }
            return true;
        }
        bool infinite = false;
        void EmitConstantLoop(EmitContext ec)
        {

            ConstantExpression ce = null;

            if (_expr is ConstantExpression)
                ce = (ConstantExpression)_expr;
            else
                ce = (ConstantExpression)_expr.current;

            bool val = (bool)ce.GetValue();
            if (val)
            { // if true
                infinite = true;

                ec.MarkLabel(EnterLoop);

                _stmt.Emit(ec);
                ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
                ec.MarkLabel(ExitLoop);
            }
        }

    
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
          

            FlowState ok =  _expr.DoFlowAnalysis(fc);
            _stmt.DoFlowAnalysis(fc);
   

            if (infinite && !HasBreak)
                return FlowState.Unreachable;

            return ok;
        }
    }
    
	
}