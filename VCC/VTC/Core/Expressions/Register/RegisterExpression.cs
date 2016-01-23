using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC.Core
{
    /// <summary>
    /// Register Expr
    /// </summary>
    public class RegisterExpression : Identifier
    {
        public RegistersEnum Register { get; set; }

        RegisterIdentifier rid;
        [Rule(@"<REGISTER>       ::= <REGISTER ID>")]
        public RegisterExpression(RegisterIdentifier id)
            : base(id.Name)
        {
            rid = id;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            rid = (RegisterIdentifier)rid.DoResolve(rc);
            Register = rid.Register;
            Type = rid.Is16Bits ? RegisterTypeSpec.RegisterWord : RegisterTypeSpec.RegisterByte;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            return rid.Resolve(rc);
        }

        public override bool Emit(EmitContext ec)
        {

            ec.EmitPush(Register);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitPop(Register);
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitPush(Register);
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            if (rg != Register)
                ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceReg = Register, Size = 80 });

            return true;
        }
        public override string CommentString()
        {
            return Name;
        }


        public static void EmitAssign(EmitContext ec, RegistersEnum dst, ushort val)
        {

            ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceValue = val, Size = 80 });

        }
        public static void EmitAssign(EmitContext ec, RegistersEnum src, RegistersEnum dst)
        {
            if (src != dst)
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = src, Size = 80 });

        }
        public static void EmitOperation(EmitContext ec, InstructionWithDestinationAndSourceAndSize ins, RegistersEnum src, RegistersEnum dst, bool tostack = true)
        {
            ins.SourceReg = src;
            ins.DestinationReg = dst;

            ins.Size = 80;
            ec.EmitInstruction(ins);
            if (tostack)
                ec.EmitPush(dst);

        }
        public static void EmitOperation(EmitContext ec, InstructionWithDestinationAndSourceAndSize ins, ushort src, RegistersEnum dst)
        {
            ins.SourceValue = src;
            ins.DestinationReg = dst;

            ins.Size = 80;
            ec.EmitInstruction(ins);
            ec.EmitPush(dst);

        }
        public static void EmitUnaryOperation(EmitContext ec, InstructionWithDestinationAndSize ins, RegistersEnum dst, bool tostack = true)
        {

            ins.DestinationReg = dst;

            ins.Size = 80;
            ec.EmitInstruction(ins);
            if (tostack)
                ec.EmitPush(dst);

        }
    }
}
