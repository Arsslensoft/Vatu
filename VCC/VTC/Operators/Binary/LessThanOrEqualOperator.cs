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
	[Terminal("<=")]
    public class LessThanOrEqualOperator : BinaryOp
    {
        public LessThanOrEqualOperator()
        {
            FloatingPointSupported = true;
            Operator = BinaryOperator.LessThanOrEqual;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);

            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
          
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        bool EmitFloatOperationBranchable(EmitContext ec, Label truecase, bool v)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());

            ec.EmitInstruction(new Vasm.x86.x87.FloatCompareAnd2Pop());
            ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() { DestinationReg = EmitContext.A });
            ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
            ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags());

            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.BelowOrEqual, ConditionalTestEnum.NotBelowOrEqual);

            return true;
        }
        bool EmitFloatOperation(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());

            ec.EmitInstruction(new Vasm.x86.x87.FloatCompareAnd2Pop());
            ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() {  DestinationReg = EmitContext.A});
            ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
            ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags() );

        
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.BelowOrEqual, ConditionalTestEnum.NotBelowOrEqual);
            ec.EmitPush(LeftRegister.Value);

            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

            if (Left.Type.IsFloat && !Left.Type.IsPointer)
                return EmitFloatOperation(ec);

            Left.EmitToStack(ec);
            ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            ec.MarkOptimizable(); // Marks last instruction as last push


            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80, OptimizingBehaviour = OptimizationKind.PPO });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80, OptimizingBehaviour = OptimizationKind.PPO });
            if (unsigned)
                ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.BelowOrEqual, ConditionalTestEnum.NotBelowOrEqual);
            else
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.LessThanOrEqualTo, ConditionalTestEnum.NotGreaterThanOrEqualTo);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.NotEqual, ConditionalTestEnum.Equal);

            if (Left.Type.IsFloat && !Left.Type.IsPointer)
                return EmitFloatOperationBranchable(ec, truecase, v);


            Left.EmitToStack(ec);
            ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            ec.MarkOptimizable(); // Marks last instruction as last push


            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80, OptimizingBehaviour = OptimizationKind.PPO });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80, OptimizingBehaviour = OptimizationKind.PPO });


            // jumps
            if (unsigned)
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.BelowOrEqual, ConditionalTestEnum.NotBelowOrEqual);
            else
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.LessThanOrEqualTo,ConditionalTestEnum.NotLessThanOrEqualTo);

            return true;
        }
    }
    
	
}