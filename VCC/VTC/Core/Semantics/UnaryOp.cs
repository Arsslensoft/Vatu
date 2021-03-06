﻿using VTC.Base.GoldParser.Semantic;
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
   
        internal bool ReturnExpression = false;
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
       
            ec.EmitComment("Override Operator : " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCallOperator(Right, OvlrdOp);
            return true;
        }
      
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {
        
       
            ec.EmitComment("Override Operator : " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCallOperator(Right, OvlrdOp);

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

        public void EmitCheckOvf(EmitContext ec, RegistersEnum rg, bool signed)
        {
            if (CheckOverlflow)
            {
                Label lb = ec.DefineLabel(LabelType.CHECKED_EXPR);
                ec.EmitInstruction(new ConditionalJump() { Condition = signed ? ConditionalTestEnum.NoOverflow : ConditionalTestEnum.NotCarry, DestinationLabel = lb.Name });
                ec.EmitInstruction(new Xor() { DestinationReg = rg, SourceReg = rg }); // make 0
                ec.MarkLabel(lb);
            }
        }
    }
}
