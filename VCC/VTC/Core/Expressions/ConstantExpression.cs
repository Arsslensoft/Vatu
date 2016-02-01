using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
[assembly: RuleTrim("<Value> ::= '(' <Expression> ')'", "<Expression>", SemanticTokenType = typeof(VTC.Core.SimpleToken))]
namespace VTC.Core
{

    public class ConstantExpression : Expr
    {




        public ConstantExpression(TypeSpec type, Location loc)
            : base(type, loc)
        {

        }




        /// <summary>
        ///  This is used to obtain the actual value of the literal
        ///  cast into an object.
        /// </summary>
        public virtual object GetValue()
        {
            return null;
        }

        public override bool EmitToStack(EmitContext ec)
        {

            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual ConstantExpression ConvertExplicitly(ResolveContext rc, TypeSpec type, ref bool cv)
        {
            cv = false;
            if (this.type.Equals( type))
            {
                cv = true;
                return this;
            }
            if (type is RegisterTypeSpec)
                type = type.BaseType;
            if (!type.IsBuiltinType)
            {
                ResolveContext.Report.Error(12, Location, "Cannot convert type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (type.IsPointer && (this.type != BuiltinTypeSpec.Int && this.type != BuiltinTypeSpec.UInt))
            {
                ResolveContext.Report.Error(13, Location, "Cannot convert pointer type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (!CanConvertExplicitly(this.type, type))
                return this;
            else
            {
                cv = true;
                return ConstantExpression.CreateConstantFromValueExplicitly(type, this.GetValue(), loc);
            }
        }
        public void ConvertArrays(byte[] src, ref byte[] dst, int typesize)
        {
            if (src.Length % typesize != 0)
                dst = new byte[src.Length + 1];
            else dst = new byte[src.Length];

            System.Buffer.BlockCopy(src, 0, dst, 0, src.Length);


        }
        public virtual ConstantExpression ConvertImplicitly(ResolveContext rc, TypeSpec type, ref bool cv)
        {
            if (this is ArrayConstant)
            {
                cv = true;
                byte[] a = null;
                ConvertArrays((byte[])GetValue(), ref a, type.Size);
                return new ArrayConstant(a, Location);
            }

            cv = false;
            if (TypeChecker.Equals(this.type, type))
            {
                cv = true;
                return this;
            }

            if (type is RegisterTypeSpec)
                type = type.BaseType;
            if (!type.IsBuiltinType)
            {
                ResolveContext.Report.Error(12, Location, "Cannot convert type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (type.IsPointer && (this.type != BuiltinTypeSpec.Int && this.type != BuiltinTypeSpec.UInt))
            {
                ResolveContext.Report.Error(13, Location, "Cannot convert pointer type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (!CanConvert(this.type, type))
                return this;
            else
            {
                cv = true;
                return ConstantExpression.CreateConstantFromValue(type, this.GetValue(), loc);
            }
        }
        public static bool CanConvert(TypeSpec src, TypeSpec dst)
        {
            if (src.Size > dst.Size)
                return false;
            else if (!src.IsBuiltinType && !dst.IsBuiltinType)
                return (src == dst);
            else if (src.IsBuiltinType && dst.IsBuiltinType)
            {
                // try convert
                if (src.BuiltinType == BuiltinTypes.Byte)
                    return (dst.BuiltinType == BuiltinTypes.Int || dst.BuiltinType == BuiltinTypes.SByte || dst.BuiltinType == BuiltinTypes.UInt);
                else if (src.BuiltinType == BuiltinTypes.SByte)
                    return (dst.BuiltinType == BuiltinTypes.Int || dst.BuiltinType == BuiltinTypes.Byte || dst.BuiltinType == BuiltinTypes.UInt);

                return false;
            }
            else if (src.IsPointer && dst.IsBuiltinType && dst.BuiltinType == BuiltinTypes.UInt) // convert pointer to uint
                return true;
            else if (dst.IsPointer && src.IsBuiltinType && src.BuiltinType == BuiltinTypes.UInt) // convert uint to pointer
                return true;
            else return false;


        }
        public static bool CanConvertExplicitly(TypeSpec src, TypeSpec dst)
        {
            if (!src.IsBuiltinType && !dst.IsBuiltinType)
                return (src == dst);
            else if (src.IsBuiltinType && dst.IsBuiltinType)
            {
                // try convert
                return true;
            }
            else if (src.IsPointer && dst.IsBuiltinType && dst.BuiltinType == BuiltinTypes.UInt) // convert pointer to uint
                return true;
            else if (dst.IsPointer && src.IsBuiltinType && src.BuiltinType == BuiltinTypes.UInt) // convert uint to pointer
                return true;
            else return false;


        }
        public static ConstantExpression CreateConstantFromValue(TypeSpec t, object v, Location loc)
        {

            switch (t.BuiltinType)
            {
                case BuiltinTypes.Int:
                    return new IntConstant(short.Parse(v.ToString()), loc);
                case BuiltinTypes.String:
                    return new StringConstant(v.ToString(), loc);
                case BuiltinTypes.UInt:
                    return new UIntConstant(ushort.Parse(v.ToString()), loc);
                case BuiltinTypes.SByte:
                    return new SByteConstant(sbyte.Parse(v.ToString()), loc);
                case BuiltinTypes.Byte:
                    return new ByteConstant(byte.Parse(v.ToString()), loc);
                case BuiltinTypes.Bool:
                    return new BoolConstant(bool.Parse(v.ToString()), loc);
                case BuiltinTypes.Pointer:
                    return new PointerConstant(ushort.Parse(v.ToString()), loc);
                case BuiltinTypes.Float:
                    return new FloatConstant(float.Parse(v.ToString()), loc);
            }


            return null;

        }
        public static ConstantExpression CreateConstantFromValueExplicitly(TypeSpec t, object v, Location loc)
        {
            if (t.BuiltinType == BuiltinTypes.String)
                return new StringConstant(v.ToString(), loc);

            long l = long.Parse(v.ToString());
            switch (t.BuiltinType)
            {
                case BuiltinTypes.Int:
                    return new IntConstant((short)l, loc);
                case BuiltinTypes.Pointer:
                case BuiltinTypes.UInt:
                    return new UIntConstant((ushort)l, loc);
                case BuiltinTypes.SByte:
                    return new SByteConstant((sbyte)l, loc);
                case BuiltinTypes.Byte:
                    return new ByteConstant((byte)l, loc);
                case BuiltinTypes.Bool:
                    return new BoolConstant(l > 0, loc);

            }


            return null;

        }
    }

}