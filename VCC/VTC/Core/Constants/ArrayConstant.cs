using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class ArrayConstant : ConstantExpression
    {
        internal List<byte> _value;
        public ArrayConstant(byte[] b, Location loc)
            : base(BuiltinTypeSpec.Byte.MakeArray(), loc)
        {
            _value = new List<byte>();
            _value .AddRange(b);
            if (_value[_value.Count - 1] == 0)
                _value.RemoveAt(_value.Count - 1);
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
   
	
	
	
}