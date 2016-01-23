using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public enum AccessOperator : byte
    {
        ByValue,
        ByAddress,
        ByIndex,
        ByName
    }
}
