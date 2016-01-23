using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class BitAccessExpression : VariableExpression
    {
        public bool IsBit;
        internal Expr BitIndex;
        public BitAccessExpression(MemberSpec ms, Expr bitidx)
            : base(ms)
        {
            Type = BuiltinTypeSpec.Bool;
            BitIndex = bitidx;
        }

        public void SetBit(EmitContext ec)
        {

            BitIndex.EmitToStack(ec);
            ec.EmitPop(EmitContext.C);
            ec.EmitPop(EmitContext.B); // the value

            // emit var
            base.EmitToStack(ec);
            ec.EmitPop(EmitContext.D);


            // shifting
            ec.EmitInstruction(new ShiftLeft() { DestinationReg = EmitContext.B, SourceReg = ec.GetLow(EmitContext.C), Size = 80 });
            ec.EmitInstruction(new Vasm.x86.BitTestAndReset() { DestinationReg = EmitContext.D, SourceReg = EmitContext.C, Size = 80 });
            ec.EmitInstruction(new Or() { DestinationReg = EmitContext.D, SourceReg = EmitContext.B });

            ec.EmitPush(EmitContext.D);

        }
        public void GetBit(EmitContext ec)
        {
            BitIndex.EmitToStack(ec);
            ec.EmitPop(EmitContext.C);
            ec.EmitPop(EmitContext.D);

            // shifting
            ec.EmitInstruction(new ShiftRight() { DestinationReg = EmitContext.D, SourceReg = ec.GetLow(EmitContext.C), Size = 80 });
            ec.EmitInstruction(new And()
            {
                DestinationReg = EmitContext.D
                ,
                SourceValue = 1,
                Size = 80
            });
            ec.EmitPush(EmitContext.D);
        }
        public override bool Emit(EmitContext ec)
        {
            bool v = base.EmitToStack(ec);
            GetBit(ec);
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            bool v = base.EmitToStack(ec);
            GetBit(ec);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            SetBit(ec);
            return base.EmitFromStack(ec);
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
            return "BitAccess(" + variable.Name + ") at " + BitIndex.CommentString();
        }
    }
}
