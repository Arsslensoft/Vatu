using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class RestrictStatement : BaseStatement
    {
     
        Expr _expr;
        Statement _stmt;
        [Rule(@"<Statement>        ::= ~restrict ~'(' <Expression> ~')' <Statement>  ")]
        public RestrictStatement(Expr expr, BaseStatement stmt)
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
           
        
            // enter if
            _expr = (Expr)_expr.DoResolve(rc);
        
            if (!(_expr is VariableExpression))
                ResolveContext.Report.Error(30, Location, "Restrict expression must be variable based expressions");

            _stmt = (Statement)_stmt.DoResolve(rc);

      
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            _expr.EmitToStack(ec);
            _stmt.Emit(ec);
            _expr.EmitFromStack(ec);
            return true;
        }
     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
         
            FlowState ok = _expr.DoFlowAnalysis(fc);
            if (!((_stmt is Block) || (_stmt is BlockStatement)))
                ok = FlowState.Valid;
            else ok = _stmt.DoFlowAnalysis(fc);
     
            return ok;
        }
    }
    
	
}