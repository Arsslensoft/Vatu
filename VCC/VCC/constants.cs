using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VCC.Core
{
    public class CharConstant : CharConst
    {
       byte _value;
       public CharConstant(byte value, Location loc)
           : base(BuiltinTypeSpec.Char, loc)
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
    }
    public class StringConstant : StringConst
   {
       string _value;
       public StringConstant(string value, Location loc)
           : base(BuiltinTypeSpec.Char.MakePointer(), loc)
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
   }
    public class ByteConstant : CharConst
   {
       sbyte _value;
       public ByteConstant(sbyte value, Location loc)
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
   }
    public class ShortConstant : IntegralConst
   {
       short _value;
       public ShortConstant(short value, Location loc)
           : base(BuiltinTypeSpec.Short, loc)
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
   }
    public class UShortConstant : IntegralConst
   {
       ushort _value;
       public UShortConstant(ushort value, Location loc)
           : base(BuiltinTypeSpec.UShort, loc)
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
   }
    public class IntConstant : IntegralConst
   {
       int _value;
       public IntConstant(int value, Location loc)
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
   }
    public class UIntConstant : IntegralConst
   {
       uint _value;
       public UIntConstant(uint value, Location loc)
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
   }
    public class LongConstant : IntegralConst
   {
       long _value;
       public LongConstant(long value, Location loc)
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
   }
    public class ULongConstant : IntegralConst
   {
       ulong _value;
       public ULongConstant(ulong value, Location loc)
           : base(BuiltinTypeSpec.ULong, loc)
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
   }

    public class FloatConstant : RealConst
   {
       float _value;
       public FloatConstant(float value, Location loc)
           : base(BuiltinTypeSpec.Float, loc)
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
   }
    public class DoubleConstant : RealConst
   {
       double _value;
       public DoubleConstant(double value, Location loc)
           : base(BuiltinTypeSpec.Double, loc)
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
   }


    // Recognized by the parser
   public class RealConst : ConstantExpression
   {
       public RealConst(TypeSpec tp, Location loc)
           : base(tp, loc)
       {
    
       }
   }
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
