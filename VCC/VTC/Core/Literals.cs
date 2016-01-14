using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
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
        public ConstantExpression Value { get { return _value; } }

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
					case 'U': 
                    case 'u':
                        return TypeCode.UInt16;
					case 'i':
					case 'I':
                        return TypeCode.Int16;
		            case 'B':
					case 'b': 
						return   TypeCode.Byte;
                    case 's':
					case 'S': 
						return TypeCode.SByte;
                   case 'A':
                    case 'a':

                        return TypeCode.Object;
					default:
					   return real_type_suffix(c);
					}

		}
        public string GetSuffix()
        {
  
            string suffix = "";
            foreach (char c in _strvalue)
            {
                if (char.IsLetter(c))
                    suffix += "" + char.ToUpper(c);
                
            }
            if (suffix != "")
                return suffix;
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
            _value = new StringConstant(value.Remove(0,1).Remove(value.Length -2,1), CompilerContext.TranslateLocation(position));

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
        {try{
            _value = new ByteConstant(Encoding.ASCII.GetBytes(value.Replace("'",""))[0], CompilerContext.TranslateLocation(position));
        }
        catch (Exception ex)
        {
            ResolveContext.Report.Error(1, Location, ex.Message);
        }
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
    [Terminal("MacroLiteral")]
    public class MacroLiteral : Literal
    {
    
        public MacroLiteral(string value)
            : base(value)
        {
            string id = value.Remove(0, 1);
            if (Preprocessor.TempSymbols.ContainsKey(id))
            {
                object d = Preprocessor.TempSymbols[id];
                if (d is bool)
                    _value = new BoolConstant((bool)d, Location);
                else if (d is decimal)
                    _value = new UIntConstant((ushort)d, Location);
                else _value = new StringConstant(d.ToString(), Location);
            }
            else
            {
                Preprocessor.Symbols.Add(id, false);
                Preprocessor.TempSymbols.Add(id, false);
                _value = new BoolConstant(false, Location);
            }
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
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
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
        }


    }
    [Terminal("HexLiteral")]
    public class HexLiteral : Literal
    {
     
        public HexLiteral(string value)
            : base(value)
        {
            try{
            if (value.Length <= 4)
                _value = new ByteConstant(Convert.ToByte(value, 16), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 6)
                _value = new UIntConstant(Convert.ToUInt16(value, 16), CompilerContext.TranslateLocation(position));
            else ResolveContext.Report.Error(1,Location, "Hex value cannot be larger than 16 bits");
            }
            catch (Exception ex)
            {
                ResolveContext.Report.Error(1, Location, ex.Message);
            }
        }



    }
    [Terminal("OctLiteral")]
    public class OctLiteral : Literal
    {
    
        public OctLiteral(string value)
            : base(value)
        {
            try{
            if (value.Length <= 4)
                _value = new ByteConstant(Convert.ToByte(value, 8), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 6)
                _value = new UIntConstant(Convert.ToUInt16(value, 8), CompilerContext.TranslateLocation(position));

            else ResolveContext.Report.Error(1, Location, "Octal value cannot be larger than 16 bits");
            }
            catch (Exception ex)
            {
                ResolveContext.Report.Error(1, Location, ex.Message);
            }

        }



    }
    [Terminal("DecLiteral")]
    public class DecLiteral : Literal
    {

        public DecLiteral(string value)
            : base(value, true)
        {
            try
            {
                long v;
                if (HasSuffix)
                {
                    int l = GetSuffix().Length;
                    TypeCode tpc = GetTypeBySuffix();
                    value = value.Remove(value.Length - l, l);
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

                        case TypeCode.Object:
                            _value = new ArrayConstant(System.Numerics.BigInteger.Parse(value).ToByteArray(), CompilerContext.TranslateLocation(position));
                            break;



                    }
                }
                else if (long.TryParse(value, out v))
                {
                    if (v >= 0)
                    {
                        if (v <= byte.MaxValue)
                            _value = new ByteConstant(byte.Parse(value), CompilerContext.TranslateLocation(position));
                        else if (v <= ushort.MaxValue)
                            _value = new UIntConstant(ushort.Parse(value), CompilerContext.TranslateLocation(position));
                        else ResolveContext.Report.Error(1, Location, "Decimal value cannot be larger than 16 bits");
                    }
                    else
                    {
                        if (v >= sbyte.MinValue)
                            _value = new SByteConstant(sbyte.Parse(value), CompilerContext.TranslateLocation(position));
                        else if (v >= short.MinValue)
                            _value = new IntConstant(short.Parse(value), CompilerContext.TranslateLocation(position));
                        else ResolveContext.Report.Error(1, Location, "Decimal value cannot be larger than 16 bits");
                    }

                }
                else ResolveContext.Report.Error(1, Location, "Decimal value cannot be larger than 16 bits");

            }
            catch(Exception ex)
            {
                ResolveContext.Report.Error(1, Location, ex.Message);
            }
        }
     

    } 
    [Terminal("BinaryLiteral")]
    public class BinaryLiteral : Literal
    {

        public BinaryLiteral(string value)
            : base(value)
        {
            try{
            value = value.Remove(0, 2);
            if (value.Length <= 8)
                _value = new ByteConstant(Convert.ToByte(value, 2), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 16)
                _value = new UIntConstant(Convert.ToUInt16(value, 2), CompilerContext.TranslateLocation(position));

            else ResolveContext.Report.Error(1, Location, "Octal value cannot be larger than 16 bits");
            }
            catch (Exception ex)
            {
                ResolveContext.Report.Error(1, Location, ex.Message);
            }

        }



    }




}
