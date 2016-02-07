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
	[Terminal("-")]
    public class SubtractionOperator : BinaryOp
    {
        public SubtractionOperator()
        {
            FloatingPointSupported = true;
            Operator = BinaryOperator.Subtraction;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
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
            ec.EmitComment(Left.CommentString() + " - " + Right.CommentString());

            ec.EmitInstruction(new Vasm.x86.x87.FloatSubAndPop() { DestinationReg = RegistersEnum.ST1, SourceReg = RegistersEnum.ST0 });



            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

            if (CommonType.IsFloat && !CommonType.IsPointer)
                return EmitFloatOperation(ec);

      
            Left.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push
            ec.EmitComment(Left.CommentString() + " - " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);


            ec.EmitInstruction(new Sub() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80, OptimizingBehaviour = OptimizationKind.PPO });
            EmitCheckOvf(ec, LeftRegister.Value, CommonType.IsSigned);
            ec.EmitPush(LeftRegister.Value);

            return true;
        }

    }

	
}