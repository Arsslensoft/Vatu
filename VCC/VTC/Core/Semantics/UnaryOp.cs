using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    [Terminal("new")]
    [Terminal("delete")]
    [Terminal("[]")]
    public class UnaryOp : Operator
    {
        public MethodSpec OvlrdOp;
        protected bool RegisterOperation = false;
        public RegistersEnum? Register { get; set; }
        public UnaryOperator Operator { get; set; }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (OvlrdOp != null)
                fc.MarkAsUsed(OvlrdOp);

            return base.DoFlowAnalysis(fc);
        }
        public virtual bool EmitOverrideOperator(EmitContext ec)
        {

            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {

            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);

            ec.EmitPush(EmitContext.A);
            ec.EmitPop(Register.Value);

            if (CommonType.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(Register.Value), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = EmitContext.TRUE, Size = 80 });

            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = cond, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = acond, DestinationLabel = truecase.Name });

            return true;
        }
    }
}
