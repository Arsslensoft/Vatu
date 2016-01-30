using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	[Terminal("DecLiteral")]
    public class DecLiteral : IntegralConst
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
   
	
	
}