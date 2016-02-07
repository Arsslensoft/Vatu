using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
    class HostedMatrixEmitter : ReferenceSpec
    {

        public HostedMatrixEmitter(MemberSpec ms, int off, ReferenceKind k)
            : base(k)
        {

            Member = ms;
            Offset = off;
        }
        public HostedMatrixEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
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
                ec.EmitComment("Push  Hosted Multi-Dim Array  Field @" + Signature.ToString() + " " + Offset);
                LoadEffectiveAddress(ec);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("Push  Hosted Multi-Dim Array  Var @BP" + Offset);
                LoadEffectiveAddress(ec);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("Push  Hosted Multi-Dim Array  Var @" + Register.ToString() + Offset);
                LoadEffectiveAddress(ec);

            }
            else
            {
                ec.EmitComment("Push  Hosted Multi-Dim Array  Parameter @BP " + Offset);
                LoadEffectiveAddress(ec);
                //  ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement = Offset, DestinationIsIndirect = true });
            }
            return true;
        }
        public override bool LoadEffectiveAddress(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("AddressOf  Hosted Multi-Dim Array  Field ");

                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, SourceDisplacement = Offset, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("AddressOf  Hosted Multi-Dim Array  @BP" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("AddressOf  Hosted Multi-Dim Array  @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = Register, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            else
            {
                ec.EmitComment("AddressOf  Hosted Multi-Dim Array  @BP+" + Offset);
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                ec.EmitPush(EmitContext.SI);
            }
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf  Hosted Multi-Dim Array  Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
                ec.EmitPush(EmitContext.SI, 16, false);

            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf  Hosted Multi-Dim Array  @BP" + Offset);

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitPush(EmitContext.SI, 16, false);
            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf  Hosted Multi-Dim Array  @" + Register.ToString() + Offset);

                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitPush(EmitContext.A, 16, false);

            }
            else
            {
                ec.EmitComment("ValueOf  Hosted Multi-Dim Array  @BP+" + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
                 ec.EmitPush(EmitContext.SI, 16, false);


            }
            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {
            throw new NotSupportedException();
        }
        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {
            if (ReferenceType == ReferenceKind.Field)
            {
                ec.EmitComment("ValueOf Access  Hosted Multi-Dim Array  Field ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = !memberType.IsArray, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });

                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.SI, SourceDisplacement = off });
                ec.EmitPush(EmitContext.SI);


            }
            else if (ReferenceType == ReferenceKind.LocalVariable)
            {
                ec.EmitComment("ValueOf Access  Hosted Multi-Dim Array  Var ");
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = false, Size = 16, SourceReg = EmitContext.BP });
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SI, SourceValue = (ushort)(-Offset), Size = 16 });
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.SI, SourceDisplacement = off });
                ec.EmitPush(EmitContext.SI);

            }
            else if (ReferenceType == ReferenceKind.Register)
            {
                ec.EmitComment("ValueOf Access  Hosted Multi-Dim Array  @" + Register.ToString() + Offset);
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceIsIndirect = false, Size = 16, SourceReg = Register });
                ec.EmitInstruction(new Add() { DestinationReg = EmitContext.A, SourceValue = (ushort)(Offset), Size = 16 });
                ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.A, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.A, SourceDisplacement = off });
                ec.EmitPush(EmitContext.A);

            }
            else
            {
                ec.EmitComment("ValueOf Access  Hosted Multi-Dim Array  @BP+" + Offset);
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