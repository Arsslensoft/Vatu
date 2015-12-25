using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{

    public class CompilerContext
    {
        public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
        {
            return new Location(li.Line, li.Column);
        }
    }
}
