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
                float f= 0;
                double d = 0;
                decimal dd = 0;
                if (HasSuffix)
                {
                    int l = GetSuffix().Length;
                    TypeCode tpc = GetTypeBySuffix();
                    value = value.Remove(value.Length - l, l);
                    switch (tpc)
                    {
                        case TypeCode.Single:
                            _value = new FloatConstant(f, CompilerContext.TranslateLocation(position));
                            break;
                    }
                }
              else if (float.TryParse(value, out f))
                        _value = new FloatConstant(f, CompilerContext.TranslateLocation(position));
              //else if (double.TryParse(value, out d))
              //      _value = new FloatConstant(d, CompilerContext.TranslateLocation(position));
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
