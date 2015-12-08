using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VJay;

namespace VCC
{
   public class CharConstant : ConstantExpression
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
   public class StringConstant : ConstantExpression
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
   public class ByteConstant : ConstantExpression
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
   }
   public class ShortConstant : ConstantExpression
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



   public class UShortConstant : ConstantExpression
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
   public class IntConstant : ConstantExpression
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

   public class UIntConstant : ConstantExpression
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
   public class LongConstant : ConstantExpression
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

   public class ULongConstant : ConstantExpression
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
   public class FloatConstant : ConstantExpression
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


   public class BoolConstant : ConstantExpression
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
   public class DoubleConstant : ConstantExpression
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
   public class ExtendedConstant : ConstantExpression
   {
       decimal _value;
       public ExtendedConstant(decimal value, Location loc)
           : base(BuiltinTypeSpec.Extended, loc)
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
 
}
