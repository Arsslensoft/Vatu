using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    [Flags]
    public enum BinaryOperator
    {
        Multiply = 0 | ArithmeticMask,
        Division = 1 | ArithmeticMask,
        Modulus = 2 | ArithmeticMask,
        Addition = 3 | ArithmeticMask | AdditionMask,
        Subtraction = 4 | ArithmeticMask | SubtractionMask,

        LeftShift = 5 | ShiftMask,
        RightShift = 6 | ShiftMask,

        LessThan = 7 | ComparisonMask | RelationalMask,
        GreaterThan = 8 | ComparisonMask | RelationalMask,
        LessThanOrEqual = 9 | ComparisonMask | RelationalMask,
        GreaterThanOrEqual = 10 | ComparisonMask | RelationalMask,
        Equality = 11 | ComparisonMask | EqualityMask,
        Inequality = 12 | ComparisonMask | EqualityMask,




        BitwiseAnd = 13 | BitwiseMask,
        ExclusiveOr = 14 | BitwiseMask,
        BitwiseOr = 15 | BitwiseMask,

        LogicalAnd = 16 | LogicalMask,
        LogicalOr = 17 | LogicalMask,



        LeftRotate = 18 | ShiftMask,
        RightRotate = 19 | ShiftMask,
        UserDefine = 20,
        //
        // Operator masks
        //
        ValuesOnlyMask = ArithmeticMask - 1,
        ArithmeticMask = 1 << 5,
        ShiftMask = 1 << 6,
        ComparisonMask = 1 << 7,
        EqualityMask = 1 << 8,
        BitwiseMask = 1 << 9,
        LogicalMask = 1 << 10,
        AdditionMask = 1 << 11,
        SubtractionMask = 1 << 12,
        RelationalMask = 1 << 13,

        DecomposedMask = 1 << 19,
        NullableMask = 1 << 20
    }
}
