﻿using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VCC.Core
{
    /// <summary>
    /// Basic Literal
    /// </summary>
    public class Literal : Expr
    {
        protected ConstantExpression _value;
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
            else if (_strvalue.Length == 1 && char.IsLetter(_strvalue[0]))
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
   
    }

    [Terminal("StringLiteral")]
    public class StringLiteral : Literal
    {
     
        public StringLiteral(string value) 
            : base(value)
        {
            _value = new StringConstant(value.Remove(0,1).Remove(value.Length -2,1) + "\0", CompilerContext.TranslateLocation(position));

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
        }
      
    }
    [Terminal("CharLiteral")]
    public class CharLiteral : Literal
    {
      
        public CharLiteral(string value)
            : base(value, true)
        {
            _value = new ByteConstant(Encoding.UTF8.GetBytes(value)[0], CompilerContext.TranslateLocation(position));
  
        }



    }
    [Terminal("BooleanLiteral")]
    public class BooleanLiteral : Literal
    {
       
        public BooleanLiteral(string value)
            : base(value)
        {
            _value = new BoolConstant(bool.Parse(value), CompilerContext.TranslateLocation(position));
        }



    }
    [Terminal("NullLiteral")]
    public class NullLiteral : Literal
    {
      
        public NullLiteral(string value)
            : base(value)
        {
            _value = new NullConstant(CompilerContext.TranslateLocation(position));
        }



    }
    [Terminal("HexLiteral")]
    public class HexLiteral : Literal
    {
     
        public HexLiteral(string value)
            : base(value)
        {
            
            if (value.Length <= 4)
                _value = new ByteConstant(Convert.ToByte(value, 16), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 6)
                _value = new UIntConstant(Convert.ToUInt16(value, 16), CompilerContext.TranslateLocation(position));
            else throw new ArgumentOutOfRangeException(value + "Hex value cannot be larger than 16 bits");
           
        }



    }
    [Terminal("OctLiteral")]
    public class OctLiteral : Literal
    {
    
        public OctLiteral(string value)
            : base(value)
        {

            if (value.Length <= 4)
                _value = new ByteConstant(Convert.ToByte(value, 8), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 6)
                _value = new UIntConstant(Convert.ToUInt16(value, 8), CompilerContext.TranslateLocation(position));
        
            else throw new ArgumentOutOfRangeException(value + "Hex value cannot be larger than 16 bits");

        }



    }
    [Terminal("DecLiteral")]
    public class DecLiteral : Literal
    {
      
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
                        _value = new ByteConstant(byte.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    case TypeCode.SByte:
                        _value = new SByteConstant(sbyte.Parse(value), CompilerContext.TranslateLocation(position));
                        break;

                    case TypeCode.Int16:
                        _value = new IntConstant(short.Parse(value), CompilerContext.TranslateLocation(position));
                        break;
                    case TypeCode.UInt16:
                        _value = new UIntConstant(ushort.Parse(value), CompilerContext.TranslateLocation(position));
                        break;

                 
                    

                    
                }
            }
            else if (ulong.TryParse(value, out v))
            {
                if (v <= byte.MaxValue)
                    _value = new ByteConstant((byte)v, CompilerContext.TranslateLocation(position));
                else if (v <= ushort.MaxValue)
                    _value = new UIntConstant((ushort)v, CompilerContext.TranslateLocation(position));
                else throw new ArgumentOutOfRangeException(value + "Decimal value cannot be larger than 16 bits");
              
            }
            else throw new ArgumentOutOfRangeException(value + "Decimal value cannot be larger than 16 bits");
        }



    }






}