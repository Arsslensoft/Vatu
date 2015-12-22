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
        internal byte _value;
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
            EmitToStack(ec);
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
            ec.EmitInstruction(new Push() { DestinationValue = (uint)_value, Size = 8 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec,RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg,SourceValue = (uint)_value, Size = 8 });
            return true;
        }
        public override string CommentString()
        {
            return _value.ToString();
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

        bool decl = false;
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
            if(ConstVar != null)
                  ec.EmitData(new DataMember(ConstVar.Signature.ToString(), Encoding.ASCII.GetBytes(_value)), ConstVar, true);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!rc.IsInGlobal() && rc.IsInVarDeclaration)
            {

                ConstVar = new VarSpec(rc.CurrentNamespace, "STRC_" + id, rc.CurrentMethod, Type, loc);
                rc.LocalStackIndex -= 2;
                ConstVar.StackIdx = rc.LocalStackIndex;
            }
            else if (!rc.IsInGlobal() && !rc.IsInVarDeclaration)
            {
                ConstVar = new VarSpec(rc.CurrentNamespace, "STRC_" + id, rc.CurrentMethod, Type, loc);
            }
            id++;
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (!decl)
                Emit(ec);
      
            ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(ConstVar.Signature.ToString()), Size = 16 });

            return true;
        }
     
        public override string CommentString()
        {
            return _value.ToString();
        }
    }
    public class SByteConstant : CharConst
    {
        internal sbyte _value;
        public SByteConstant(sbyte value, Location loc)
            : base(BuiltinTypeSpec.SByte, loc)
        {
            _value = value;
        }

        public override string CommentString()
        {
            return _value.ToString();
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
            EmitToStack(ec);
      
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
        internal int _value;
        public IntConstant(short value, Location loc)
            : base(BuiltinTypeSpec.Int, loc)
        {
            _value = value;
        }

        public override string CommentString()
        {
            return _value.ToString();
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
            EmitToStack(ec);
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
    public class ArrayConstant : IntegralConst
    {
        internal List<byte> _value;
        public ArrayConstant(byte[] b, Location loc)
            : base(BuiltinTypeSpec.Byte.MakeArray(), loc)
        {
            _value = new List<byte>();
            _value .AddRange(b);
        }

        public override string CommentString()
        {
            string vals = "";
            foreach (byte b in _value)
                vals += " " + b.ToString();
            return vals;
        }
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + CommentString();
        }
        public override object GetValue()
        {
            return _value.ToArray();
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
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
       
            return true;
        }

      
    }
    public class UIntConstant : IntegralConst
    {
        internal ushort _value;
        public UIntConstant(ushort value, Location loc)
            : base(BuiltinTypeSpec.UInt, loc)
        {
            _value = value;
        }

        public override string CommentString()
        {
            return _value.ToString();
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
            EmitToStack(ec);
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
        internal bool _value;
        public BoolConstant(bool value, Location loc)
            : base(BuiltinTypeSpec.Bool, loc)
        {
            _value = value;
        }
        public override string CommentString()
        {
            return _value.ToString();
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
            EmitToStack(ec);
       
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
            ec.EmitInstruction(new Push() { DestinationValue = _value ? (uint)EmitContext.TRUE : 0, Size = 16 });
            return true;
        }

        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = ec.GetLow(rg), SourceValue = _value ? (uint)EmitContext.TRUE : 0, Size = 16 });
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

        public override string CommentString()
        {
            return _value.ToString();
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
            EmitToStack(ec);
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
