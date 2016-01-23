using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	 [Terminal("CharLiteral")]
    public class CharLiteral : IntegralConst
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
}