using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
   public class WordEmitter  : ReferenceSpec
    {
    
       public WordEmitter(MemberSpec ms,int off, ReferenceKind k) : base( k)
       {
      
           Member = ms;
           Offset = off;
       }
       public WordEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
           : this(ms,off,k)
       {
           Register = reg;
        
       }

       public override bool EmitFromStack(EmitContext ec)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Pop Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new Pop() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = memberType.SizeInBits });
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Pop Var @BP" +  Offset);
               ec.EmitInstruction(new Pop() { Size = memberType.SizeInBits, DestinationReg = EmitContext.BP, DestinationDisplacement =  Offset, DestinationIsIndirect = true });
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Pop Var @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Pop() { Size = memberType.SizeInBits, DestinationReg = Register, DestinationDisplacement = Offset, DestinationIsIndirect = true });
           }
           else
           {
               ec.EmitComment("Pop Parameter @BP " +  Offset);
               ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement =  Offset, DestinationIsIndirect = true });
           }
           return true;
       }
       public override bool EmitToStack(EmitContext ec)
       {

           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Push Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 16 });
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Push Var @BP" +  Offset);
               ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, DestinationDisplacement =  Offset, DestinationIsIndirect = true, Size = 16 });
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Push Var @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Push() { DestinationReg = Register, DestinationDisplacement = Offset, DestinationIsIndirect = true, Size = 16 });

           }
           else
           {
               ec.EmitComment("Push Parameter @BP " +  Offset);
               ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement =  Offset, DestinationIsIndirect = true });
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

               ec.EmitComment("AddressOf @BP" +  Offset);
               ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
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
               ec.EmitComment("AddressOf @BP+" +  Offset);
               ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
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
               ec.EmitComment("ValueOf @BP" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
               ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
           }
           else
           {
               ec.EmitComment("ValueOf @BP+" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });

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
               ec.EmitComment("ValueOf Stack @BP" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
               ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf Stack @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

           }
           else
           {
               ec.EmitComment("ValueOf  Stack @BP+" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
              
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
               ec.EmitComment("ValueOf Access @BP" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
               ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf Access @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else
           {
               ec.EmitComment("ValueOf Access @BP+" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
          
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
               ec.EmitComment("ValueOf Stack Access @BP" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
               ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("ValueOf Stack Access @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
               ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);


           }
           else
           {
               ec.EmitComment("ValueOf Access Stack @BP+" +  Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement =  Offset });
           
                   ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);
           }
           return true;
       }
    }
}
