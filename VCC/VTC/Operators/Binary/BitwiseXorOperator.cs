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
	[Terminal("^")]
    public class BitwiseXorOperator : BinaryOp
    {
        public BitwiseXorOperator()
        {
            Operator = BinaryOperator.ExclusiveOr;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            if (!FixConstant(rc))
                ResolveContext.Report.Error(22, Location, "Bitwise operation must have the same type");
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
            //ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push


            ec.EmitComment(Left.CommentString() + " ^ " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new Xor() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80, OptimizingBehaviour = OptimizationKind.PPO });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            Left.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push
            Right.EmitToStack(ec);
            //ec.MarkOptimizable(); // Marks last instruction as last push

            ec.EmitComment(Left.CommentString() + " ^ " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Xor() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80, OptimizingBehaviour = OptimizationKind.PPO });

            if (CommonType.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceValue = EmitContext.TRUE, Size = 80 });

            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);


            return true;
        }
    }
    
	
}