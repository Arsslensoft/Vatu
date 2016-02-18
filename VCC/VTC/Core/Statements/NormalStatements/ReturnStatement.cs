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
        MethodSpec Method;
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


       public bool PopAllToRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
       {

           int s = size / 2;


           for (int i = 0; i < s; i++)
               ec.EmitInstruction(new Pop() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });
           if (size % 2 != 0)
           {
               ec.EmitPop(RegistersEnum.DX);
               ec.EmitInstruction(new Mov() { DestinationReg = rg, DestinationDisplacement = offset - 1 + size, DestinationIsIndirect = true, Size = 8, SourceReg = RegistersEnum.DL });

           }
           return true;
       }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            Method = rc.CurrentMethod;
            if (_expr != null)
            {
                _expr = (Expr)_expr.DoResolve(rc);
                if (!TypeChecker.CompatibleTypes(_expr.Type,rc.CurrentMethod.MemberType))
                    ResolveContext.Report.Error(0, Location, "Return expression type must be " + rc.CurrentMethod.MemberType.Name);
            
             
            }
            else if(!rc.CurrentMethod.MemberType.Equals(BuiltinTypeSpec.Void))
                ResolveContext.Report.Error(0, Location, "Empty returns are only used with void methods");

            if (rc.EnclosingTry == null)
                ReturnLabel = new Label(rc.CurrentMethod.Signature + "_ret");
            else ReturnLabel = rc.EnclosingTry.TryReturn;
            //// set exit loops
            //ILoop enc = rc.EnclosingLoop;
            //while (enc != null)
            //{

            //    enc.HasBreak = true;
            //    enc = enc.ParentLoop;

            //}
            return this;
        }
        public override bool Emit(EmitContext ec)
        {   bool ok=true;
            if (_expr != null )
            {
                
                ok = _expr.EmitToStack(ec);
                if (!(Method.memberType.IsFloat && !Method.memberType.IsPointer))
                {
                    int ret_size = Method.memberType.GetSize(Method.memberType);
                    if (ret_size <= 2)
                        ec.EmitPop(EmitContext.A);
                    else
                        PopAllToRegister(ec, EmitContext.BP, ret_size,Method.LastParameterEndIdx);
                }
            }
            ec.EmitInstruction(new Vasm.x86.Jump() { DestinationLabel = this.ReturnLabel.Name });
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