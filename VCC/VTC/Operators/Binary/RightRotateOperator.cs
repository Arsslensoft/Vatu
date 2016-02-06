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
	[Terminal("~>")]
    public class RightRotateOperator : BinaryOp
    {
        public RightRotateOperator()
        {
            Operator = BinaryOperator.RightRotate;
            LeftRegister = RegistersEnum.AX;

        }
        public ushort RotValue { get; set; }
        bool norot = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                RotValue = ushort.Parse((Right as ConstantExpression).GetValue().ToString());

            RotValue = (ushort)(RotValue % 16);


            norot = (RotValue == 0);

          
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(),ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
           
            if (norot)
            {
                Left.EmitToStack(ec);
                return true;
            }
            Left.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " ~> " + RotValue);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new RotateRight() { DestinationReg = LeftRegister.Value, SourceValue = RotValue, Size = 80 });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }

    }

	
}