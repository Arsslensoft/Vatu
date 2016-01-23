using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
	
	[Terminal("~")]
    public class OnesComplementOperator : UnaryOp
    {
        public OnesComplementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.OnesComplement;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type.Equals(BuiltinTypeSpec.Bool) && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "OnesComplement must be used with non boolean, pointer types, use ! instead");
            CommonType = Right.Type;

            if (Right is RegisterExpression)
                RegisterOperation = true;
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            if (RegisterOperation)
            {
                ec.EmitComment("~" + Right.CommentString());
                RegisterExpression.EmitUnaryOperation(ec, new Not(), ((RegisterExpression)Right).Register);
                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("~" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Not() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
         


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }

    }

}