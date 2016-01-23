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
	 [Terminal("??")]
    public class ZeroTestOperator : UnaryOp
    {
        public ZeroTestOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ZeroTest;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type.Equals(BuiltinTypeSpec.Bool) && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(32, Location, "Zero Operators must be used with non boolean, pointer types");
         
            CommonType = BuiltinTypeSpec.Bool;


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
                ec.EmitComment("??" + Right.CommentString());
              
                ec.EmitInstruction(new Compare() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 0, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
                ec.EmitPush(((RegisterExpression)Right).Register);
             
                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("??" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = 0, Size = 80 });
            ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
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
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);

            if (RegisterOperation)
            {
                ec.EmitComment("??" + Right.CommentString());

                ec.EmitInstruction(new Compare() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 0, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
                ec.EmitPush(((RegisterExpression)Right).Register);

                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("??" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = 0, Size = 80 });
            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
            return true;
        }

    }
    
	
}