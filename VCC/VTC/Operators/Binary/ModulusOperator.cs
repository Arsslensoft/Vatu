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
	 [Terminal("%")]
    public class ModulusOperator : BinaryOp
    {
        public ModulusOperator()
        {
            FloatingPointSupported = true;
            Operator = BinaryOperator.Modulus;
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
          
            CommonType = Left.Type;
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        bool EmitFloatOperation(EmitContext ec)
        {
            Right.EmitToStack(ec);
            Left.EmitToStack(ec);
      
            ec.EmitComment(Left.CommentString() + " % " + Right.CommentString());
            Label modlb = ec.DefineLabel(LabelType.FLOAT_REM);
            ec.MarkLabel(modlb);
            ec.EmitInstruction(new Vasm.x86.x87.FloatPRem() );
            ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() { DestinationReg = EmitContext.A });
            ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
            ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags());
            ec.EmitInstruction(new ConditionalJump() {DestinationLabel = modlb.Name,Condition =  ConditionalTestEnum.ParityEven });
            ec.EmitInstruction(new Vasm.x86.x87.FloatStoreAndPop() {DestinationReg = RegistersEnum.ST1 });

            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

            if (CommonType.IsFloat && !CommonType.IsPointer)
                return EmitFloatOperation(ec);
            Left.EmitToStack(ec);
            ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            ec.MarkOptimizable(); // Marks last instruction as last push

            ec.EmitComment(Left.CommentString() + " % " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new Xor() { DestinationReg = EmitContext.D, SourceReg = EmitContext.D, Size = 80, OptimizingBehaviour = OptimizationKind.None });
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
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = LeftRegister.Value, SourceReg = RegistersEnum.AH, Size = 80 });
            else
                ec.EmitInstruction(new Mov() { DestinationReg =  LeftRegister.Value, SourceReg = RegistersEnum.DX, Size = 80 });

            ec.EmitPush(LeftRegister.Value);
        
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec) ;
        }
    }
    
	
}