using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using VJay;

namespace VCC
{
   public class ConstantExpression : Expression
    {

       public ConstantExpression(TypeSpec type, Location loc)
           : base(type, loc)
       {

       }


       public virtual bool Resolve(ResolveContext rc)
       {
           return true;
       }
       public virtual bool Emit(EmitContext ec)
       {
           return true;
       }
       public virtual bool EmitFromStack(EmitContext ec)
       {
           return true;
       }
       public virtual bool EmitToStack(EmitContext ec)
       {
           return true;
       }

        /// <summary>
        ///  This is used to obtain the actual value of the literal
        ///  cast into an object.
        /// </summary>
        public abstract object GetValue();


        public virtual ConstantExpression ConvertImplicitly(TypeSpec type)
        {
            if (this.type == type)
                return this;

            // TODO CONSTANT CONVERSION
            return null;
        }

    /*  public static Constant CreateConstantFromValue(TypeSpec t, object v, Location loc)
        {
            switch (t.BuiltinType)
            {
                case BuiltinTypeSpec.Type.Int:
                    return new IntConstant(t, (int)v, loc);
                case BuiltinTypeSpec.Type.String:
                    return new StringConstant(t, (string)v, loc);
                case BuiltinTypeSpec.Type.UInt:
                    return new UIntConstant(t, (uint)v, loc);
                case BuiltinTypeSpec.Type.Long:
                    return new LongConstant(t, (long)v, loc);
                case BuiltinTypeSpec.Type.ULong:
                    return new ULongConstant(t, (ulong)v, loc);
                case BuiltinTypeSpec.Type.Float:
                    return new FloatConstant(t, (float)v, loc);
                case BuiltinTypeSpec.Type.Double:
                    return new DoubleConstant(t, (double)v, loc);
                case BuiltinTypeSpec.Type.Short:
                    return new ShortConstant(t, (short)v, loc);
                case BuiltinTypeSpec.Type.UShort:
                    return new UShortConstant(t, (ushort)v, loc);
                case BuiltinTypeSpec.Type.SByte:
                    return new SByteConstant(t, (sbyte)v, loc);
                case BuiltinTypeSpec.Type.Byte:
                    return new ByteConstant(t, (byte)v, loc);
                case BuiltinTypeSpec.Type.Char:
                    return new CharConstant(t, (char)v, loc);
                case BuiltinTypeSpec.Type.Bool:
                    return new BoolConstant(t, (bool)v, loc);
                case BuiltinTypeSpec.Type.Decimal:
                    return new DecimalConstant(t, (decimal)v, loc);
            }

            if (t.IsEnum)
            {
                var real_type = EnumSpec.GetUnderlyingType(t);
                return new EnumConstant(CreateConstantFromValue(real_type, v, loc), t);
            }

            if (v == null)
            {
                if (t.IsNullableType)
                    return Nullable.LiftedNull.Create(t, loc);

                if (TypeSpec.IsReferenceType(t))
                    return new NullConstant(t, loc);
            }

#if STATIC
			throw new InternalErrorException ("Constant value `{0}' has unexpected underlying type `{1}'", v, t.GetSignatureForError ());
#else
            return null;
#endif
        }*/
    }
}
