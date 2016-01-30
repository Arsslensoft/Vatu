using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class ForStatement : BaseStatement, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }

        List<Expr> Init = new List<Expr>();
        List<Expr> Inc = new List<Expr>();

        ParameterSequence<Expr> _init;
        Expr _cond;
        ParameterSequence<Expr> _inc;

   
        Statement _stmt;

        [Rule("<Statement>     ::= ~for ~'(' <PARAM EXPR> ~';' <Expression>  ~';' <PARAM EXPR> ~')' <Statement>")]
        public ForStatement(ParameterSequence<Expr> init, Expr cond, ParameterSequence<Expr> inc, Statement stmt)
        {
            _init = init;
            _cond = cond;
            _inc = inc;
            _stmt = stmt;
        }
       public override bool Resolve(ResolveContext rc)
        {
            _stmt.Resolve(rc);
            _inc.Resolve(rc);
            _init.Resolve(rc);
            _cond.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CurrentScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.FOR);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;

            foreach (Expr e in _init)
            {
                if (e != null)
                    Init.Add((Expr)e.DoResolve(rc));
            }

            _cond = (Expr)_cond.DoResolve(rc);

            foreach (Expr e in _inc)
            {
                if (e != null)
                    Inc.Add((Expr)e.DoResolve(rc));
            }


    

            _stmt = (Statement)_stmt.DoResolve(rc);

            // exit current loop
            rc.CurrentScope &= ~ResolveScopes.Loop;
            rc.EnclosingLoop = ParentLoop;
            return this;
        }
    
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("For init");
            foreach (Expr e in Init)
                e.Emit(ec);

            ec.EmitInstruction(new Jump() { DestinationLabel = LoopCondition.Name });
            ec.MarkLabel(EnterLoop);
            ec.EmitComment("For block");
            _stmt.Emit(ec);

            ec.EmitComment("For increment");

            foreach (Expr e in Inc)
                e.Emit(ec);

            ec.MarkLabel(LoopCondition);
            if (_cond != null)
                _cond.EmitBranchable(ec, EnterLoop, true);
            else ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
            ec.EmitComment("Exit for");
            ec.MarkLabel(ExitLoop);
            return true;
        }


        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            CodePath cur = new CodePath(loc); // sub code path
          
            CodePath back = fc.CodePathReturn;
            fc.CodePathReturn = cur; // set current code path
            FlowState ok = FlowState.Valid;
            foreach (Expr e in _init)
                ok &= e.DoFlowAnalysis(fc);

            foreach (Expr e in _inc)
                ok &= e.DoFlowAnalysis(fc);
           ok &= _cond.DoFlowAnalysis(fc) & _stmt.DoFlowAnalysis(fc);

            foreach (Expr e in _inc)
                ok &= e.DoFlowAnalysis(fc);
            back.AddPath(cur);
            fc.CodePathReturn = back; // restore code path
            return ok;
        }
    }
	
}