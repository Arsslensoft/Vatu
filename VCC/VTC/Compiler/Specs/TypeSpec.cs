using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Basic type specs
    /// </summary>
    public class TypeSpec : MemberSpec, IEquatable<TypeSpec>
    {
     
        public static int TypeKind = 0;
        public static Dictionary<string, ushort> KindsCache = new Dictionary<string, ushort>();
        BuiltinTypes _bt;
        TypeSpec _base;
        TypeFlags _flags;
        protected int _size;

        public override string Name
        {
            get
            {
                return GetTypeName(this);
            }
            internal set
            {
                _name = value;
            }
        }
        public ushort TypeDescriptor { get; private set; }
        public byte FloatSizeBits
        {
            get
            {
                if (IsFloat)
                    return 32;
                else return 80;
            }

        }
        public byte SizeInBits
        {
            get
            {
                if (Size <= 2)
                    return (byte)(Size * 8);
       
                else return 80;
            }

        }
        public int Size
        {
            get
            {
                if (IsReference)
                    return 4;
                if (IsTypeDef)
                    return GetTypeDefBase(this).Size;
        
                return _size;

            }
            set { _size = value; }
        }

        public bool IsNumeric
        {
            get
            {
                return IsBuiltinType && (BuiltinType == BuiltinTypes.Byte || BuiltinType == BuiltinTypes.SByte || BuiltinType == BuiltinTypes.Int || BuiltinType == BuiltinTypes.UInt || BuiltinType == BuiltinTypes.Pointer);
            }

        }
        public bool IsTemplate
        {
            get
            {
                return (_flags & TypeFlags.Template) == TypeFlags.Template;
            }
        }
        public bool IsValueType
        {
            get
            {
                return (IsBuiltinType);
            }
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
        public bool IsForeignType
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && (IsStruct || IsUnion);
            }

        }
        public bool IsReferenceType
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && (IsClass);
            }

        }
        bool _isref = false;
        public bool IsReference
        {
            get
            {
                return _isref;
            }
            set { _isref = value; }

        }
        public bool IsClass
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Class) == TypeFlags.Class);
            }

        }
        public bool IsUnion
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Union) == TypeFlags.Union);
            }

        }
        public bool IsDelegate
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Delegate) == TypeFlags.Delegate);
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
        bool _template = false;
        public bool IsTemplateBased
        {
            get { return _template; }
            set { _template = value; }
        }
        public bool IsArray
        {
            get
            {
                return ((_flags & TypeFlags.Array) == TypeFlags.Array);
            }

        }
        public bool IsMultiDimensionArray
        {
            get
            {
                return ((_flags & TypeFlags.Array) == TypeFlags.Array && this is ArrayTypeSpec && _base is ArrayTypeSpec);
            }

        }
        public bool IsFloat
        {
            get
            {
                return _bt  == BuiltinTypes.Float;
            }

        }
      
        public bool IsPointer
        {
            get
            {
                return ((_flags & TypeFlags.Pointer) == TypeFlags.Pointer);
            }

        }
        public bool IsRegister
        {
            get
            {
                return ((_flags & TypeFlags.Register) == TypeFlags.Register);
            }

        }
        public bool IsVoid
        {
            get
            {
                return _bt == BuiltinTypes.Void && ((_flags & TypeFlags.Void) == TypeFlags.Void);
            }

        }
        public bool IsType
        {
            get
            {
                return _bt == BuiltinTypes.Type;
            }
        }
        public bool IsUnknown
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Missing) == TypeFlags.Missing);
            }

        }
        public bool IsSigned
        {
            get { return BuiltinType == BuiltinTypes.SByte || BuiltinType == BuiltinTypes.Int; }
        }
        public bool IsUnsigned
        {
            get { return BuiltinType == BuiltinTypes.Byte || BuiltinType == BuiltinTypes.UInt; }
        }
        public bool NeedsNamespaceAccess
        {
            get { return !NS.IsDefault; }
        }

        public List<MethodSpec> ExtendedMethods { get; set; }
        public List<FieldSpec> ExtendedFields { get; set; }
        public List<MethodSpec> StaticExtendedMethods { get; set; }
    

        /// <summary>
        /// For typedef
        /// </summary>
        public TypeSpec BaseType
        {
            get { return _base; }
            set { _base = value; }
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
            set { _flags = value; }
        }




        //
        // Returns the size of type if known, otherwise, 0
        //
        internal int GetSize(TypeSpec type)
        {
            if (type.IsArray)
                return (type as ArrayTypeSpec).ArrayCount * GetSize(type.BaseType);
       
            else if (type.IsBuiltinType)
            {
                switch (type.BuiltinType)
                {
                    case BuiltinTypes.Bool:
                    case BuiltinTypes.SByte:
                    case BuiltinTypes.Byte:
                        return 1;
                    case BuiltinTypes.Float:
                        return 4;
                    case BuiltinTypes.Int:
                    case BuiltinTypes.UInt:
                    case BuiltinTypes.String:
                    case BuiltinTypes.Pointer:
                    case BuiltinTypes.Type:
                        return 2;
                    case BuiltinTypes.DPointer:
                        return 4;

                    default:
                        return 0;
                }
            }
         
            return type.Size;
        }
        internal int GetAllocSize(TypeSpec type)
        {
            if (type.IsArray)
                return (type as ArrayTypeSpec).ArrayCount * GetSize(type.BaseType);
            else if (type.IsBuiltinType)
            {
                switch (type.BuiltinType)
                {
                    case BuiltinTypes.Bool:
                    case BuiltinTypes.SByte:
                    case BuiltinTypes.Byte:
                        return 1;
                    case BuiltinTypes.Float:
                        return 4;
                    case BuiltinTypes.Int:
                    case BuiltinTypes.UInt:
                    case BuiltinTypes.String:
                    case BuiltinTypes.Pointer:
                    case BuiltinTypes.Type:
                        return 2;
                    case BuiltinTypes.DPointer:
                        return 4;

                    default:
                        return 0;
                }
            }
            else if (type is ClassTypeSpec)
                return (type as ClassTypeSpec).ClassSize;
          
            return type.Size;
        }
        protected int GetSizeBt(TypeSpec type)
        {
          
            if (type.IsBuiltinType)
            {
                switch (type.BuiltinType)
                {
                    case BuiltinTypes.Bool:
                    case BuiltinTypes.SByte:
                    case BuiltinTypes.Byte:
                        return 1;
                    case BuiltinTypes.Float:
                        return 4;
                    case BuiltinTypes.Int:
                    case BuiltinTypes.UInt:
                    case BuiltinTypes.String:
                    case BuiltinTypes.Pointer:
                    case BuiltinTypes.Type:
                        return 2;
                    case BuiltinTypes.DPointer:
                        return 4;

                    default:
                        return 0;
                }
            }
        
            return 0;
        }
        public static string NormalizeTypeName(string name)
        {
            return name.Replace("byte*","string");
        }
        public TypeSpec(Namespace ns, string name, BuiltinTypes bt, TypeFlags flags, Modifiers mods, Location loc,List<TypeSpec> param, TypeSpec basetype = null)
            : base(NormalizeTypeName(name), new MemberSignature(ns, NormalizeTypeName(name), param.ToArray(), loc), mods, ReferenceKind.Type)
        {
            NS = ns;
            _bt = bt;
            _flags = flags;
            _base = basetype;
            _size = GetSizeBt(this);
            if (KindsCache.ContainsKey(Signature.Signature))
                TypeDescriptor = KindsCache[Signature.Signature];
            else
            {

                TypeDescriptor = (ushort)TypeKind++;
                KindsCache.Add(Signature.Signature, TypeDescriptor);
            }
            StaticExtendedMethods = new List<MethodSpec>();
            ExtendedFields = new List<FieldSpec>();
            ExtendedMethods = new List<MethodSpec>();
        }


        public TypeSpec(Namespace ns, string name, BuiltinTypes bt, TypeFlags flags, Modifiers mods, Location loc, TypeSpec basetype = null)
            : base(NormalizeTypeName(name), new MemberSignature(ns, NormalizeTypeName(name), loc), mods, ReferenceKind.Type)
        {
            NS = ns;
            _bt = bt;
            _flags = flags;
            _base = basetype;
            _size = GetSizeBt(this);
            if (KindsCache.ContainsKey(Signature.Signature))
                TypeDescriptor = KindsCache[Signature.Signature];
            else
            {

                TypeDescriptor = (ushort)TypeKind++;
                KindsCache.Add(Signature.Signature, TypeDescriptor);
            }
            StaticExtendedMethods = new List<MethodSpec>();
            ExtendedFields = new List<FieldSpec>();
            ExtendedMethods = new List<MethodSpec>();
        }
        public TypeSpec(Namespace ns, string name, int size, BuiltinTypes bt, TypeFlags flags, Modifiers mods, Location loc, TypeSpec basetype = null)
            : base(NormalizeTypeName(name), new MemberSignature(ns, NormalizeTypeName(name), loc), mods, ReferenceKind.Type)
        {
            NS = ns;
            _bt = bt;
            _flags = flags;
            _base = basetype;
            _size = size;
            if (KindsCache.ContainsKey(Signature.Signature))
                TypeDescriptor = KindsCache[Signature.Signature];
            else
            {

                TypeDescriptor = (ushort)TypeKind++;
                KindsCache.Add(Signature.Signature, TypeDescriptor);
            }
            StaticExtendedMethods = new List<MethodSpec>();
            ExtendedFields = new List<FieldSpec>();
            ExtendedMethods = new List<MethodSpec>();
        }

        public TypeSpec MakePointer()
        {
            return new PointerTypeSpec(NS, this);
        }
        public virtual TypeSpec MakeReference()
        {
            TypeSpec tp = new TypeSpec(NS, Name, BuiltinType, Flags, Modifiers, Signature.Location, _base);
            tp.IsReference = true;
            return tp;
        }
        public TypeSpec MakeArray(int size)
        {
            return new ArrayTypeSpec(NS, this, size);
        }
        public string GetTypeName(TypeSpec tp)
        {
            if (tp.BaseType != null && tp.BaseType == BuiltinTypeSpec.Byte && tp.IsPointer)
                return "string";
            else if (tp.BaseType != null  && tp is PointerTypeSpec)
                return GetTypeName(tp.BaseType) + "*";
            //else if (tp.BaseType != null && tp.IsReference)
            //    return GetTypeName(tp.BaseType) + "&";
            else return tp._name;
        }

        public TypeSpec GetTypeDefBase(TypeSpec ts)
        {
            if (ts.IsTypeDef)
               return GetTypeDefBase(ts.BaseType);
            else return ts;
        }
        public void MakeBase(ref TypeSpec ts,  TypeSpec tp)
        {
            if (ts.BaseType != null && ts.BaseType.BaseType == null)
                ts.BaseType = tp;
            else  if (ts.BaseType != null)
                MakeBase(ref ts._base, tp);
            else tp = ts;
        }
        public TypeSpec CloneBase(TypeSpec t, TypeSpec dst, TypeSpec src)
        {
            if (t.BaseType == null && src.Equals(t))
                return dst;
            else if (t.BaseType == null && !src.Equals(t))
                return t;
            else if (t is PointerTypeSpec)
                return CloneBase(t.BaseType, dst, src).MakePointer();
            else if (t is ArrayTypeSpec)
                return CloneBase(t.BaseType, dst, src).MakeArray((t as ArrayTypeSpec).ArrayCount);
            else return CloneBase(t.BaseType, dst, src);

        }
        public void GetBase(TypeSpec ts, ref TypeSpec tp)
        {
            if (ts.BaseType != null)
                GetBase(ts.BaseType, ref tp);
            else tp = ts;
        }
        public override string ToString()
        {
            return GetTypeName(this);
        }
        public string NormalizedName
        {
            get { return Name.Replace("*", "P"); }
        }
      
        public virtual bool Equals(TypeSpec tp)
        {
            if (tp.IsArray && this.Name == "string")
                return true;
            else
            return tp.Signature == Signature;
        }

    }

	
	
}