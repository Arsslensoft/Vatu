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
	[Terminal("&")]
    public class BitwiseAndOperator : BinaryOp
    {
        public BitwiseAndOperator()
        {
            Operator = BinaryOperator.BitwiseAnd;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
        
            if (!FixConstant(rc))
                ResolveContext.Report.Error(22, Location, "Bitwise operation must have the same type");
            CommonType = Left.Type;

            if (Right is RegisterExpression && Left is RegisterExpression)
                RegisterOperation = true;
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");
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
            ec.EmitComment(Left.CommentString() + " & " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new And() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }

    }

	
}