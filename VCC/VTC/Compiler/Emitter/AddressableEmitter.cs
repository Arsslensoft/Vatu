using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
   public class AddressableEmitter  : ReferenceSpec
    {
       public override int Offset
       {
           get
           {
               return base.Offset;
           }
           set
           {
               base.Offset = value;
               if(BaseEmitter != null)
                 BaseEmitter.Offset = value+2;
           }
       }
       public int SegmentOffset
       {
           get { return Offset; }
       }
       public int PointerOffset
       {
           get { return Offset+2; }
       }
       public AddressableEmitter(MemberSpec ms,int off, ReferenceKind k) : base( k)
       {
      
           Member = ms;
           Offset = off;
       }
       public AddressableEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
           : this(ms,off,k)
       {
           Register = reg;
        
       }
       void SaveSegment(EmitContext ec,RegistersEnum dst)
       {
           if (ReferenceType == ReferenceKind.Field)
           {
               ec.EmitComment("Save Segment Field @" + Signature.ToString() + " " + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceRef = ElementReference.New(Signature.ToString()), SourceDisplacement = Offset, SourceIsIndirect = true, Size = 16 });
           }
           else if (ReferenceType == ReferenceKind.LocalVariable)
           {
               ec.EmitComment("Save Segment Var @BP" + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = EmitContext.BP, SourceDisplacement = Offset, SourceIsIndirect = true, Size = 16 });
           }
           else if (ReferenceType == ReferenceKind.Register)
           {
               ec.EmitComment("Save Segment Var @" + Register.ToString() + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = Register, SourceDisplacement = Offset, SourceIsIndirect = true, Size = 16 });

           }
           else
           {
               ec.EmitComment("Save Segment Parameter @BP " + Offset);
               ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = EmitContext.BP, Size = 16, SourceDisplacement = Offset, SourceIsIndirect = true });
           }
           ec.EmitPush(RegistersEnum.DS);
           ec.EmitMovFromRegister(RegistersEnum.DS, dst);
           ec.EmitPop(RegistersEnum.CX);
       }
       void RestoreSegment(EmitContext ec, RegistersEnum dst)
       {
           ec.EmitMovFromRegister(RegistersEnum.DS, dst);
       }

       public override bool EmitFromStack(EmitContext ec)
       {

           ec.EmitComment("Pop Reference @ " + BaseEmitter.ToString() + " " + Offset + ", INITIAL IDX : " + InitialIndex);
           SaveSegment(ec, RegistersEnum.CX);

           if (InitialIndex != Offset)
           {
               BaseEmitter.Offset = InitialIndex+2;

               BaseEmitter.ValueOfStackAccess(ec, Offset - InitialIndex - 2, memberType);
           }
           else
           {
               BaseEmitter.Offset = Offset+2;

               BaseEmitter.ValueOfStack(ec);
           }
           RestoreSegment(ec, RegistersEnum.CX);
           return true;
       }
       public override bool EmitToStack(EmitContext ec)
       {

           ec.EmitComment("Push Reference @ " + BaseEmitter.ToString() + " " + Offset + ", INITIAL IDX : " + InitialIndex);
           SaveSegment(ec, RegistersEnum.CX);
           if (InitialIndex != Offset)
           {
               BaseEmitter.Offset = InitialIndex+2;

               BaseEmitter.ValueOfAccess(ec, Offset - InitialIndex - 2, memberType);
           }
           else
           {
               BaseEmitter.Offset = Offset+2;
        
               BaseEmitter.ValueOf(ec);
           }

           RestoreSegment(ec, RegistersEnum.CX);
           return true;
       }
       public override bool LoadEffectiveAddress(EmitContext ec)
       {
          
           return BaseEmitter.EmitToStack(ec);
       }


       public override bool ValueOf(EmitContext ec)
       {
           SaveSegment(ec, RegistersEnum.CX);
           BaseEmitter.ValueOf(ec);
           RestoreSegment(ec, RegistersEnum.CX);
           return true;
       }
       public override bool ValueOfStack(EmitContext ec)
       {
           SaveSegment(ec, RegistersEnum.CX);
           BaseEmitter.ValueOfStack(ec);
        
           return true;
       }
       public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
       {
           SaveSegment(ec, RegistersEnum.CX);
           BaseEmitter.ValueOfAccess(ec,off,mem);
           RestoreSegment(ec, RegistersEnum.CX);
           return true;
       }
       public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
       {
           SaveSegment(ec, RegistersEnum.CX);
           BaseEmitter.ValueOfStackAccess(ec, off, mem);
           RestoreSegment(ec, RegistersEnum.CX);
           return true;
       }
    }
}
