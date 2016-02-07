using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class IfElseStatement : BaseStatement, IConditional
    {
        public IConditional ParentIf { get; set; }
      
        public Label ExitIf { get; set; }
        public Label Else { get; set; }
 
        Expr _expr;
        Statement _stmt;
        Statement _elsestmt;
   
        [Rule(@"<Statement>        ::= ~if ~'(' <Expression> ~')' <Then Stm> ~else <Statement>   ")]
        public IfElseStatement(Expr expr, ThenStatement stmt, Statement elsestmt)
        {
            _expr = expr;
            _stmt = stmt;
            _elsestmt = elsestmt;
        }
       public override bool Resolve(ResolveContext rc)
        {
            _expr.Resolve(rc);
            _stmt.Resolve(rc);
            _elsestmt.Resolve(rc);
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
            _stmt = (Statement)_stmt.DoResolve(rc);
            _elsestmt = (Statement)_elsestmt.DoResolve(rc);

     
            if (_expr.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(30, Location, "If condition must be a boolean expression");

            rc.CurrentScope &= ~ResolveScopes.If;
            // exit current if
            rc.EnclosingIf = ParentIf;
    

            return this;
        }
     
        public override bool Emit(EmitContext ec)
        {
            if (_expr.current is ConstantExpression || _expr is ConstantExpression)
                EmitIfConstant(ec);
            else
            {
                // emit expression branchable
                ec.EmitComment("if-expression evaluation");
                _expr.EmitBranchable(ec, Else, false);
                ec.EmitComment("("+_expr.CommentString() + ") is true");
                _stmt.Emit(ec);
                ec.EmitInstruction(new Jump() {DestinationLabel = ExitIf.Name });
                ec.MarkLabel(Else);
                ec.EmitComment("Else ");
                _elsestmt.Emit(ec);
                ec.MarkLabel(ExitIf);
            }
            return true;
        }
        void EmitIfConstant(EmitContext ec)
        {
            ConstantExpression ce = null;

            if (_expr is ConstantExpression)
                ce = (ConstantExpression)_expr;
            else
                ce = (ConstantExpression)_expr.current;

            bool val = (bool)ce.GetValue();
            if (!val) // emit else
                _elsestmt.Emit(ec);
            else _stmt.Emit(ec);
        }

  
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
           

            FlowState ok = _expr.DoFlowAnalysis(fc);
     
            
            _stmt.DoFlowAnalysis(fc);
            
         
       
       
          ok =  _elsestmt.DoFlowAnalysis(fc);

          
       

            return ok;
        }
    }

	
}