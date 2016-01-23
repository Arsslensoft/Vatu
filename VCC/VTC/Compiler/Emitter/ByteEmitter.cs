using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
   public class ByteEmitter : ReferenceSpec
    {
    
       public ByteEmitter(MemberSpec ms,int off, ReferenceKind k) : base( k)
       {
      
           Member = ms;
           Offset = off;
       }
       public ByteEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
           : this(ms,off,k)
       {
           Register = reg;
        
       }

       public override bool EmitFromStack(EmitContext ec)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Pop Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.D, Size = 16 });
               ec.EmitInstruction(new Mov() { SourceReg = ec.GetLow(EmitContext.D), DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 8 });              
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Pop Var @BP" + Offset);
               ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.D, Size = 16 });
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, Size = 8, DestinationDisplacement = Offset, DestinationIsIndirect = true, SourceReg = ec.GetLow(EmitContext.D) });
           }
           else if (ReferenceType == ReferenceKind.Register)
           {

           }
           else
           {
               ec.EmitComment("Pop Parameter @BP " + Offset);
               ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.D, Size = 16 });
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, Size = 8, DestinationDisplacement = Offset, DestinationIsIndirect = true, SourceReg = ec.GetLow(EmitContext.D) });
           }
           return true;
       }
       public override bool EmitToStack(EmitContext ec)
       {

           if (ReferenceType == ReferenceKind.Field)
           {

               ec.EmitComment("Push Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = EmitContext.D, SourceRef = ElementReference.New(Signature.ToString()), SourceDisplacement = Offset, SourceIsIndirect = true, Size = 8 });
               ec.EmitInstruction(new Push() { DestinationReg = EmitContext.D, Size = 16 });


               
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Push Var @BP" + Offset);
               ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = EmitContext.D, SourceReg = EmitContext.BP, SourceDisplacement = Offset, SourceIsIndirect = true, Size = 8 });
               ec.EmitInstruction(new Push() { DestinationReg = EmitContext.D, Size = 16 });
           }
           else if (ReferenceType == ReferenceKind.Register)
           {

           }
           else
           {
               ec.EmitComment("Push Parameter @BP " + Offset);
               ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = EmitContext.D, SourceReg = EmitContext.BP, Size = 8, SourceDisplacement = Offset, SourceIsIndirect = true });
               ec.EmitInstruction(new Push() { DestinationReg = EmitContext.D, Size = 16 });
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
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);


           }
           else if (ReferenceType == ReferenceKind.Register)
           {

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
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {

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
               ec.EmitComment("ValueOf Access @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {

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
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
               ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {

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
