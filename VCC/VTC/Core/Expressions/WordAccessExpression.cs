using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class WordAccessExpression : VariableExpression
    {
        bool IsWord= false;
        internal Expr BitIndex;
        bool IsSegment = false;
        public WordAccessExpression(MemberSpec ms, Expr bitidx)
            : base(ms)
        {
            Type = BuiltinTypeSpec.Pointer;
            BitIndex = bitidx;

            IsSegment = ((ValuePosIdentifier)bitidx).Name == "SEGMENT";
          
        }
        public void GetWordAccess(EmitContext ec)
        {
            // emit var
            base.EmitToStack(ec);
            ec.EmitPop(EmitContext.B); // high
            ec.EmitPop(EmitContext.A); // low

            if (IsSegment)
                ec.EmitPush(EmitContext.A);
            else
                ec.EmitPush(EmitContext.B);

        }
        void SetWordAccess(EmitContext ec)
        {

            // pop value
            ec.EmitPop(EmitContext.C);
            // emit var
            base.EmitToStack(ec);
            ec.EmitPop(EmitContext.B);//high
            ec.EmitPop(EmitContext.A);// low

            if (IsSegment)
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceReg = EmitContext.C, Size = 16 });
            else ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.B, SourceReg = EmitContext.C, Size = 16 });

            ec.EmitPop(EmitContext.A);// low
            ec.EmitPop(EmitContext.B);//high
            base.EmitFromStack
                (ec);
        }

        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("DWord Pointer Access");
            GetWordAccess(ec);
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitComment("DWord Pointer Access");
            GetWordAccess(ec);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitComment("DWord Pointer  Access Stack");
            SetWordAccess(ec);
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
            return "DPointerAccess(" + variable.Name + ") at " ;
        }
    }
}
