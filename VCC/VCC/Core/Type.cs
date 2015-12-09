using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VJay
{
    /// <summary>
    /// Member Signature [Types, member, variable]
    /// </summary>
    public class MemberSignature
    {
        public string Signature { get; set; }
        public Location Location { get; set; }

        public MemberSignature(string name,Location loc,string type, params TypeSpec[] parameters)
        {
            Signature =type  + "_" +name;
            foreach (TypeSpec param in parameters)
                Signature += "_" + param.Name;
            Location = loc;

        }
        public MemberSignature(string name, Location loc)
        {
            Signature = name;
          
            Location = loc;

        }
        public override string ToString()
        {
            return Signature;
        }

    }

    /// <summary>
    /// Builtin Types [Not like C99 Specs]
    /// </summary>
    public enum BuiltinTypes : byte
    {
        Char, // 8 bits unsigned [unsigned char]
        Byte, // 8 bits signed [signed char]
        Int, // 32 bits signed [long int]
        Long, // 64 bits signed [long long]
        Short, // 16 bits signed [short int]
        Void, // void
        Double, // 64 bits float
        Float, // 32 bits float
        ULong, // 64 bits unsigned
        UInt, // 32 bits unsigned
        UShort, // 16 bits unsigned
        Extended, // 80 bits floating point
        Bool,
        Unknown
    }
    /// <summary>
    /// Type Flags
    /// </summary>
    [Flags]
    public enum TypeFlags
    {
        Struct = 1,
        TypeDef = 1 << 1,
        Builtin = 1 << 2,
        Enum = 1 << 3,
        Pointer = 1 << 4,
        Array = 1 << 5,
        Missing = 1 << 6,
        Void = 1 << 7,
        Null = 1 << 8
    }
    /// <summary>
    /// Member Modifiers
    /// </summary>
    [Flags]
    public enum Modifiers
    {
        Static = 1,
        Extern = 1 << 1,
        NoModifier = 1 << 2,
        Prototype = 1 << 3,
        Register  = 1 << 4,
        Auto = 1 << 5,
        Volatile = 1 << 6,
        Const = 1 << 7
    }

    public abstract class MemberSpec
    {
       protected MemberSignature _sig;
       protected Modifiers _mod;
       string _name;


       public MemberSpec(string name,MemberSignature sig, Modifiers mod)
       {
           _mod = mod;
           _name = name;
           _sig = sig;
       }

       /// <summary>
       /// Member name
       /// </summary>
       public virtual string Name
       {
           get { return _name; }
       }
       /// <summary>
       /// Member Signature for struct_node_
       /// </summary>
       public MemberSignature Signature
       {
           get { return _sig; }

       }
        /// <summary>
        /// Member Modifiers
        /// </summary>
       public Modifiers Modifiers
       {
           get { return _mod; }

       }


       public bool IsExtern
       {
           get { return (_mod & Modifiers.Extern) != 0; }
       }
       public bool IsStatic
       {
           get
           {
               return (_mod & Modifiers.Static) != 0;
           }
       }
    }
        /// <summary>
    /// Basic Type Checker
    /// </summary>
    internal static class TypeChecker
    {
        public static bool IsInt(object obj)
        {
            return (Type.GetTypeCode(obj.GetType()) == TypeCode.Int32);
        }
        public static BuiltinTypes ResolveBuiltin(object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Empty:
                case TypeCode.Char:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return BuiltinTypes.Unknown;
                case TypeCode.Boolean:
                    return BuiltinTypes.Bool;
                case TypeCode.SByte:
                    return BuiltinTypes.Byte;
                case TypeCode.Byte:
                    return  BuiltinTypes.Char;
                case TypeCode.Int16:
                    return BuiltinTypes.Short;
                case TypeCode.UInt16:
                    return BuiltinTypes.UShort;
                case TypeCode.Int32:
                    return BuiltinTypes.Int;
                case TypeCode.UInt32:
                    return BuiltinTypes.UInt;
                case TypeCode.Int64:
                    return BuiltinTypes.Long;
                case TypeCode.UInt64:
                    return BuiltinTypes.ULong;
                case TypeCode.Single:
                    return BuiltinTypes.Float;
                case TypeCode.Double:
                    return BuiltinTypes.Double;
                case TypeCode.Decimal:
                    return  BuiltinTypes.Extended;

                default:
                    return BuiltinTypes.Unknown;
            }
        }


        /// <summary>
        /// Type checker Resolve builtin types
        /// </summary>
        /// <param name="toCheck">Types to check</param>
        /// <returns>BuiltinType</returns>
        public static BuiltinTypes GCT(params object[] toCheck)
        {
            BuiltinTypes bt = BuiltinTypes.Unknown;
            foreach (var obj in toCheck)
                bt = ResolveBuiltin(toCheck);

            return bt;

        }

    }


    /// <summary>
    /// Basic type specs
    /// </summary>
    public class TypeSpec : MemberSpec
    {
        BuiltinTypes _bt;
        TypeSpec _base;
        TypeFlags _flags;


     
        public bool IsBuiltinType
        {
            get
            {
                return _bt != BuiltinTypes.Unknown;
            }
        }
        public bool IsStruct
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Struct) == TypeFlags.Struct);
            }
 
        }
        public bool IsTypeDef
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.TypeDef) == TypeFlags.TypeDef);
            }

        }
        public bool IsEnum
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Enum) == TypeFlags.Enum);
            }

        }
        public bool IsPointer
        {
            get
            {
                return ((_flags & TypeFlags.Pointer) == TypeFlags.Pointer);
            }

        }
        public bool IsVoid
        {
            get
            {
                return _bt == BuiltinTypes.Void && ((_flags & TypeFlags.Void) == TypeFlags.Void);
            }

        }
        public bool IsUnknown
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Missing) == TypeFlags.Missing);
            }

        }
       
        /// <summary>
        /// For typedef
        /// </summary>
        public TypeSpec BaseType
        {
            get { return _base; }

        }
        /// <summary>
        /// Builtin Type
        /// </summary>
        public BuiltinTypes BuiltinType
        {
            get { return _bt; }
        }
        /// <summary>
        /// Type Flags
        /// </summary>
        public TypeFlags Flags
        {
            get { return _flags; }
        }
     
  


        //
        // Returns the size of type if known, otherwise, 0
        //
        public static int GetSize(TypeSpec type)
        {
            if (type.IsBuiltinType)
            {
                switch (type.BuiltinType)
                {
                    case BuiltinTypes.Bool:
                    case BuiltinTypes.Char:
                    case BuiltinTypes.Byte:
                        return 1;
                    case BuiltinTypes.Short:
                    case BuiltinTypes.UShort:
                        return 2;
                    case BuiltinTypes.Int:
                    case BuiltinTypes.UInt:
                    case BuiltinTypes.Float:
                        return 4;
                    case BuiltinTypes.Long:
                    case BuiltinTypes.ULong:
                    case BuiltinTypes.Double:
                        return 8;
                    case BuiltinTypes.Extended:
                        return 10;

                    default:
                        return 0;
                }
            }
            return 0;
        }

        public TypeSpec(string name, BuiltinTypes bt, TypeFlags flags,Modifiers mods,Location loc, TypeSpec basetype = null)
            : base(name,  new MemberSignature(name, loc),mods)
        {
            _bt = bt;
            _flags = flags;
            _base = basetype;


        }

        public TypeSpec MakePointer()
        {
            return new TypeSpec(Name, _bt, _flags | TypeFlags.Pointer,Modifiers, _sig.Location, this);
        }
        public TypeSpec MakeArray()
        {
            return new TypeSpec(Name, _bt, _flags | TypeFlags.Pointer | TypeFlags.Array,Modifiers, _sig.Location);
        }
        public string GetTypeName(TypeSpec tp)
        {
            if (tp.BaseType != null && tp.IsBuiltinType && tp.IsPointer)
                return GetTypeName(tp.BaseType) + "*";
            else return tp.Name;
        }

        public override string ToString()
        {
            return Name;
        }
       
    }

    /// <summary>
    /// Pointer Type
    /// </summary>
    public class PointerTypeSpec : TypeSpec
    {   
        public PointerTypeSpec(TypeSpec _basetype)
            : this(_basetype,0)
        {

        }
        public PointerTypeSpec(TypeSpec _basetype,TypeFlags _flags)
            : base(_basetype.Name, _basetype.BuiltinType, _basetype.Flags | TypeFlags.Pointer | _flags, _basetype.Modifiers, _basetype.Signature.Location, _basetype)
        {

        }
    }

    /// <summary>
    /// Array type
    /// </summary>
    public class ArrayTypeSpec : PointerTypeSpec
    {
        private readonly int _size;

        public int Size
        {
            get
            {
                return _size;
            }
        }
        public ArrayTypeSpec(TypeSpec _basetype, int size = 0)
            : base(_basetype, TypeFlags.Array)
        {
            _size = size;
        }
    }

    /// <summary>
    /// Base Type
    /// </summary>
    public class BuiltinTypeSpec : TypeSpec
    {
        public BuiltinTypeSpec(string name, BuiltinTypes bt)
            : base(name, bt, TypeFlags.Builtin,Modifiers.NoModifier, Location.Null)
        {

        }
        public BuiltinTypeSpec(string name, BuiltinTypes bt, TypeFlags tf)
            : base(name, bt, TypeFlags.Builtin | tf,Modifiers.NoModifier, Location.Null)
        {

        }
        public static BuiltinTypeSpec Char = new BuiltinTypeSpec("char", BuiltinTypes.Char);
        public static BuiltinTypeSpec Byte = new BuiltinTypeSpec("byte", BuiltinTypes.Byte);
        public static BuiltinTypeSpec Short = new BuiltinTypeSpec("short", BuiltinTypes.Short);
        public static BuiltinTypeSpec UShort = new BuiltinTypeSpec("ushort", BuiltinTypes.UShort);
        public static BuiltinTypeSpec Int = new BuiltinTypeSpec("int", BuiltinTypes.Int);
        public static BuiltinTypeSpec UInt = new BuiltinTypeSpec("uint", BuiltinTypes.UInt);

        public static BuiltinTypeSpec Long = new BuiltinTypeSpec("long", BuiltinTypes.Long);
        public static BuiltinTypeSpec ULong = new BuiltinTypeSpec("ulong", BuiltinTypes.ULong);
        public static BuiltinTypeSpec Float = new BuiltinTypeSpec("float", BuiltinTypes.Float);
        public static BuiltinTypeSpec Double = new BuiltinTypeSpec("double", BuiltinTypes.Double);
        public static BuiltinTypeSpec Extended = new BuiltinTypeSpec("extended", BuiltinTypes.Extended);
        public static BuiltinTypeSpec Bool = new BuiltinTypeSpec("bool", BuiltinTypes.Bool);
      
        public static BuiltinTypeSpec Null = new BuiltinTypeSpec("null", BuiltinTypes.Int, TypeFlags.Null);

        public override string ToString()
        {
            return "[BUILTIN] "+base.ToString();
        }
    }
    /// <summary>
    /// Global Variable Specs
    /// </summary>
    public class FieldSpec : MemberSpec
    {
        TypeSpec memberType;


        public TypeSpec MemberType
        {
            get
            {
                return memberType;
            }
        }

        public FieldSpec(string name, Modifiers mods, TypeSpec type, Location loc)
            : base(name, new MemberSignature(name, loc, type.Name), mods)
        {

        }
    }
    /// <summary>
    /// Method Specs Specs
    /// </summary>
    public class MethodSpec : MemberSpec
    {
        TypeSpec memberType;


        public TypeSpec MemberType
        {
            get
            {
                return memberType;
            }
        }
        public bool IsPrototype
        {
            get
            {
                return (Modifiers & Modifiers.Prototype) == VJay.Modifiers.Prototype;
            }
        }
        public MethodSpec(string name, Modifiers mods, TypeSpec type,TypeSpec[] parameters,  Location loc)
            : base(name, new MemberSignature(name, loc, type.Name, parameters), mods)
        {

        }
    }
}
