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
      
         public ReferenceSpec BaseEmitter { get; set; }


         public static ReferenceSpec GetEmitter(MemberSpec ms, TypeSpec memberType, int idx, ReferenceKind k, bool access, bool reference = false)
         {
             ReferenceSpec Emitter = null;


             if (reference)
             {

                 Emitter = new ReferenceEmitter(ms, 4, ReferenceKind.Parameter);
                 if (!memberType.IsMultiDimensionArray)
                     Emitter.BaseEmitter = ReferenceSpec.GetEmitter(new ParameterSpec(ms.NS, ms.Name, (ms as ParameterSpec).MethodHost, memberType.MakePointer(), ms.Signature.Location, idx, VTC.Modifiers.NoModifier, access), memberType.MakePointer(), idx, ReferenceKind.Parameter, access);

                         //
                 else
                     Emitter.BaseEmitter = ReferenceSpec.GetEmitter(new ParameterSpec(ms.NS, ms.Name, (ms as ParameterSpec).MethodHost, memberType, ms.Signature.Location, idx, VTC.Modifiers.NoModifier, access), memberType, idx, ReferenceKind.Parameter, access);

                 //   ;
             }
             else if (memberType is ReferenceTypeSpec)
             {
                 Emitter = new AddressableEmitter(ms, idx, k);
     
                 if (ms is ParameterSpec)
                     Emitter.BaseEmitter = ReferenceSpec.GetEmitter(new ParameterSpec(ms.NS, ms.Name, (ms as ParameterSpec).MethodHost, memberType.BaseType.MakePointer(), ms.Signature.Location, idx + 2, VTC.Modifiers.NoModifier, access), memberType.BaseType.MakePointer(), idx + 2, ReferenceKind.Parameter, access);
                 else if (ms is VarSpec)
                     Emitter.BaseEmitter = ReferenceSpec.GetEmitter(new VarSpec(ms.NS, ms.Name, (ms as VarSpec).MethodHost, memberType.BaseType.MakePointer(), ms.Signature.Location, idx+2, VTC.Modifiers.NoModifier, access), memberType.BaseType.MakePointer(), idx+2, ReferenceKind.LocalVariable, access);
                 else if (ms is VarSpec)
                     Emitter.BaseEmitter = ReferenceSpec.GetEmitter(new RegisterSpec(memberType.BaseType.MakePointer(), (ms as RegisterSpec).Register, ms.Signature.Location, idx + 2, access), memberType.BaseType.MakePointer(), idx + 2, ReferenceKind.Register, access);
                 else
                     Emitter.BaseEmitter = ReferenceSpec.GetEmitter(new FieldSpec(ms.NS, ms.Name, Modifiers.NoModifier, memberType.BaseType.MakePointer(), ms.Signature.Location, access), memberType.BaseType.MakePointer(), idx + 2, ReferenceKind.Field, access);

             
             }
             else if (memberType.IsMultiDimensionArray)
             {
                 if (access)
                     Emitter = new HostedMatrixEmitter(ms, idx, k);
                 else
                     Emitter = new MatrixEmitter(ms, idx, k);
             }
             else if (memberType.IsArray)
             {
                 if (access)
                     Emitter = new HostedArrayEmitter(ms, idx, k);
                 else
                     Emitter = new ArrayEmitter(ms, idx, k);
             }
             else if (memberType.IsBuiltinType || memberType.IsDelegate || memberType.IsTemplate)
             {
                 if (memberType.IsFloat && !memberType.IsPointer)
                     Emitter = new FloatEmitter(ms, idx, k);
                 else if (memberType.IsSigned && memberType.Size == 1)
                     Emitter = new SByteEmitter(ms, idx, k);

                 else if (memberType.Size == 2)
                     Emitter = new WordEmitter(ms, idx, k);
                 else if (memberType.Size == 1)
                     Emitter = new ByteEmitter(ms, idx, k);
                 else if (memberType.IsTemplate && memberType.Size > 2)
                     Emitter = new StructEmitter(ms, idx, k);
             }
         
             else if (memberType.IsClass)
                 Emitter = new ClassEmitter(ms, idx, k);
             else if (memberType.IsForeignType)
                 Emitter = new StructEmitter(ms, idx, k);


             return Emitter;
         }
         public int InitialIndex { get; set; }
         public virtual int Offset { get; set; }
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