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
	
	[Terminal("==")]
    public class EqualOperator : BinaryOp
    {
        public EqualOperator()
        {
            FloatingPointSupported = true;
            Operator = BinaryOperator.Equality;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
           
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
            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());

            ec.EmitInstruction(new Vasm.x86.x87.FloatCompareAnd2Pop());
            ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() { DestinationReg = EmitContext.A });
            ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
            ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags());

            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);

            return true;
        }
        bool EmitFloatOperation(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());

            ec.EmitInstruction(new Vasm.x86.x87.FloatCompareAnd2Pop());
            ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() { DestinationReg = EmitContext.A });
            ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
            ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags());


            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
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
            //ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push


            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80, OptimizingBehaviour = OptimizationKind.PPO });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80, OptimizingBehaviour = OptimizationKind.PPO });

            ec.EmitBoolean(ec.GetLow(LeftRegister.Value) ,ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);


            ec.EmitPush(LeftRegister.Value);


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase,bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.NotEqual, ConditionalTestEnum.Equal);

            if (Left.Type.IsFloat && !Left.Type.IsPointer)
                return EmitFloatOperationBranchable(ec, truecase, v);
            
            Left.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push


            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80, OptimizingBehaviour = OptimizationKind.PPO });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80, OptimizingBehaviour = OptimizationKind.PPO });

            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);

            return true;
        }
    }
   
}