using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class ByteAccessExpression : VariableExpression
    {
        bool IsByte = false;
        internal Expr BitIndex;
        bool IsHigh = false;
        LoadEffectiveAddressOp LEA;
        public ByteAccessExpression(MemberSpec ms, Expr bitidx, VariableExpression vexpr = null, ResolveContext rc = null)
            : base(ms)
        {
            Type = BuiltinTypeSpec.Byte;
            BitIndex = bitidx;
            IsByte = vexpr != null;
            if (!IsByte)
                IsHigh = ((ValuePosIdentifier)bitidx).Name == "HIGH";
            else
            {
                LEA = new LoadEffectiveAddressOp();
                LEA.Right = vexpr;
                LEA = (LoadEffectiveAddressOp)LEA.DoResolve(rc);

            }
        }
        public void GetByteAccess(EmitContext ec)
        {
            // emit var
            base.EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            if (IsHigh)
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = EmitContext.B, SourceReg = ec.GetHigh(EmitContext.A), Size = 80 });
            else
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = EmitContext.B, SourceReg = ec.GetLow(EmitContext.A), Size = 80 });

            ec.EmitPush(EmitContext.B);

        }
        void SetByteAccess(EmitContext ec)
        {

            // pop value
            ec.EmitPop(EmitContext.B);
            // emit var
            base.EmitToStack(ec);
            ec.EmitPop(EmitContext.A);

            if (IsHigh)
                ec.EmitInstruction(new Mov() { DestinationReg = ec.GetHigh(EmitContext.A), SourceReg = ec.GetLow(EmitContext.B), Size = 8 });
            else ec.EmitInstruction(new Mov() { DestinationReg = ec.GetLow(EmitContext.A), SourceReg = ec.GetLow(EmitContext.B), Size = 8 });

            ec.EmitPush(EmitContext.A);
            base.EmitFromStack
                (ec);
        }
        public void SetByte(EmitContext ec)
        {
            if (!IsByte)
                SetByteAccess(ec);
            else
            {
                // push/pop idx
                BitIndex.EmitToStack(ec);
                ec.EmitPop(EmitContext.B);
                // emit address of var
                LEA.EmitToStack(ec);
                ec.EmitPop(EmitContext.SI);
                // add addresof + offset
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SI, SourceReg = EmitContext.B });
                // pop value
                ec.EmitPop(EmitContext.SI, 80, true);
            }


        }
        public void GetByte(EmitContext ec)
        {
            if (!IsByte)
                GetByteAccess(ec);
            else
            {
                // push/pop idx
                BitIndex.EmitToStack(ec);
                ec.EmitPop(EmitContext.B);
                // emit address of var
                LEA.EmitToStack(ec);
                ec.EmitPop(EmitContext.SI);
                // add addresof + offset
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SI, SourceReg = EmitContext.B });
                // pop value
                ec.EmitPush(EmitContext.SI, 80, true);
            }
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("Byte Access");
            GetByte(ec);
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitComment("Byte Access");
            GetByte(ec);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitComment("Byte Access Stack");
            SetByte(ec);
            return true;
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });

            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            return true;
        }
        public override string CommentString()
        {
            return "ByteAccess(" + variable.Name + ") at " + BitIndex.CommentString();
        }
    }
}
