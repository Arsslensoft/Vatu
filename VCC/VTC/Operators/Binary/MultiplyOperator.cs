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
	[Terminal("*")]
    public class MultiplyOperator : BinaryOp
    {
        public MultiplyOperator()
        {
            FloatingPointSupported = true;
            Operator = BinaryOperator.Multiply;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        bool bytemul = false;
        bool unsigned = true;

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bytemul = !(Right.Type.SizeInBits > 8 || Left.Type.SizeInBits > 8);
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");

            CommonType = Left.Type;
        

            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        bool EmitFloatOperation(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " * " + Right.CommentString());

            ec.EmitInstruction(new Vasm.x86.x87.FloatMulAndPop() { DestinationReg = RegistersEnum.ST1, SourceReg = RegistersEnum.ST0 });



            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

            if (CommonType.IsFloat && !CommonType.IsPointer)
                return EmitFloatOperation(ec);

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " * " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            // TODO:CHECKED MUL
            if (unsigned)
            {
                if (bytemul)
                    ec.EmitInstruction(new Multiply() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });
                else ec.EmitInstruction(new Multiply() { DestinationReg = RightRegister.Value, Size = 80 });
            }
            else
            {
                if (bytemul)
                    ec.EmitInstruction(new SignedMultiply() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });
                else ec.EmitInstruction(new SignedMultiply() { DestinationReg =RightRegister.Value, Size = 80 });
            }
            if (bytemul)
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = LeftRegister.Value, SourceReg = RegistersEnum.AL, Size = 80 });

            ec.EmitPush(LeftRegister.Value);
      
            return true;
        }
    }
    
	
}