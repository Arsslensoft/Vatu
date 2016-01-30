using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
    class HostedArrayEmitter : ReferenceSpec
    {

        public HostedArrayEmitter(MemberSpec ms, int off, ReferenceKind k)
            : base(k)
        {

            Member = ms;
            Offset = off;
        }
        public HostedArrayEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
            : this(ms, off, k)
        {
            Register = reg;

        }

        public override bool EmitFromStack(EmitContext ec)
        {

            return false;
        }
        public override bool EmitToStack(EmitContext ec)
        {

            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("Push Hosted Array Field @" + Signature.ToString() + " " + Offset);
                LoadEffectiveAddress(ec);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("Push Hosted Array Var @BP" + Offset);
                LoadEffectiveAddress(ec);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("Push Hosted Array Var @" + Register.ToString() + Offset);
                LoadEffectiveAddress(ec);

            }
            else
            {
                ec.EmitComment("Push Hosted Array Parameter @BP " + Offset);
                LoadEffectiveAddress(ec);
              //  ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement = Offset, DestinationIsIndirect = true });
            }
            return true;
        }
        public override bool LoadEffectiveAddress(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("AddressOf Hosted Array Field ");

                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, SourceDisplacement = Offset, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("AddressOf Hosted Array @BP" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("AddressOf Hosted Array @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else
            {
                ec.EmitComment("AddressOf Hosted Array @BP+" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Hosted Array Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPush(EmitContext.SI, 16, false);

            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Hosted Array @BP" + Offset);

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPush(EmitContext.SI, 16, false);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Hosted Array @" + Register.ToString() + Offset);

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPush(EmitContext.SI, 16, false);

            }
            else
            {
                ec.EmitComment("ValueOf Hosted Array @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI,16, false);
            }
            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Stack Hosted Array Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, false);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Stack Hosted Array @BP" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, false);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Stack Hosted Array @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, false);

            }
            else
            {
                ec.EmitComment("ValueOf  Stack Hosted Array @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });

                ec.EmitPop(EmitContext.SI, 16, true);


            }
            return true;
        }
        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Access Hosted Array Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.SI, SourceDisplacement = off });
                ec.EmitPush(EmitContext.SI);


            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Access Hosted Array Var ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.SI, SourceDisplacement = off });
                ec.EmitPush(EmitContext.SI);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Access Hosted Array @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.SI, SourceDisplacement = off });
                ec.EmitPush(EmitContext.SI);

            }
            else
            {
                ec.EmitComment("ValueOf Access Hosted Array @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.SI, SourceDisplacement = off });
                ec.EmitPush(EmitContext.SI);
            }
            return true;
        }
        public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
        {
            throw new NotSupportedException();
        }
    }
}