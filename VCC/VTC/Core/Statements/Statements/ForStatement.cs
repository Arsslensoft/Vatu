using bsn.GoldParser.Semantic;
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


        ArgumentExpression _init;
        ArgumentExpression _cond;
        ArgumentExpression _inc;

        Expr _initialize;
        Expr _exit;
        Expr _increment;
        Statement _stmt;

        [Rule("<Statement>     ::= ~for ~'(' <Arg> ~';' <Arg> ~';' <Arg> ~')' <Statement>")]
        public ForStatement(ArgumentExpression init, ArgumentExpression cond, ArgumentExpression inc, Statement stmt)
        {
            _init = init;
            _cond = cond;
            _inc = inc;
            _stmt = stmt;
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

            if (_init.argexpr != null)
                _initialize = (Expr)_init.DoResolve(rc);

            if (_cond.argexpr != null)
                _exit = (Expr)_cond.DoResolve(rc);

            if (_inc.argexpr != null)
                _increment = (Expr)_inc.DoResolve(rc);

            _stmt = (Statement)_stmt.DoResolve(rc);

            // exit current loop
            rc.CurrentScope &= ~ResolveScopes.Loop;
            rc.EnclosingLoop = ParentLoop;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _stmt.Resolve(rc);
            _inc.Resolve(rc);
            _init.Resolve(rc);
            _cond.Resolve(rc);
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("For init");
            if(_initialize != null)
            _initialize.Emit(ec);
            ec.EmitInstruction(new Jump() { DestinationLabel = LoopCondition.Name });
            ec.MarkLabel(EnterLoop);
            ec.EmitComment("For block");
            _stmt.Emit(ec);

            ec.EmitComment("For increment");
          
            if (_increment != null)
            _increment.Emit(ec);

            ec.MarkLabel(LoopCondition);
            if (_exit != null)
                _exit.EmitBranchable(ec, EnterLoop, true);
            else ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
            ec.EmitComment("Exit for");
            ec.MarkLabel(ExitLoop);
            return true;
        }
        public override Reachability MarkReachable(Reachability rc)
        {
            base.MarkReachable(rc);
            if (_stmt is Block || _stmt is BlockStatement)
                return _stmt.MarkReachable(rc);
            else
                return rc;
        }

        public override bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            CodePath cur = new CodePath(loc); // sub code path
          
            CodePath back = fc.CodePathReturn;
            fc.CodePathReturn = cur; // set current code path

            bool ok = _init.DoFlowAnalysis(fc) && _stmt.DoFlowAnalysis(fc);
            back.AddPath(cur);
            fc.CodePathReturn = back; // restore code path
            return ok;
        }
    }
	
}