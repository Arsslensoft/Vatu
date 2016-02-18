using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC.Core
{
    public class AccessOp : Operator
    {
        CallingConventionsHandler ccvh;
        public AccessOp()
        {
            ccvh = new CallingConventionsHandler();
        }
        public AccessOp RightOp;
        public TypeToken LeftType;

     
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
         
      
            ec.EmitComment("Override Operator : " + _op.ToString() + " " + Right.CommentString());
            ec.EmitCallOperator(Left, Right, OvlrdOp);
            return true;
        }
        public virtual bool EmitOverrideOperatorValue(EmitContext ec)
        {
         
            ec.EmitComment("Override Operator : " + _op.ToString() + " " + Right.CommentString());
            ec.EmitCallOperator(Left, Right, OvlrdOp);
            ec.EmitInstruction(new Mov() { SourceReg = EmitContext.A, DestinationReg = EmitContext.SI });
            ec.EmitPush(EmitContext.SI, 80, true);
            return true;
        }
    }
}
