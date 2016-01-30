using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	 [Terminal("BinaryLiteral")]
    public class BinaryLiteral : IntegralConst
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