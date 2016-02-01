using VTC.Base.GoldParser.Parser;
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
	[Terminal("¤")]
    public class ParityTestOperator : UnaryOp
    {
        public ParityTestOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ParityTest;
        }

       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type.Equals(BuiltinTypeSpec.Bool) )
                ResolveContext.Report.Error(33, Location, "Parity Operators must be used with non boolean, pointer types");
            UnaryCheck(rc);
            CommonType = BuiltinTypeSpec.Bool;

            if (Right is RegisterExpression)
                RegisterOperation = true;
          rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(),ref OvlrdOp, new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }


        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
           
            Right.EmitToStack(ec);
            ec.EmitComment("¤" + Right.CommentString());
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new Test() { DestinationReg = Register.Value, SourceValue = 1, Size = 80 });
            ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
            ec.EmitPush(Register.Value);
     


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec,truecase,v, ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
           
            Right.EmitToStack(ec);
            ec.EmitComment("¤" + Right.CommentString());
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new Test() { DestinationReg = Register.Value, SourceValue = 1, Size = 80 });
            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
            return true;
        }
    }

	
}