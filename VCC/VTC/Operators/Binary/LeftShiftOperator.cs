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
	
	[Terminal("<<")]
    public class LeftShiftOperator : BinaryOp
    {
        public LeftShiftOperator()
        {
            Operator = BinaryOperator.LeftShift;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public ushort ShiftValue { get; set; }
        bool noshift = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                ShiftValue = ushort.Parse((Right as ConstantExpression).GetValue().ToString());


            if (ShiftValue > 15)
                noshift = true;

            if (Left is RegisterExpression)
                RegisterOperation = true;
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

            if (RegisterOperation)
            {
                RegisterExpression.EmitOperation(ec, new ShiftLeft(), ShiftValue, ((RegisterExpression)Left).Register);
                return true;
            }
            if (noshift)
            {
                ec.EmitInstruction(new Mov() { SourceValue = 0, DestinationReg = LeftRegister.Value, Size = 16 });
                ec.EmitPush(LeftRegister.Value);
                return true;

            }
            Left.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " << " + ShiftValue);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new ShiftLeft() { DestinationReg = LeftRegister.Value, SourceValue = ShiftValue, Size = 80 });
            ec.EmitPush(LeftRegister.Value);
    
            return true;
        }

    }
    
}