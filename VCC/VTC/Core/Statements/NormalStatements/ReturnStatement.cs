using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ReturnStatement : NormalStatment
    {
        public Label ReturnLabel { get; set; }

        private Expr _expr;

        [Rule("<Normal Stm> ::= ~return <Expression> ~';' ")]
        public ReturnStatement(Expr b)
        {
            _expr = b;

        }
        [Rule("<Normal Stm> ::= ~return ~';' ")]
        public ReturnStatement()
        {
            _expr = null;

        }
       public override bool Resolve(ResolveContext rc)
        {
            if (_expr != null)
                return _expr.Resolve(rc);
            else return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_expr != null)
            {
                _expr = (Expr)_expr.DoResolve(rc);
                if (!_expr.Type.Equals(rc.CurrentMethod.MemberType))
                    ResolveContext.Report.Error(0, Location, "Return expression type must be " + rc.CurrentMethod.MemberType.Name);
            
             
            }
            else if(!rc.CurrentMethod.MemberType.Equals(BuiltinTypeSpec.Void))
                ResolveContext.Report.Error(0, Location, "Empty returns are only used with void methods");

            ReturnLabel = new Label(rc.CurrentMethod.Signature + "_ret");

            // set exit loops
            ILoop enc = rc.EnclosingLoop;
            while (enc != null)
            {

                enc.HasBreak = true;
                enc = enc.ParentLoop;

            }
            return this;
        }
        public override bool Emit(EmitContext ec)
        {   bool ok=true;
            if (_expr != null)
            {
                ok = _expr.Emit(ec);
                ec.EmitInstruction(new Vasm.x86.Pop() { DestinationReg = EmitContext.A });
            }
            ec.EmitInstruction(new Vasm.x86.Jump() { DestinationLabel = this.ReturnLabel.Name });
            return ok;
        }
      
     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            fc.CodePathReturn.Returns = true;
            if (_expr != null)
                _expr.DoFlowAnalysis(fc);
            return FlowState.Unreachable;
        }
    }
    
	
}