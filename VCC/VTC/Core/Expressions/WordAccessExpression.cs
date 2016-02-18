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

            IsSegment = ((ValuePosIdentifier)bitidx).Name == "segment";
          
        }
        void EmitRef(EmitContext ec,RegistersEnum dst,int disp = 0)
        {
           
            if (variable is FieldSpec)
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceRef = ElementReference.New(variable.Signature.ToString()), SourceDisplacement = (variable as FieldSpec).InitialFieldIndex + disp, SourceIsIndirect = true, Size = 16 });

            else if (variable is VarSpec)
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = EmitContext.BP, SourceDisplacement = (variable as VarSpec).InitialStackIndex+disp, SourceIsIndirect = true, Size = 16 });
            else if (variable is RegisterSpec)
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = (variable as RegisterSpec).Register, SourceDisplacement = (variable as RegisterSpec).InitialRegisterIndex + disp, SourceIsIndirect = true, Size = 16 });
            else
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = EmitContext.BP, Size = 16, SourceDisplacement = (variable as ParameterSpec).InitialStackIndex + disp, SourceIsIndirect = true });
            
        }
        void EmitToRef(EmitContext ec, RegistersEnum dst, int disp = 0)
        {

            if (variable is FieldSpec)
                ec.EmitInstruction(new Mov() { SourceReg = dst, DestinationRef = ElementReference.New(variable.Signature.ToString()), DestinationDisplacement = (variable as FieldSpec).InitialFieldIndex + disp, DestinationIsIndirect = true, Size = 16 });

            else if (variable is VarSpec)
                ec.EmitInstruction(new Mov() { SourceReg = dst, DestinationReg = EmitContext.BP, DestinationDisplacement = (variable as VarSpec).InitialStackIndex + disp, DestinationIsIndirect = true, Size = 16 });
            else if (variable is RegisterSpec)
                ec.EmitInstruction(new Mov() { SourceReg = dst, DestinationReg = (variable as RegisterSpec).Register, DestinationDisplacement = (variable as RegisterSpec).InitialRegisterIndex + disp, DestinationIsIndirect = true, Size = 16 });
            else
                ec.EmitInstruction(new Mov() { SourceReg = dst, DestinationReg = EmitContext.BP, Size = 16, DestinationDisplacement = (variable as ParameterSpec).InitialStackIndex + disp, DestinationIsIndirect = true });

        }
        public void GetWordAccess(EmitContext ec)
        {
          if (IsSegment)
                EmitRef(ec, EmitContext.A);
            else
                EmitRef(ec, EmitContext.A,2);

            ec.EmitPush(EmitContext.A);
        }
        void SetWordAccess(EmitContext ec)
        {
            ec.EmitPop(EmitContext.A);
            if (IsSegment)
                EmitToRef(ec, EmitContext.A);
            else
                EmitToRef(ec, EmitContext.A, 2);

          
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
