using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC.Core
{
    public class AccessOp : Operator
    {
        public AccessOp RightOp;
        public TypeToken LeftType;

        public MethodSpec OvlrdOp;
        public virtual int Offset { get { return 0; } }
        public virtual MemberSpec Member { get { return null; } }
        public AccessOperator _op;
        public RegistersEnum? Register { get; set; }


        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (OvlrdOp != null)
                fc.MarkAsUsed(OvlrdOp);

            return base.DoFlowAnalysis(fc);
        }
        public virtual bool EmitOverrideOperatorAddress(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + _op.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorValue(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + _op.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitInstruction(new Mov() { SourceReg = EmitContext.A, DestinationReg = EmitContext.SI });
            ec.EmitPush(EmitContext.SI, 80, true);
            return true;
        }
    }
}
