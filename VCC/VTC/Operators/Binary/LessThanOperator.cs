using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
	[Terminal("<")]
    public class LessThanOperator : BinaryOp
    {
      
        public LessThanOperator()
        {
            Operator = BinaryOperator.LessThan;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {


            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");

            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;

            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " < " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

       
            if(unsigned)
                ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.Below, ConditionalTestEnum.NotBelow);
            else
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.LessThan, ConditionalTestEnum.NotLessThan);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.LessThan, ConditionalTestEnum.NotLessThan);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " < " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);


            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps

            if (unsigned)
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Below, ConditionalTestEnum.NotBelow);
            else
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.LessThan, ConditionalTestEnum.NotLessThan);
         

            return true;
        }
    }
    
	
}