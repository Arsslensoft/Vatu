using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VCC.Core
{
    public class ByteConstant : CharConst
    {
        byte _value;
        public ByteConstant(byte value, Location loc)
            : base(BuiltinTypeSpec.Byte, loc)
        {
            _value = value;
        }


        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (uint)_value, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec,RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg,SourceValue = (uint)_value, Size = 16 });
            return true;
        }
    }
    public class StringConstant : StringConst
    {
        public static int id = 0;
        public VarSpec ConstVar { get; set; }
        string _value;
        public StringConstant(string value, Location loc)
            : base(BuiltinTypeSpec.Byte.MakePointer(), loc)
        {
            _value = value;
        }


        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitData(new DataMember(ConstVar.Signature.ToString(), Encoding.ASCII.GetBytes(_value)), ConstVar);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            ConstVar = new VarSpec("STRC_" + id, rc.CurrentMethod, Type, loc);

            id++;
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.AX, Size = 16, SourceRef = ElementReference.New(ConstVar.Signature.ToString())});
            ec.EmitInstruction(new Push() { DestinationReg = RegistersEnum.AX, Size = 16 });

            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.AX, Size = 16, SourceRef = ElementReference.New(ConstVar.Signature.ToString()) });
            return true;
        }
    }
    public class SByteConstant : CharConst
    {
        sbyte _value;
        public SByteConstant(sbyte value, Location loc)
            : base(BuiltinTypeSpec.SByte, loc)
        {
            _value = value;
        }


        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (uint)_value, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (uint)_value, Size = 16 });
            return true;
        }
    }
    public class IntConstant : IntegralConst
    {
        int _value;
        public IntConstant(short value, Location loc)
            : base(BuiltinTypeSpec.Int, loc)
        {
            _value = value;
        }


        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }

        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (uint)_value, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (uint)_value, Size = 16 });
            return true;
        }
    }
    public class UIntConstant : IntegralConst
    {
        uint _value;
        public UIntConstant(ushort value, Location loc)
            : base(BuiltinTypeSpec.UInt, loc)
        {
            _value = value;
        }


        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (uint)_value, Size = 16 });
            return true;
        }

        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (uint)_value, Size = 16 });
            return true;
        }
    }
    public class BoolConstant : BoolConst
    {
        bool _value;
        public BoolConstant(bool value, Location loc)
            : base(BuiltinTypeSpec.Bool, loc)
        {
            _value = value;
        }


        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }

        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = _value?(uint)255:0, Size = 16 });
            return true;
        }

        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = _value?(uint)255:0, Size = 16 });
            return true;
        }
    }
    public class NullConstant : ConstantExpression
    {
        int _value;
        public NullConstant(Location loc)
            : base(BuiltinTypeSpec.Int, loc)
        {
            _value = 0;
        }


        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }

        public override bool Emit(EmitContext ec)
        {

            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = 0, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue =0, Size = 16 });
            return true;
        }
    }



    // Recognized by the parser
    public class IntegralConst : ConstantExpression
    {
        public IntegralConst(TypeSpec tp, Location loc)
            : base(tp, loc)
        {

        }
    }
    public class StringConst : ConstantExpression
    {
        public StringConst(TypeSpec tp, Location loc)
            : base(tp, loc)
        {

        }
    }
    public class CharConst : ConstantExpression
    {
        public CharConst(TypeSpec tp, Location loc)
            : base(tp, loc)
        {

        }
    }
    public class BoolConst : ConstantExpression
    {
        public BoolConst(TypeSpec tp, Location loc)
            : base(tp, loc)
        {

        }
    }
}
