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
	
	[Terminal("/")]
    public class DivisionOperator : BinaryOp
    {
        public DivisionOperator()
        {
            Operator = BinaryOperator.Division;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        bool bytemul = false;
   

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bytemul = !(Right.Type.SizeInBits > 8 || Left.Type.SizeInBits > 8);
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");

            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            CommonType = Left.Type;

             rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " / " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new Xor() { DestinationReg = EmitContext.D, SourceReg = EmitContext.D, Size = 80 });
            // TODO:CHECKED DIV
            if (unsigned)
            {
                if (bytemul)
                {

                    ec.EmitInstruction(new Divide() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });

                }
                else
                {
                    ec.EmitInstruction(new Divide() { DestinationReg = RightRegister.Value, Size = 80 });

                }
            }
            else
            {
                if (bytemul)
                {
                    ec.EmitInstruction(new SignedDivide() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });

                }
                else
                {
                    ec.EmitInstruction(new SignedDivide() { DestinationReg = RightRegister.Value, Size = 80 });

                }
            }
            if (bytemul)
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = LeftRegister.Value, SourceReg = RegistersEnum.AL, Size = 80 });

            ec.EmitPush(LeftRegister.Value);
          
            return true;
        }
    }
   
}