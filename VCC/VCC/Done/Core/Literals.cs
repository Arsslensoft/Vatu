using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VJay;

namespace VCC
{
    /// <summary>
    /// Basic Literal
    /// </summary>
    public class Literal : Expression
    {
        protected string _strvalue;
        protected bool _unsigned;
        protected bool _numeric;

        public bool HasSuffix
        {
            get { return GetSuffix() != null; }
        }
        public bool IsNumber { get { return _numeric; } }
        public bool IsString { get { return !_numeric; } }
        public Literal(string value, bool num = false)
        {
            _strvalue = value;
            _numeric = num;
        }
    
        TypeCode real_type_suffix(char c)
		{
            
			switch (c){
			case 'F': case 'f':
				return TypeCode.Single;
			case 'D': case 'd':
				return TypeCode.Double;
		
          
			default:
      
				return TypeCode.Empty;
			}
		}
		TypeCode integer_type_suffix (char c,char nxt)
		{
			
					switch (c)
                    {
					case 'U': case 'u':
						_unsigned = true;
                            return integer_type_suffix(nxt,'#');
					case 'i':
					case 'I': 
						return  (_unsigned)? TypeCode.UInt32:TypeCode.Int32;
		            case 's':
					case 'S': 
						return  (_unsigned)? TypeCode.UInt16:TypeCode.Int16;
                    case 'b':
					case 'B': 
						return  (_unsigned)? TypeCode.Byte:TypeCode.SByte;
					case 'l':
					case 'L': 
						return  (_unsigned)? TypeCode.UInt64:TypeCode.Int64;
						
					default:
					   return real_type_suffix(c);
					}

		}
        public string GetSuffix()
        {
            if (_strvalue == null)
                return null;

            if (_strvalue.Length > 2)
            {

                string suffix = _strvalue.Substring(0, _strvalue.Length - 2);
                if(char.IsLetter(suffix[0]) && char.IsLetter(suffix[1]))
                    return suffix;
                else return null;
            }
            else if (_strvalue.Length == 1)
            {

                string suffix = _strvalue.Substring(0, _strvalue.Length - 1);
                if(char.IsLetter(suffix[0]))
                    return suffix;
                else return null;
            }
            else return null;
        }
        public bool IsValidNumber(string num)
        {
     
            long l;
            decimal d;
            string suffix = "";
            if (num.Contains("."))
            {
                if((suffix=GetSuffix()) == null)
                  return decimal.TryParse(num, out d);
                else return decimal.TryParse(num.Substring(0,num.Length-suffix.Length), out d);
            }
            else
            {
                if ((suffix=GetSuffix()) == null)
                    return long.TryParse(num, out l);
                else return long.TryParse(num.Substring(0, num.Length - suffix.Length), out l);
            }
        }
        public TypeCode GetTypeBySuffix()
        {
            string suffix = GetSuffix();
            // decimals first
            if (_strvalue.Contains(".") && suffix != null && suffix.Length == 1)
                    return real_type_suffix(suffix[0]);
            else if (!_strvalue.Contains(".") && suffix != null && suffix.Length <= 2)
            {
                if (suffix.Length == 1)
                    return integer_type_suffix(suffix[0], '#');

                else
                    return integer_type_suffix(suffix[0], suffix[1]);
            }
            else return TypeCode.Empty;
        }
    }


    [Terminal("StringLiteral")]
    public class StringLiteral : Literal
    {
        private readonly StringConstant _value;
        public StringLiteral(string value) 
            : base(value)
        {
            _value = new StringConstant(value, CompilerContext.TranslateLocation(position));
        }

        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);
   
        }
      
    }
    [Terminal("CharLiteral")]
    public class CharLiteral : Literal
    {
        private readonly CharConstant _value;
        public CharLiteral(string value)
            : base(value, true)
        {
            _value = new CharConstant(Encoding.UTF8.GetBytes(value)[0], CompilerContext.TranslateLocation(position));
  
        }
        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);

        }


    }
    [Terminal("BooleanLiteral")]
    public class BooleanLiteral : Literal
    {
        private readonly BoolConstant _value;
        public BooleanLiteral(string value)
            : base(value)
        {
            _value = new BoolConstant(bool.Parse(value), CompilerContext.TranslateLocation(position));
        }
        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);

        }

    }
    [Terminal("NullLiteral")]
    public class NullLiteral : Literal
    {
        private readonly NullConstant _value;
        public NullLiteral(string value)
            : base(value)
        {
            _value = new NullConstant(CompilerContext.TranslateLocation(position));
        }
        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);

        }

    }
    [Terminal("HexLiteral")]
    public class HexLiteral : Literal
    {
        private readonly ConstantExpression _value;
        public HexLiteral(string value)
            : base(value)
        {
            
            if (value.Length <= 4)
                _value = new CharConstant(Convert.ToByte(value, 16), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 6)
                _value = new UShortConstant(Convert.ToUInt16(value, 16), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 10)
                _value = new UIntConstant(Convert.ToUInt32(value, 16), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 18)
                _value = new ULongConstant(Convert.ToUInt64(value, 16), CompilerContext.TranslateLocation(position));
            else throw new ArgumentOutOfRangeException(value + "Hex value cannot be larger than 64 bits");
           
        }

        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);

        }
    }
    [Terminal("OctLiteral")]
    public class OctLiteral : Literal
    {
        private readonly ConstantExpression _value;
        public OctLiteral(string value)
            : base(value)
        {

            if (value.Length <= 4)
                _value = new CharConstant(Convert.ToByte(value, 8), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 6)
                _value = new UShortConstant(Convert.ToUInt16(value, 8), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 10)
                _value = new UIntConstant(Convert.ToUInt32(value, 8), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 18)
                _value = new ULongConstant(Convert.ToUInt64(value, 8), CompilerContext.TranslateLocation(position));
            else throw new ArgumentOutOfRangeException(value + "Hex value cannot be larger than 64 bits");

        }

        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);

        }
    }
    [Terminal("DecLiteral")]
    public class DecLiteral : Literal
    {
        private readonly ConstantExpression _value;
        public DecLiteral(string value)
            : base(value, true)
        {
            ulong v;
            if (HasSuffix)
            {
                TypeCode tpc = GetTypeBySuffix();
                switch (tpc)
                {
                    case TypeCode.Byte:
                        _value = new CharConstant(byte.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    case TypeCode.SByte:
                        _value = new ByteConstant(sbyte.Parse(value), CompilerContext.TranslateLocation(position));
                        break;

                    case TypeCode.Int16:
                        _value = new ShortConstant(short.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    case TypeCode.UInt16:
                        _value = new UShortConstant(ushort.Parse(value), CompilerContext.TranslateLocation(position));
                        break;

                    case TypeCode.Int32:
                        _value = new IntConstant(int.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    case TypeCode.UInt32:
                        _value = new UIntConstant(uint.Parse(value), CompilerContext.TranslateLocation(position));
                        break;

                    case TypeCode.Int64:
                        _value = new LongConstant(long.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    case TypeCode.UInt64:
                        _value = new ULongConstant(ulong.Parse(value), CompilerContext.TranslateLocation(position));
                        break;

                    case TypeCode.Single:
                        _value = new FloatConstant(float.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    case TypeCode.Double:
                        _value = new DoubleConstant(double.Parse(value), CompilerContext.TranslateLocation(position));
                        break;

                    
                }
            }
            else if (ulong.TryParse(value, out v))
            {
                if (v <= byte.MaxValue)
                    _value = new CharConstant((byte)v, CompilerContext.TranslateLocation(position));
                else if (v <= ushort.MaxValue)
                    _value = new UShortConstant((ushort)v, CompilerContext.TranslateLocation(position));
                else if (v <= uint.MaxValue)
                    _value = new UIntConstant((uint)v, CompilerContext.TranslateLocation(position));
                else
                    _value = new ULongConstant(v, CompilerContext.TranslateLocation(position));
            }
            else throw new ArgumentOutOfRangeException(value + "Decimal value cannot be larger than 64 bits");
        }

        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);

        }
    }
    [Terminal("FloatLiteral")]
    public class FloatLiteral : Literal
    {
        private readonly ConstantExpression _value;
        public FloatLiteral(string value)
            : base(value, true)
        {
            double v;
            if (HasSuffix)
            {
                TypeCode tpc = GetTypeBySuffix();
                switch(tpc)
                {
                    case TypeCode.Single:
                        _value = new FloatConstant(float.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    default:
                        _value = new DoubleConstant(double.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                }
            }
            else if (double.TryParse(value, out v))
            {
                if (v <= float.MaxValue)
                    _value = new FloatConstant((float)v, CompilerContext.TranslateLocation(position));
                else
                    _value = new DoubleConstant(v, CompilerContext.TranslateLocation(position));
            }
            else throw new ArgumentOutOfRangeException(value + "float value cannot be larger than 64 bits");
          
        }
        public override Expression DoResolve(ResolveContext rc)
        {
            return _value;
        }

        public override bool Resolve(ResolveContext rc)
        {
            if (_value == null)
                return false;

            return _value.Resolve(rc);

        }
    }

}
