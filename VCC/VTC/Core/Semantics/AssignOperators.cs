using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Semantics
{
    public enum AssignOperator : byte
    {
        Equal,
        AddAssign,
        SubAssign,
        MulAssign,
        DivAssign,
        XorAssign,
        AndAssign,
        OrAssign,
        RightShiftAssign,
        LeftShiftAssign,
        Exchange

    }
}
