using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
   public class StructEmitter  : ReferenceSpec
    {
  
       public StructEmitter(MemberSpec ms,int off, ReferenceKind k) : base( k)
       {
      
           Member = ms;
           Offset = off;
       }
       public StructEmitter(MemberSpec ms, int off, ReferenceKind k,RegistersEnum reg)
           : this(ms,off,k)
       {
           Register = reg;
        
       }
       

        public override bool EmitFromStack(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("Pop Field @" + Member.Signature.ToString() + " " + Offset);
                return PopAllToRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.Size, Offset);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("Pop Var [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PopAllToRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("Pop Var [TypeOf " + Member.MemberType.Name + "] @" + Register.ToString() + Offset);
                return PopAllToRegister(ec, Register, Member.MemberType.Size, Offset);
            }
            else
            {
                ec.EmitComment("Pop Parameter [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PopAllToRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);
            }

        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("Push Field @" + Member.Signature.ToString() + " " + Offset);
                return PushAllFromRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.Size, Offset); // FieldOffset Removed
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("Push Var [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PushAllFromRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("Push Var [TypeOf " + Member.MemberType.Name + "] @" + Register.ToString() + Offset);
                return PushAllFromRegister(ec, Register, Member.MemberType.Size, Offset);
            }
            else // is composed type
            {
                ec.EmitComment("Push Parameter [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PushAllFromRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);


            }
        }
    


        public override bool LoadEffectiveAddress(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("AddressOf Field ");

                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, SourceDisplacement = Offset, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("AddressOf @BP" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("AddressOf @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else
            {
                ec.EmitComment("AddressOf @BP+" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
     
           
           if(ReferenceType == ReferenceKind.Field){

               ec.EmitComment("Push ValueOf Field [TypeOf " + Member.MemberType.Name + "] ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
               PushAllFromRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.BaseType.Size, Offset);
            }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {

               ec.EmitComment("Push ValueOf Var [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               PushAllFromRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Push ValueOf Var [TypeOf " + Member.MemberType.Name + "] @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               PushAllFromRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);
           }
           else
           {
               ec.EmitComment("ValueOf @BP+" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
               PushAllFromRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);
           }
            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {

            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
               return PopAllToRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.BaseType.Size, Offset);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Stack @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                return PopAllToRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Stack @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                PopAllToRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);

            }
            else
            {
                ec.EmitComment("ValueOf  Stack @BP+" +  Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
     
                PopAllToRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);

            }
         
            return true;
        }

        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {

      
            if(ReferenceType == ReferenceKind.Field){

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
                ec.EmitComment("Push ValueOf Access Field [TypeOf " + mem.Name + "] ");
                PushAllFromRef(ec, ElementReference.New(Member.Signature.ToString()), mem.Size, off);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {

                ec.EmitComment("Push ValueOf Access Var [TypeOf " + mem.Name + "] @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                return  PushAllFromRegister(ec, EmitContext.SI, mem.Size, off);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {


                ec.EmitComment("Push ValueOf Access Var [TypeOf " + mem.Name + "] @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                PushAllFromRegister(ec, EmitContext.SI, mem.Size, off);

            }
            else
            {
                ec.EmitComment("ValueOf Access @BP+" +  Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                PushAllFromRegister(ec, EmitContext.SI, mem.Size, off);
            }
            return true;
        }
        public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
        {
           
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Access Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
                PopAllToRef(ec, ElementReference.New(Member.Signature.ToString()), mem.Size, off + Offset);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Stack Access @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                return PopAllToRegister(ec, EmitContext.SI, mem.Size, off);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Stack Access @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                PopAllToRegister(ec, EmitContext.SI, mem.Size, off);
            }
            else
            {
                ec.EmitComment("ValueOf Access Stack @BP+" +  Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });      
                PopAllToRegister(ec, EmitContext.SI, mem.Size, off);
            }
            return true;
        }
    }
}
