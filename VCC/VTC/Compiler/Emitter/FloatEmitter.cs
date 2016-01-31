using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using Vasm.x86.x87;
namespace VTC
{
   public class FloatEmitter  : ReferenceSpec
    {
  
       public FloatEmitter(MemberSpec ms,int off, ReferenceKind k) : base( k)
       {
      
           Member = ms;
           Offset = off;
       }
       public FloatEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
           : this(ms,off,k)
       {
           Register = reg;
        
       }

       public override bool EmitFromStack(EmitContext ec)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Float Store Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new FloatStore() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 32});
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Float Store Var @BP" + Offset);
               ec.EmitInstruction(new FloatStore() { DestinationReg = EmitContext.BP, DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 32 });
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Float Store REG @" + Register.ToString() + Offset);
               ec.EmitInstruction(new FloatStore() { DestinationReg = Register, DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 32 });
           }
           else
           {
               ec.EmitComment("Float Store Parameter @BP " + Offset);
               ec.EmitInstruction(new FloatStore() { DestinationReg = EmitContext.BP, DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 32 });
           }
           return true;
       }
       public override bool EmitToStack(EmitContext ec)
       {

           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Float Load Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new FloatLoad() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 32 });
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Float Load Var @BP" + Offset);
               ec.EmitInstruction(new FloatLoad() { DestinationReg = EmitContext.BP, DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 32 });
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Float Load Var @" + Register.ToString() + Offset);
               ec.EmitInstruction(new FloatLoad() { DestinationReg = Register, DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 32 });

           }
           else
           {
               ec.EmitComment("Float Load Parameter @BP " + Offset);
               ec.EmitInstruction(new FloatLoad() { DestinationReg = EmitContext.BP, Size = 32, DestinationDisplacement = Offset, DestinationIsIndirect = true });
           }
           return true;
       }
       public override bool LoadEffectiveAddress(EmitContext ec)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("AddressOf Field ");

               ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, SourceDisplacement = Offset, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
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
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Float Load ValueOf Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitLoadFloat(EmitContext.SI,32, true);

           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Float Load ValueOf @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitLoadFloat(EmitContext.SI, 32, true);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Float Load ValueOf @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitLoadFloat(EmitContext.SI, 32, true);
           }
           else
           {
               ec.EmitComment("Float Load ValueOf @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });

               ec.EmitLoadFloat(EmitContext.SI, 32, true);
           }
           return true;
       }
       public override bool ValueOfStack(EmitContext ec)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Float Store ValueOf Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitStoreFloat(EmitContext.SI, 32, true);
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Float Store ValueOf Stack @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitStoreFloat(EmitContext.SI, 32, true);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Float Store ValueOf Stack @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitStoreFloat(EmitContext.SI, 32, true);

           }
           else
           {
               ec.EmitComment("Float Store ValueOf  Stack @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });

               ec.EmitStoreFloat(EmitContext.SI, 32, true);

           }
           return true;
       }
       public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Float Load ValueOf Access Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitLoadFloat(EmitContext.SI, 32, true, off);

           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Float Load ValueOf Access @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitLoadFloat(EmitContext.SI, 32, true, off);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Float Load ValueOf Access @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitLoadFloat(EmitContext.SI, 32, true, off);

           }
           else
           {
               ec.EmitComment("Float Load ValueOf Access @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });

               ec.EmitLoadFloat(EmitContext.SI, 32, true, off);
           }
           return true;
       }
       public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Float Store ValueOf Access Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitStoreFloat(EmitContext.SI,32, true, off);

           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Float Store ValueOf Stack Access @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitStoreFloat(EmitContext.SI, 32, true, off);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Float Store ValueOf Stack Access @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitStoreFloat(EmitContext.SI, 32, true, off);


           }
           else
           {
               ec.EmitComment("Float Store ValueOf Access Stack @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });

               ec.EmitStoreFloat(EmitContext.SI, 32, true, off);
           }
           return true;
       }
    /*    public override bool EmitFromStack(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("Pop Float Field @" + Member.Signature.ToString() + " " + Offset);
                return PopAllToRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.Size, Offset);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("Pop Float Var [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PopAllToRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("Pop Float Var [TypeOf " + Member.MemberType.Name + "] @" + Register.ToString() + Offset);
                return PopAllToRegister(ec, Register, Member.MemberType.Size, Offset);
            }
            else
            {
                ec.EmitComment("Pop Float Parameter [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PopAllToRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);
            }

        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("Push Float Field @" + Member.Signature.ToString() + " " + Offset);
                return PushAllFromRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.Size, Offset); // FieldOffset Removed
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("Push Float Var [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PushAllFromRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("Push Float Var [TypeOf " + Member.MemberType.Name + "] @" + Register.ToString() + Offset);
                return PushAllFromRegister(ec, Register, Member.MemberType.Size, Offset);
            }
            else // is composed type
            {
                ec.EmitComment("Push Float Parameter [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
                return PushAllFromRegister(ec, EmitContext.BP, Member.MemberType.Size, Offset);


            }
        }
    


        public override bool LoadEffectiveAddress(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("AddressOf Float Field ");

                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, SourceDisplacement = Offset, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("AddressOf Float @BP" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("AddressOf Float @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else
            {
                ec.EmitComment("AddressOf Float @BP+" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
     
           
           if(ReferenceType == ReferenceKind.Field){

               ec.EmitComment("Push ValueOf Float Field [TypeOf " + Member.MemberType.Name + "] ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
               PushAllFromRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.BaseType.Size, Offset);
            }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {

               ec.EmitComment("Push ValueOf Float Var [TypeOf " + Member.MemberType.Name + "] @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               PushAllFromRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Push ValueOf Float Var [TypeOf " + Member.MemberType.Name + "] @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               PushAllFromRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);
           }
           else
           {
               ec.EmitComment("ValueOf Float @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
               PushAllFromRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);
           }
            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {

            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Float Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
               return PopAllToRef(ec, ElementReference.New(Member.Signature.ToString()), Member.MemberType.BaseType.Size, Offset);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Float Stack @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                return PopAllToRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Float Stack @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                PopAllToRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);

            }
            else
            {
                ec.EmitComment("ValueOf Float Stack @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
     
                PopAllToRegister(ec, EmitContext.SI, Member.MemberType.BaseType.Size, 0);

            }
         
            return true;
        }

        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {

      
            if(ReferenceType == ReferenceKind.Field){

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
                ec.EmitComment("Push ValueOf Float Access Field [TypeOf " + mem.Name + "] ");
                PushAllFromRef(ec, ElementReference.New(Member.Signature.ToString()), mem.Size, off);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {

                ec.EmitComment("Push ValueOf Float Access Var [TypeOf " + mem.Name + "] @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                return  PushAllFromRegister(ec, EmitContext.SI, mem.Size, off);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {


                ec.EmitComment("Push ValueOf Float Access Var [TypeOf " + mem.Name + "] @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                PushAllFromRegister(ec, EmitContext.SI, mem.Size, off);

            }
            else
            {
                ec.EmitComment("ValueOf Access Float @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                PushAllFromRegister(ec, EmitContext.SI, mem.Size, off);
            }
            return true;
        }
        public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
        {
           
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Access Float Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !Member.memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Member.Signature.ToString()) });
                PopAllToRef(ec, ElementReference.New(Member.Signature.ToString()), mem.Size, off + Offset);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Stack Float Access @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
                return PopAllToRegister(ec, EmitContext.SI, mem.Size, off);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Stack Float Access @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                PopAllToRegister(ec, EmitContext.SI, mem.Size, off);
            }
            else
            {
                ec.EmitComment("ValueOf Access Float Stack @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });      
                PopAllToRegister(ec, EmitContext.SI, mem.Size, off);
            }
            return true;
        }*/
    }
}
