using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Literals
{
    [Terminal("FloatLiteral")]
    public class FloatLiteral : IntegralConst
    {

        public FloatLiteral(string value)
            : base(value)
        {
            try
            {
                _value = new FloatConstant(Half.Parse(value), CompilerContext.TranslateLocation(position));

            }
            catch (Exception ex)
            {
                ResolveContext.Report.Error(1, Location, ex.Message);
            }

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return _value.DoResolve(rc);
        }


    }
}
