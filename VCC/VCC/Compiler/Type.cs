using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VCC
{
    /// <summary>
    /// Member Signature [Types, member, variable]
    /// </summary>
    public class MemberSignature
    {
        public string Signature { get; set; }
        public Location Location { get; set; }

        public MemberSignature(string name,Location loc, params TypeSpec[] parameters)
        {
            Signature = "_" +name;
            if (parameters != null)
            {
                foreach (TypeSpec param in parameters)
                    Signature += "_" + param.Name;
            }
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
        Byte, // 8 bits unsigned [unsigned char]
        SByte, // 8 bits signed [signed char]
        Int, // 32 bits signed [long int]
        Void, // void
        UInt, // 32 bits unsigned
        Bool,
        String,
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
      
        Const = 1 << 4
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


       public ElementReference Reference { get; set; }
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
            
                case TypeCode.String:
                    return BuiltinTypes.String;
                case TypeCode.Boolean:
                    return BuiltinTypes.Bool;
                case TypeCode.SByte:
                    return BuiltinTypes.SByte;
                case TypeCode.Byte:
                    return  BuiltinTypes.Byte;
                case TypeCode.Int16:
                    return BuiltinTypes.Int;
                case TypeCode.UInt16:
                    return BuiltinTypes.UInt;
            

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
        int _size;


        public int Size
        {
            get { return _size; }
        }
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
        private int GetSize(TypeSpec type)
        {
            if (type.IsBuiltinType)
            {
                switch (type.BuiltinType)
                {
                    case BuiltinTypes.Bool:
                    case BuiltinTypes.SByte:
                    case BuiltinTypes.Byte:
                        return 1;
                    case BuiltinTypes.Int:
                    case BuiltinTypes.UInt:
                        return 2;


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
            _size = GetSize(this);

        }
        public TypeSpec(string name,int size, BuiltinTypes bt, TypeFlags flags, Modifiers mods, Location loc, TypeSpec basetype = null)
            : base(name, new MemberSignature(name, loc), mods)
        {
            _bt = bt;
            _flags = flags;
            _base = basetype;
            _size = size;

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
            return base.ToString();
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
            : base(_basetype.Name, _basetype.Size,_basetype.BuiltinType, _basetype.Flags | TypeFlags.Pointer | _flags, _basetype.Modifiers, _basetype.Signature.Location, _basetype)
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
        public BuiltinTypeSpec(string name, BuiltinTypes bt, TypeSpec baset = null)
            : base(name, bt, TypeFlags.Builtin, Modifiers.NoModifier, Location.Null,baset)
        {

        }
        public BuiltinTypeSpec(string name, BuiltinTypes bt, TypeFlags tf, TypeSpec baset = null)
            : base(name, bt, TypeFlags.Builtin | tf,Modifiers.NoModifier, Location.Null, baset)
        {
          
        }
  
        public static BuiltinTypeSpec Byte = new BuiltinTypeSpec("byte", BuiltinTypes.Byte);
        public static BuiltinTypeSpec SByte = new BuiltinTypeSpec("sbyte", BuiltinTypes.SByte);
        public static BuiltinTypeSpec Int = new BuiltinTypeSpec("int", BuiltinTypes.Int);
        public static BuiltinTypeSpec UInt = new BuiltinTypeSpec("uint", BuiltinTypes.UInt);

        public static BuiltinTypeSpec Bool = new BuiltinTypeSpec("bool", BuiltinTypes.Bool);
        public static BuiltinTypeSpec Void = new BuiltinTypeSpec("void", BuiltinTypes.Void);
        public static BuiltinTypeSpec String = new BuiltinTypeSpec("string", BuiltinTypes.String, TypeFlags.Pointer, Byte.MakePointer());
        public static BuiltinTypeSpec Null = new BuiltinTypeSpec("null", BuiltinTypes.Int, TypeFlags.Null);
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
            : base(name, new MemberSignature(name, loc), mods)
        {
            memberType = type;
        }
    }
    /// <summary>
    /// Method Specs
    /// </summary>
    public class MethodSpec : MemberSpec
    {
        TypeSpec memberType;

        public List<ParameterSpec> Parameters { get; set; }

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
                return (Modifiers & Modifiers.Prototype) == Modifiers.Prototype;
            }
        }
      
        public MethodSpec(string name, Modifiers mods, TypeSpec type,  Location loc)
            : base(name, new MemberSignature(name, loc), mods)
        {
            memberType = type;
            Parameters = new List<ParameterSpec>();
        }
    }

    /// <summary>
    /// Local Variable Specs
    /// </summary>
    public class VarSpec : MemberSpec
    {
        TypeSpec memberType;
        MethodSpec method;
        public int StackIndex { get; set; }
        public bool Initialized { get; set; }
        public TypeSpec MemberType
        {
            get
            {
                return memberType;
            }
        }
        public MethodSpec MethodHost
        {
            get
            {
                return method;
            }
        }
  
        public VarSpec(string name, MethodSpec host, TypeSpec type, Location loc)
            : base(name, new MemberSignature(host.Name + "_"+name, loc),  Modifiers.NoModifier)
        {
            method = host;
            memberType = type;
      
            Initialized = false;
            StackIndex = 0;
        }
     
    }

    /// <summary>
    /// Local Variable Specs
    /// </summary>
    public class ParameterSpec : MemberSpec
    {
        TypeSpec memberType;
        MethodSpec method;
        public bool IsConstant { get; set; }
        public int StackIndex { get; set; }

        public TypeSpec MemberType
        {
            get
            {
                return memberType;
            }
        }
        public MethodSpec MethodHost
        {
            get
            {
                return method;
            }
        }
        public bool IsParameter
        {
            get
            {
                return true;
            }
        }
        public ParameterSpec(string name, MethodSpec host, TypeSpec type,bool constant, Location loc)
            : base(name, new MemberSignature(host.Name + "_param_" + name, loc), Modifiers.NoModifier)
        {
            method = host;
            memberType = type;
            IsConstant = constant;
            StackIndex = 2;
        }
     
    }
}
