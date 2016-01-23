using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
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

	
}