using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VJay;

namespace VCC
{
    [Terminal("CharLiteral")]//To check
    public class CharLiteral : CharConstant
    {
        private readonly byte _value;
        public CharLiteral(byte value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }
   
    [Terminal("StringLiteral")]
    public class StringLiteral : StringConstant
    {
        private readonly string _value;
        public StringLiteral(string value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class ByteLiteral : ByteConstant
    {
        private readonly byte _value;
        public ByteLiteral(byte value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class ShortLiteral : ShortConstant
    {
        private readonly short _value;
        public ShortLiteral(short value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class UShortLiteral : UShortConstant
    {
        private readonly ushort _value;
        public UShortLiteral(ushort value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class IntLiteral : IntConstant
    {
        private readonly int _value;
        public IntLiteral(int value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class UIntLiteral : UIntConstant
    {
        private readonly uint _value;
        public UIntLiteral(uint value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class LongLiteral : LongConstant
    {
        private readonly long _value;
        public LongLiteral(long value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class ULongLiteral : ULongConstant
    {
        private readonly ulong _value;
        public ULongLiteral(ulong value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class FloatLiteral : FloatConstant
    {
        private readonly float _value;
        public FloatLiteral(float value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class BoolLiteral : BoolConstant
    {
        private readonly bool _value;
        public BoolLiteral(bool value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }
    
    [Terminal("")]//To check
    public class DoubleLiteral : DoubleConstant
    {
        private readonly double _value;
        public DoubleLiteral(double value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }

    [Terminal("")]//To check
    public class ExtendedLiteral : ExtendedConstant
    {
        private readonly decimal _value;
        public ExtendedLiteral(decimal value, Location loc)
            : base(value, loc)
        {
            _value = value;
        }
    }


    [Terminal("")]//To check
    public class NullLiteral : NullConstant
    {
        
        public NullLiteral(Location loc)
            : base(loc)
        {
        
        }
    }

}
