using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class ForEachStatement : BaseStatement, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }
        public bool HasBreak { get; set; }

        List<Expr> Init = new List<Expr>();
        List<Expr> Inc = new List<Expr>();

       
        Expr _cond;
        VariableExpression _vexpr;
        Expr array;
   
        Statement _stmt;
        ByIndexOperator biop;
        AccessExpression access;
        VariableExpression _fidx;
        IncrementOperator _inc;



        [Rule("<Statement>     ::= ~foreach  ~'(' <Var Expr> ~in <Expression> ~')' <Statement> ~while ~'(' <Expression> ~')' ~';'")]
        public ForEachStatement(VariableExpression vexpr, Expr arr, Statement stmt,Expr cond)
        {
            array = arr;
            _cond = cond;
            _vexpr = vexpr;
            _stmt = stmt;
        }

        [Rule("<Statement>     ::= ~foreach  ~'(' <Var Expr> ~in <Expression> ~')' <Statement> ~';'")]
        public ForEachStatement(VariableExpression vexpr, Expr arr, Statement stmt)
        {
            array = arr;
            
            _vexpr = vexpr;
            _stmt = stmt;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _stmt.Resolve(rc);
            _vexpr.Resolve(rc);
            array.Resolve(rc);
            if(_cond != null)
            _cond.Resolve(rc);
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CreateNewState();
            rc.CurrentGlobalScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.FOREACH);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;

            _vexpr = (VariableExpression)_vexpr.DoResolve(rc);
            array = (Expr)array.DoResolve(rc);
            _fidx = (VariableExpression)(new ForeachIndexerExpression(position).DoResolve(rc));

            // operation
            biop = new ByIndexOperator();
            biop.Left = array;
            biop.Right = _fidx;
           access = (AccessExpression) biop.DoResolve(rc);                 
            //increment
           _inc = new IncrementOperator();
           _inc.Operator = UnaryOperator.PrefixIncrement;
           _inc.Right = _fidx;
           _inc = (IncrementOperator)_inc.DoResolve(rc);

           if (!_vexpr.Type.Equals(array.Type.BaseType))
               ResolveContext.Report.Error(0, Location, "Foreach variable type mismatch");


            _stmt = (Statement)_stmt.DoResolve(rc);


            if (_cond != null)
                _cond = (Expr)_cond.DoResolve(rc);
            else
            {
                if (!(array.Type is ArrayTypeSpec))
                    ResolveContext.Report.Error(0, Location, "Foreach while missing for non-array type");


                _cond = new BinaryOperation(_fidx, new LessThanOperator(), new UIntConstant((ushort)(array.Type as ArrayTypeSpec).ArrayCount, Location)); ;

            }
            // exit current loop
            rc.RestoreOldState();
            rc.EnclosingLoop = ParentLoop;
            return this;
        }
    
        public override bool Emit(EmitContext ec)
 {
     if (_cond is ConstantExpression || _cond.current is ConstantExpression )
         EmitConstantLoop(ec);
     else
     {
         ec.EmitComment("Foreach init");
         ec.EmitPush((ushort)0);
         _fidx.EmitFromStack(ec);

         ec.EmitInstruction(new Jump() { DestinationLabel = LoopCondition.Name });
         ec.MarkLabel(EnterLoop);

         ec.EmitComment("Foreach assign");
         access.EmitToStack(ec);
         _vexpr.EmitFromStack(ec);
         ec.EmitComment("Foreach block");
         _stmt.Emit(ec);

         ec.EmitComment("Foreach increment");
         _inc.Emit(ec);

         ec.MarkLabel(LoopCondition);
         if (_cond != null)
             _cond.EmitBranchable(ec, EnterLoop, true);
         else ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
         ec.EmitComment("Exit foreach");
         ec.MarkLabel(ExitLoop);
     }
            return true;
        }
        bool infinite = false;
        void EmitConstantLoop(EmitContext ec)
        {

            ConstantExpression ce = null;

            if (_cond is ConstantExpression)
                ce = (ConstantExpression)_cond;
            else
                ce = (ConstantExpression)_cond.current;

            bool val = (bool)ce.GetValue();
            if (val)
            { // if true
                infinite = true;
                ec.EmitComment("Foreach init");
                ec.EmitPush((ushort)0);
                _fidx.EmitFromStack(ec);

                ec.EmitInstruction(new Jump() { DestinationLabel = LoopCondition.Name });
                ec.MarkLabel(EnterLoop);

                ec.EmitComment("Foreach assign");
                access.EmitToStack(ec);
                _vexpr.EmitFromStack(ec);

                ec.EmitComment("Foreach block");
                _stmt.Emit(ec);

                ec.EmitComment("Foreach increment");
                _inc.Emit(ec);
               
                ec.MarkLabel(LoopCondition);
                ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
                ec.EmitComment("Exit foreach");
                ec.MarkLabel(ExitLoop);
            }
           
        }
     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
         
            FlowState ok = FlowState.Valid;
         
                ok &= array.DoFlowAnalysis(fc);

        
                ok &= _vexpr.DoFlowAnalysis(fc);
           ok &= _cond.DoFlowAnalysis(fc) ;

            
            _stmt.DoFlowAnalysis(fc);
         

            if (infinite && !HasBreak)
                return FlowState.Unreachable;

            return ok;
        }
    }
	
}