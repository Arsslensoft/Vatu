using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public enum UnaryOperator : byte
    {
        UnaryPlus, UnaryNegation, LogicalNot, OnesComplement,
        AddressOf, ValueOf, PostfixIncrement, PostfixDecrement, ZeroTest, ParityTest, UserDefined,New
    }
}
