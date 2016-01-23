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
	[Terminal("++")]
    public class IncrementOperator : UnaryOp
    {
        public IncrementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.PostfixIncrement;
        }
        AssignExpression ae;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type.Equals(BuiltinTypeSpec.Bool) && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "Unary operator must be used with non boolean, pointer types, use ! instead");


            ae = new AssignExpression(Right, new SimpleAssignOperator(), Right);
            ae = (AssignExpression)ae.DoResolve(rc);
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
            {
                base.EmitOverrideOperator(ec);
                return Right.EmitFromStack(ec);
            }
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "++");
                RegisterExpression.EmitUnaryOperation(ec, new INC(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "++ ");
            ec.EmitPop(Register.Value);
            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Add() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
            ec.EmitInstruction(new INC() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);


            ae.EmitFromStack(ec);
            //   ec.EmitPush(ec.FirstRegister());



            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                base.EmitOverrideOperator(ec);
                ec.EmitPush(EmitContext.A);
                return Right.EmitFromStack(ec);
            }
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "++");
                RegisterExpression.EmitUnaryOperation(ec, new INC(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "++ ");
            ec.EmitPop(Register.Value);
            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Add() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
            ec.EmitInstruction(new INC() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
            ec.EmitPush(Register.Value);
            ae.EmitFromStack(ec);


            return true;
        }
    }

	
}