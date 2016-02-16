using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC
{
    public class ReferenceEmitter : ReferenceSpec
    {


        public ReferenceEmitter(MemberSpec ms, int off, ReferenceKind k)
            : base(k)
        {

            Member = ms;
            Offset = off;
        }
        public ReferenceEmitter(MemberSpec ms, int off, ReferenceKind k, RegistersEnum reg)
            : this(ms, off, k)
        {
            Register = reg;

        }

        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitComment("Pop Reference Parameter @BP " + Offset);
            if (InitialIndex != Offset)
            {
                BaseEmitter.Offset = InitialIndex;
                return BaseEmitter.ValueOfStackAccess(ec, Offset - InitialIndex, memberType);
            }
            else
            {
                BaseEmitter.Offset = Offset;
                return BaseEmitter.ValueOfStack(ec);
            }

        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitComment("Push Reference Parameter @ " + BaseEmitter.ToString() + " " + InitialIndex.ToString() + "  " + Offset);


            if (InitialIndex != Offset)
            {
                BaseEmitter.Offset = InitialIndex;
                return BaseEmitter.ValueOfAccess(ec, Offset - InitialIndex, memberType);
            }
            else
            {
                BaseEmitter.Offset = Offset;
                return BaseEmitter.ValueOf(ec);
            }


        }
        public override bool LoadEffectiveAddress(EmitContext ec)
        {

            ec.EmitComment("AddressOf Reference @BP+" + Offset);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
            ec.EmitPush(EmitContext.SI);
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
            ec.EmitComment("ValueOf Reference @BP+" + Offset);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.DI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.DI });

            if (MemberType.BaseType.Size <= 2)
                ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
            else

                PushAllFromRegister(ec, EmitContext.SI, MemberType.BaseType.Size, 0);

            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {

            ec.EmitComment("ValueOf Reference Stack @BP+" + Offset);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.DI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.DI });

            if (MemberType.BaseType.Size <= 2)
                ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            else
                PopAllToRegister(ec, EmitContext.SI, MemberType.BaseType.Size, 0);

            return true;
        }
        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {

            ec.EmitComment("ValueOf Reference Access @BP+" + Offset);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.DI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.DI });

            if (mem.Size <= 2)
                ec.EmitPush(EmitContext.SI, mem.SizeInBits, true, off);
            else

                PushAllFromRegister(ec, EmitContext.SI, mem.Size, off);

            return true;
        }
        public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
        {

            ec.EmitComment("ValueOf Reference Access Stack @BP+" + Offset);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.DI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = Offset });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.DI });

            if (mem.Size <= 2)
                ec.EmitPop(EmitContext.SI, mem.SizeInBits, true, off);

            else
                PopAllToRegister(ec, EmitContext.SI, mem.Size, off);

            return true;
        }
    }


}