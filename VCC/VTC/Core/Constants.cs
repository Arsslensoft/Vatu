using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class ByteConstant : ConstantExpression
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
            ec.EmitInstruction(new Push() { DestinationValue = (uint)_value, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec,RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg,SourceValue = (uint)_value, Size = 16 });
            return true;
        }
        public override string CommentString()
        {
            return _value.ToString();
        }
    }
    public class StringConstant : ConstantExpression
    {
        public static int id = 0;
        public FieldSpec ConstVar { get; set; }
        string _value;
        public StringConstant(string value, Location loc)
            : base(BuiltinTypeSpec.String, loc)
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
            if (ConstVar != null)
            {
                ec.EmitData(new DataMember(ConstVar.Signature.ToString(), Encoding.ASCII.GetBytes(_value)), ConstVar, true);
                ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(ConstVar.Signature.ToString()), Size = 16 });

            }
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
    
                ConstVar = new FieldSpec(rc.CurrentNamespace, "STRC_" + id,  Modifiers.Const, Type, loc);
          
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
    public class SByteConstant : ConstantExpression
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
    public class IntConstant : ConstantExpression
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
    public class ArrayConstant : ConstantExpression
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
    public class UIntConstant : ConstantExpression
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
    public class BoolConstant : ConstantExpression
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




}
