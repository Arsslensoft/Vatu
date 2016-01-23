using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	 [Terminal("OctLiteral")]
    public class OctLiteral : IntegralConst
    {
    
        public OctLiteral(string value)
            : base(value)
        {
            try{
            if (value.Length <= 4)
                _value = new ByteConstant(Convert.ToByte(value.Remove(0,2), 8), CompilerContext.TranslateLocation(position));
            else if (value.Length <= 6)
                _value = new UIntConstant(Convert.ToUInt16(value.Remove(0, 2), 8), CompilerContext.TranslateLocation(position));

            else ResolveContext.Report.Error(1, Location, "Octal value cannot be larger than 16 bits");
            }
            catch (Exception ex)
            {
                ResolveContext.Report.Error(1, Location, ex.Message);
            }

        }



    }
    
	
	
}