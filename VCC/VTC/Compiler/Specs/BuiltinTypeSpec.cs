using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Base Type
    /// </summary>
    public class BuiltinTypeSpec : TypeSpec, IEquatable<TypeSpec>
    {
        public BuiltinTypeSpec( string name, BuiltinTypes bt, TypeSpec baset = null)
            : base(Namespace.Default, name, bt, TypeFlags.Builtin, Modifiers.NoModifier, Location.Null,baset)
        {

        }
        public BuiltinTypeSpec(string name, BuiltinTypes bt, TypeFlags tf, TypeSpec baset = null)
            : base(Namespace.Default,name, bt, TypeFlags.Builtin | tf, Modifiers.NoModifier, Location.Null, baset)
        {
          
        }
        public bool Equals(TypeSpec bt)
        {
            if (bt.IsArray && this.Name == "string")
                return true;
            else
            return bt.Signature == Signature;
        }

        public override string ToString()
        {
            return GetTypeName(this);
        }
        static void Reset(BuiltinTypeSpec bt)
        {
            bt.ExtendedMethods.Clear();
            bt.ExtendedFields.Clear();
            bt.StaticExtendedMethods.Clear();
        }
        public static void ResetBuiltins()
        {
            Reset(Byte);
            Reset(SByte);
            Reset(Int);
            Reset(UInt);
            Reset(Bool);
            Reset(Null);
            Reset(Void);
            Reset(String);
            Reset(Pointer);
            Reset(Float);
        }
        public static BuiltinTypeSpec Byte = new BuiltinTypeSpec("byte", BuiltinTypes.Byte);
        public static BuiltinTypeSpec SByte = new BuiltinTypeSpec("sbyte", BuiltinTypes.SByte);
        public static BuiltinTypeSpec Int = new BuiltinTypeSpec("int", BuiltinTypes.Int);
        public static BuiltinTypeSpec UInt = new BuiltinTypeSpec("uint", BuiltinTypes.UInt);

        public static BuiltinTypeSpec Pointer = new BuiltinTypeSpec("pointer", BuiltinTypes.Pointer, TypeFlags.Pointer);
        public static BuiltinTypeSpec Bool = new BuiltinTypeSpec("bool", BuiltinTypes.Bool);
        public static BuiltinTypeSpec Void = new BuiltinTypeSpec("void", BuiltinTypes.Void);
        public static BuiltinTypeSpec String = new BuiltinTypeSpec("string", BuiltinTypes.String, TypeFlags.Pointer, Byte);
        public static BuiltinTypeSpec Null = new BuiltinTypeSpec("null", BuiltinTypes.Int, TypeFlags.Null);
        public static BuiltinTypeSpec Float = new BuiltinTypeSpec("float", BuiltinTypes.Float, TypeFlags.Builtin);
    }
   
	
	
}