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
}
