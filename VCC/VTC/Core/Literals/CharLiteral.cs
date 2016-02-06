using VTC.Base.GoldParser.Semantic;
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

            char c = value[1];
            if (c == '\\' && value.Length > 3)
            {
                switch (value[2])
                {
                    // --- Simple character escapes
                    case '\'': c = '\''; break;
                    case '\"': c = '\"'; break;
                    case '\\': c = '\\'; break;
                    case '0': c = '\0'; break;
                    case 'a': c = '\a'; break;
                    case 'b': c = '\b'; break;
                    case 'f': c = '\f'; break;
                    case 'n': c = '\n'; break;
                    case 'r': c = '\r'; break;
                    case 't': c = '\t'; break;
                    case 'v': c = '\v'; break;
                  

                }
            }
            _value = new ByteConstant((byte)c, CompilerContext.TranslateLocation(position));
        }
        catch (Exception ex)
        {
            ResolveContext.Report.Error(1, Location, ex.Message);
        }
        }



    }
}