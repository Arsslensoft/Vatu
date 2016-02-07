using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class IfStatement : BaseStatement, IConditional
    {
        public IConditional ParentIf { get; set; }
        public Label Else { get; set; }
        public Label ExitIf { get; set; }
        Expr _expr;
        Statement _stmt;
        [Rule(@"<Statement>        ::= ~if ~'(' <Expression> ~')' <Statement>  ")]
        public IfStatement(Expr expr, BaseStatement stmt)
        {
            _expr = expr;
            _stmt = stmt;
        }
       public override bool Resolve(ResolveContext rc)
        {
            _expr.Resolve(rc);
            _stmt.Resolve(rc);
      
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
           
            Label lb = rc.DefineLabel(LabelType.IF);
            ExitIf = rc.DefineLabel(lb.Name + "_EXIT");
            Else = rc.DefineLabel(lb.Name + "_ELSE");
            ParentIf = rc.EnclosingIf;
            rc.EnclosingIf = this;
            rc.CurrentScope |= ResolveScopes.If;
    
            // enter if
            _expr = (Expr)_expr.DoResolve(rc);
        
            if (_expr.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(30, Location, "If condition must be a boolean expression");

            _stmt = (Statement)_stmt.DoResolve(rc);
  
            rc.CurrentScope &= ~ResolveScopes.If;
            // exit current if
            rc.EnclosingIf = ParentIf;
 
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            if (_expr.current is ConstantExpression || _expr is ConstantExpression)
                EmitConstantIf(ec);
            else
            {
                // emit expression branchable
                ec.EmitComment("if-expression evaluation");
                _expr.EmitBranchable(ec, ExitIf, false);
                ec.EmitComment("If body");
                _stmt.Emit(ec);
                ec.MarkLabel(ExitIf);
                ec.MarkLabel(Else);
            }
            return true;
        }
        void EmitConstantIf(EmitContext ec)
        {

            ConstantExpression ce = null;

            if (_expr is ConstantExpression)
                ce = (ConstantExpression)_expr;
            else
                ce = (ConstantExpression)_expr.current;

            bool val = (bool)ce.GetValue();
            if (val)
             // if true
                _stmt.Emit(ec);
            ec.MarkLabel(Else);
            }


        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
         
            FlowState ok = _expr.DoFlowAnalysis(fc);
            if (!((_stmt is Block) || (_stmt is BlockStatement)))
                ok = FlowState.Valid;
   
     
            return ok;
        }
    }
    
	
}