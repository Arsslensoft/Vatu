using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ThrowStatement : NormalStatment
    {
        public Label ThrowLabel;

        private VariableExpression _expr;
        bool supported = false;
        [Rule("<Normal Stm> ::= ~throw  <Var Expr> ~';' ")]
        public ThrowStatement(VariableExpression b)
        {
            _expr = b;

        }
   
       public override bool Resolve(ResolveContext rc)
        {
            if (_expr != null)
                return _expr.Resolve(rc);
            else return true;
        }
       public bool Supported(ITry etry, VariableExpression e, ref Label retlb)
       {
           if (etry == null)
               return false;
           else if (etry.SupportedThrow(e))
           {
               retlb = etry.TryCatch;
               return true;

           }
           else return Supported(etry.ParentTry, e, ref retlb);
       }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _expr = (VariableExpression)_expr.DoResolve(rc);
                  supported  =   Supported(rc.EnclosingTry, _expr, ref ThrowLabel);
                  if (!supported)
                      ResolveContext.Report.Error(0, Location, "Unsupported throw expression, no enclosing try-catch");
               
          
            return this;
        }
        public override bool Emit(EmitContext ec)
        {   bool ok=true;

        if (supported)
        {
            _expr.EmitToStack(ec);
            ec.EmitInstruction(new Vasm.x86.Jump() { DestinationLabel = this.ThrowLabel.Name });
        }

            return ok;
        }
      
     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
        
            if (_expr != null)
                _expr.DoFlowAnalysis(fc);
            return FlowState.Unreachable;
        }
    }
    
	
}