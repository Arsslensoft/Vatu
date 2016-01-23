using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class AssignOp : Operator
    {
        protected MethodSpec OvlrdOp;
        public BinaryOp _op;

        public bool FixConstant(ResolveContext rc)
        {
            bool conv = false;
  
             if (Right is ConstantExpression)
            {
                Right = (Right as ConstantExpression).ConvertImplicitly(rc, Left.Type, ref conv);
                return conv;
            }
            else
                return false;
        }
        public virtual bool EmitOverrideOperatorFromStack(EmitContext ec)
        {

            ec.EmitComment("Override Implicit Cast Operator : " + " (" + Left.Type.Name + ")" + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperator(EmitContext ec)
        {
            Right.EmitToStack(ec);
            ec.EmitComment("Override Cast Operator : " + " (" + Left.Type.Name + ")" + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {
            Right.EmitToStack(ec);
            ec.EmitComment("Override Cast Operator : " + " (" + Left.Type.Name + ")" + Right.CommentString());
            ec.EmitCall(OvlrdOp);



            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(EmitContext.A), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = EmitContext.TRUE, Size = 80 });

            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = cond, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = acond, DestinationLabel = truecase.Name });

            return true;
        }
      
        public bool CommonCheck()
        {
            if (Left is BitAccessExpression && Right.Type != BuiltinTypeSpec.Bool)
            {
                ResolveContext.Report.Error("Bit indexed access must have bool");

                return false;
            } return true;
        }

    }
}
