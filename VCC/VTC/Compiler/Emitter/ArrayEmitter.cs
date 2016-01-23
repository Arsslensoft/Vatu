using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
    class ArrayEmitter   : ReferenceSpec
    {
       
       public ArrayEmitter(MemberSpec ms,int off, ReferenceKind k) : base( k)
       {
      
           Member = ms;
           Offset = off;
       }
       public ArrayEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
           : this(ms,off,k)
       {
           Register = reg;
        
       }

       public override bool EmitFromStack(EmitContext ec)
       {
        
           return true;
       }
       public override bool EmitToStack(EmitContext ec)
       {

           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Push Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = Offset, DestinationIsIndirect = false, Size = 16 });
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Push Var @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceReg = EmitContext.BP, Size = 16 });
               ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(- Offset), Size = 16 });
               ec.EmitPush(EmitContext.SI, 16);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Push Var @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceReg = Register, Size = 16 });
               ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)( Offset), Size = 16 });
               ec.EmitPush(EmitContext.A, 16);

           }
           else
           {
               ec.EmitComment("Push Parameter @BP " + Offset);
               ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement = Offset, DestinationIsIndirect = true });
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
               ec.EmitComment("ValueOf Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("ValueOf @BP" + Offset);
             
                   ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                   ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(- Offset), Size = 16 });
                   ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf @" + Register.ToString() + Offset);
          
                   ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                   ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)( Offset), Size = 16 });
                   ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

           }
           else
           {
               ec.EmitComment("ValueOf @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                   ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
           }
           return true;
       }
       public override bool ValueOfStack(EmitContext ec)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("ValueOf Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("ValueOf Stack @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
               ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(- Offset), Size = 16 });
               ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf Stack @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
               ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)( Offset), Size = 16 });
               ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

           }
           else
           {
               ec.EmitComment("ValueOf  Stack @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
          
              ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);


           }
           return true;
       }
       public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("ValueOf Access Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);


           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
               ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(- Offset), Size = 16 });
               ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf Access @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
               ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)( Offset), Size = 16 });
               ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else
           {
               ec.EmitComment("ValueOf Access @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                   ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);
           }
           return true;
       }
       public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("ValueOf Access Field ");
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
               ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("ValueOf Stack Access @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
               ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(- Offset), Size = 16 });
               ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf Stack Access @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
               ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)( Offset), Size = 16 });
               ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else
           {
               ec.EmitComment("ValueOf Access Stack @BP+" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
        
                   ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

           }
           return true;
       }
    }
}
