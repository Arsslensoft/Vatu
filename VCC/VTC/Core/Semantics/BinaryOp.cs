using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class BinaryOp : Operator
    {
        public TypeToken RightType;
        public MethodSpec OvlrdOp;
        public RegistersEnum? RightRegister { get; set; }
        public RegistersEnum? LeftRegister { get; set; }
        protected bool ConstantOperation = false;

        public BinaryOperator Operator { get; set; }

        public void EmitCheckOvf(EmitContext ec, RegistersEnum rg, bool signed)
        {
            if (CheckOverlflow)
            {
                Label lb = ec.DefineLabel(  LabelType.CHECKED_EXPR);
                ec.EmitInstruction(new ConditionalJump() { Condition = signed? ConditionalTestEnum.NoOverflow :ConditionalTestEnum.NotCarry, DestinationLabel =  lb.Name});
                ec.EmitInstruction(new Xor() { DestinationReg = rg,SourceReg = rg}); // make 0
                ec.MarkLabel(lb);
            }
        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (OvlrdOp != null)
                fc.MarkAsUsed(OvlrdOp);

            return base.DoFlowAnalysis(fc);
        }
        protected bool unsigned = true;

        public virtual bool EmitOverrideOperator(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + Left.CommentString() + " " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + Left.CommentString() + " " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);

            ec.EmitPush(EmitContext.A);
            ec.EmitPop(LeftRegister.Value);

            if (CommonType.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceValue = EmitContext.TRUE, Size = 80 });

            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = cond, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = acond, DestinationLabel = truecase.Name });

            return true;
        }
    }
}
