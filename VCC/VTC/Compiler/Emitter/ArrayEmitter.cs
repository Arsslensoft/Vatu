using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
    class ArrayEmitter : ReferenceSpec
    {

        public ArrayEmitter(MemberSpec ms, int off, ReferenceKind k)
            : base(k)
        {

            Member = ms;
            Offset = off;
        }
        public ArrayEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
            : this(ms, off, k)
        {
            Register = reg;

        }

        public override bool EmitFromStack(EmitContext ec)
        {

            throw new NotSupportedException();
        }
        public override bool EmitToStack(EmitContext ec)
        {

            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("Push Array Field @" + Signature.ToString() + " " + Offset);
                ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = Offset, DestinationIsIndirect = false, Size = 16 });
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("Push Array Var @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceReg = EmitContext.BP, Size = 16 });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPush(EmitContext.SI, 16);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("Push Array Var @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceReg = Register, Size = 16 });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPush(EmitContext.A, 16);

            }
            else
            {
                ec.EmitComment("Push Array Parameter @BP " + Offset);
                ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement = Offset, DestinationIsIndirect = true });
            }
            return true;
        }
        public override bool LoadEffectiveAddress(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("AddressOf Array Field ");

                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, SourceDisplacement = Offset, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("AddressOf Array @BP" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("AddressOf Array @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else
            {
                ec.EmitComment("AddressOf Array @BP+" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Array Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Array @BP" + Offset);

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Array @" + Register.ToString() + Offset);

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPush(EmitContext.A, MemberType.BaseType.SizeInBits, true);

            }
            else
            {
                ec.EmitComment("ValueOf Array @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
            }
            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Stack Array Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Stack Array @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Stack Array @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPop(EmitContext.A, MemberType.BaseType.SizeInBits, true);

            }
            else
            {
                ec.EmitComment("ValueOf  Stack Array @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });

                ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);


            }
            return true;
        }
        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Access Array Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);


            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Access Array Var ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Access Array @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPush(EmitContext.A, mem.SizeInBits, true, off);

            }
            else
            {
                ec.EmitComment("ValueOf Access Array @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);
            }
            return true;
        }
        public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Access Array Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Stack Access Array @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Stack Access Array @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPop(EmitContext.A, mem.SizeInBits, true, off);

            }
            else
            {
                ec.EmitComment("ValueOf Access Stack Array @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });

                ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

            }
            return true;
        }
    }
}