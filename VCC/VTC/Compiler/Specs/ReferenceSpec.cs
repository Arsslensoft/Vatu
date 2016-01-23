using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	 public abstract class ReferenceSpec : IEmitter
     {
         public RegistersEnum Register { get; set; }
         public ParameterSpec ReferenceParameter { get; set; }
         public int InitialStackIndex { get; set; }
         public int Offset { get; set; }
         public MemberSignature Signature
         {
             get { return Member.Signature; }
         }
         public TypeSpec memberType
         {
             get { return Member.memberType; }
         }
         public TypeSpec MemberType
         {
             get { return Member.MemberType; }
         }
        public MemberSpec Member { get; set; }
        public ReferenceKind ReferenceType { get; set; }
        public ReferenceSpec(ReferenceKind rs)
        {
            ReferenceType = rs;
        }
      
      
        public virtual bool LoadEffectiveAddress(EmitContext ec)
        {
            return true;
        }
        public virtual bool ValueOf(EmitContext ec)
        {
            return true;
        }
        public virtual bool ValueOfStack(EmitContext ec)
        {
            return true;
        }

        public virtual bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {
            return true;
        }
        public virtual bool ValueOfStackAccess(EmitContext ec,int off, TypeSpec mem)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec) { return true; }
        public virtual bool EmitFromStack(EmitContext ec) { return true; }

        public bool PushAllFromRegister(EmitContext ec,RegistersEnum rg,int size, int offset=0)
        {
            int s = size / 2;

            if (size % 2 != 0)
            {
                ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.DL, SourceReg = rg, SourceDisplacement = offset - 1 + size, SourceIsIndirect = true, Size = 8 });
                ec.EmitPush(RegistersEnum.DX);
            }
            for (int i = s - 1; i >= 0; i--)
                ec.EmitInstruction(new Push() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });

            return true;
        }
        public bool PopAllToRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
        {

            int s = size / 2;


            for (int i = 0; i < s; i++)
                ec.EmitInstruction(new Pop() { DestinationReg =rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });
            if (size % 2 != 0)
            {
                ec.EmitPop(RegistersEnum.DX);
                ec.EmitInstruction(new Mov() { DestinationReg = rg, DestinationDisplacement = offset - 1 + size, DestinationIsIndirect = true, Size = 8, SourceReg = RegistersEnum.DL });

            }
            return true;
        }
        public bool PushAllFromRef(EmitContext ec, ElementReference re, int size, int offset = 0)
        {
            ec.EmitComment("Push Field [TypeOf " + re.Name + "] Offset=" + offset);
            int s = size / 2;

            if (size % 2 != 0)
            {
                ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.DL, SourceRef = re, SourceDisplacement = offset + size - 1, SourceIsIndirect = true, Size = 8 });
                ec.EmitPush(RegistersEnum.DX);
            }

            for (int i = s - 1; i >= 0; i--)
                ec.EmitInstruction(new Push() { DestinationRef = re, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });

            return true;
        }
        public bool PopAllToRef(EmitContext ec, ElementReference re, int size, int offset = 0)
        {
            ec.EmitComment("Pop Field [TypeOf " + re.Name + "] Offset=" + offset);
            int s = size / 2;


            for (int i = 0; i < s; i++)
                ec.EmitInstruction(new Pop() { DestinationRef = re, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });

            if (size % 2 != 0)
            {
                ec.EmitPop(RegistersEnum.DX);
                ec.EmitInstruction(new Mov() { DestinationRef = re, DestinationDisplacement = offset - 1 + size, DestinationIsIndirect = true, Size = 8, SourceReg = RegistersEnum.DL });

            }
            return true;
        }
    }

	
}