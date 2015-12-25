using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VCC
{
    /// <summary>
    /// Member Signature [Types, member, variable]
    /// </summary>
    public struct MemberSignature
    {
        string _signature;
        public string Signature { get { return _signature; } }
        Location _loc;
        public Location Location { get { return _loc; } }

        public MemberSignature(Namespace ns, string name, Location loc)
        {
            _signature = name;
            if (!ns.IsDefault)
                _signature = ns.Normalize() + "_" + _signature;
            _loc = loc;

        }
        public static bool operator !=(MemberSignature a, MemberSignature b)
        {
            return a.Signature != b.Signature;
        }
        public static bool operator ==(MemberSignature a, MemberSignature b)
        {
            return a.Signature == b.Signature;
        }

        public bool Equals(MemberSignature ns)
        {
            return ns.Signature == Signature;
        }
        public override bool Equals(object obj)
        {
            if (obj is MemberSignature)
                return Signature == ((MemberSignature)obj).Signature;
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
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
        Bool=0,
        Byte = 1, // 8 bits unsigned [unsigned char]
        SByte =2, // 8 bits signed [signed char]
        Int = 3, // 32 bits signed [long int]

        UInt=4, // 32 bits unsigned
        Void=5, // void

        String=6,
        Unknown=7
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
        Null = 1 << 8,
        Register = 1 << 9
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

    public abstract class MemberSpec :ReferenceSpec, IMember
    {
       protected MemberSignature _sig;
       protected Modifiers _mod;
       protected string _name;
       protected TypeSpec memberType;

       public bool IsConstant { get { return ((_mod & Modifiers.Const) == Modifiers.Const); } }

       public TypeSpec MemberType
       {
           get
           {
               return memberType;
           }
       }


       public MemberSpec(string name, MemberSignature sig, Modifiers mod, ReferenceKind re)
           : base(re)
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

    public interface IMember
    {
        TypeSpec MemberType { get; }
    }

    public class TypeChecker
    {
        public static bool ArtihmeticsAllowed(TypeSpec a, TypeSpec b)
        {
            return !(a.IsStruct || b.IsStruct);
        }
        public static bool Equals(TypeSpec a, TypeSpec b)
        {
            return true;
        }
        public static bool CompatibleTypes(TypeSpec a, TypeSpec b)
        {
            return true;
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
       protected int _size;

     
        public Namespace NS {get;set;}
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
            get {
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
                return IsBuiltinType && (BuiltinType == BuiltinTypes.Byte || BuiltinType == BuiltinTypes.SByte || BuiltinType == BuiltinTypes.Int || BuiltinType == BuiltinTypes.UInt);
            }

        }

        public bool IsValueType
        {
            get
            {
                return (IsBuiltinType || IsStruct) && !IsPointer;
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
        public bool IsArray
        {
            get
            {
                return ((_flags & TypeFlags.Array) == TypeFlags.Array);
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
        public bool IsUnknown
        {
            get
            {
                return _bt == BuiltinTypes.Unknown && ((_flags & TypeFlags.Missing) == TypeFlags.Missing);
            }

        }

        public bool IsUnsigned
        {
            get { return BuiltinType == BuiltinTypes.Byte || BuiltinType == BuiltinTypes.UInt; }
        }
        public bool NeedsNamespaceAccess
        {
            get { return !NS.IsDefault; }
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

        public TypeSpec(Namespace ns,string name, BuiltinTypes bt, TypeFlags flags,Modifiers mods,Location loc, TypeSpec basetype = null)
            : base(name, new MemberSignature(ns, name, loc), mods, ReferenceKind.Type)
        {
            NS = ns;
            _bt = bt;
            _flags = flags;
            _base = basetype;
            _size = GetSize(this);

        }
        public TypeSpec(Namespace ns, string name, int size, BuiltinTypes bt, TypeFlags flags, Modifiers mods, Location loc, TypeSpec basetype = null)
            : base(name, new MemberSignature(ns, name, loc), mods, ReferenceKind.Type)
        {
            NS = ns;
            _bt = bt;
            _flags = flags;
            _base = basetype;
            _size = size;

        }
      
        public TypeSpec MakePointer()
        {
            return new PointerTypeSpec(NS, this);
        }
        public TypeSpec MakeArray()
        {
            return new TypeSpec(NS, Name, _bt, _flags | TypeFlags.Pointer | TypeFlags.Array, Modifiers, _sig.Location);
        }
        public string GetTypeName(TypeSpec tp)
        {
            if (tp.BaseType != null && tp.BaseType == BuiltinTypeSpec.Byte && tp.IsPointer)
                return "string";
            else if (tp.BaseType != null && tp.IsBuiltinType && tp.IsPointer)
                return GetTypeName(tp.BaseType) + "*";
            else return tp.Name;
        }
        public TypeSpec GetTypeDefBase(TypeSpec ts)
        {
            if (ts.IsTypeDef)
                return GetTypeDefBase(ts.BaseType);
            else return ts;
        }
        public override string ToString()
        {
            return  Name;
        }
       
    }

    /// <summary>
    /// Pointer Type
    /// </summary>
    public class RegisterTypeSpec : TypeSpec
    {
        public RegisterTypeSpec(int size)
            : base(Namespace.Default, "ASM_NATIVE_REGISTER_TYPE", BuiltinTypes.Unknown, TypeFlags.Register, Modifiers.NoModifier,Location.Null, size == 2?BuiltinTypeSpec.UInt:BuiltinTypeSpec.Byte )
        {
            _name = BaseType.Name + " register ";
        }

        public static RegisterTypeSpec RegisterByte = new RegisterTypeSpec(1);
        public static RegisterTypeSpec RegisterWord = new RegisterTypeSpec(2);
      
        public override string ToString()
        {
            return Name                ;
        }
    }
    /// <summary>
    /// Pointer Type
    /// </summary>
    public class PointerTypeSpec : TypeSpec
    {
        public PointerTypeSpec(Namespace ns, TypeSpec _basetype)
            : this(ns,_basetype,0)
        {
            _size = 2;
        }
        public PointerTypeSpec(Namespace ns,TypeSpec _basetype,TypeFlags _flags)
            : base(ns,_basetype.Name, _basetype.Size,_basetype.BuiltinType, _basetype.Flags | TypeFlags.Pointer | _flags, _basetype.Modifiers, _basetype.Signature.Location, _basetype)
        {
            _size = 2;
        }
        public static TypeSpec MakePointer(TypeSpec tp, int count)
        {
            if (count == 0)
                return tp;
            else return new PointerTypeSpec(tp.NS, MakePointer(tp, count - 1));
        }
        public int PointerCount(TypeSpec pts)
        {
            if (pts == null)
                return 0;
            if (pts.IsPointer)
                return 1 + PointerCount(pts.BaseType);
            else return 0;
        }
        public override string ToString()
        {
            return GetTypeName(this);
        }
    }

    /// <summary>
    /// Array type
    /// </summary>
    public class ArrayTypeSpec : PointerTypeSpec
    {
        
        public ArrayTypeSpec(Namespace ns, TypeSpec _basetype, int size = 0)
            : base(ns,_basetype, TypeFlags.Array)
        {
            _size = size*_basetype.Size;
        }
        public override string ToString()
        {
            return GetTypeName(this);
        }
    }

    /// <summary>
    /// Base Type
    /// </summary>
    public class BuiltinTypeSpec : TypeSpec
    {
        public BuiltinTypeSpec( string name, BuiltinTypes bt, TypeSpec baset = null)
            : base(Namespace.Default, name, bt, TypeFlags.Builtin, Modifiers.NoModifier, Location.Null,baset)
        {

        }
        public BuiltinTypeSpec(string name, BuiltinTypes bt, TypeFlags tf, TypeSpec baset = null)
            : base(Namespace.Default,name, bt, TypeFlags.Builtin | tf, Modifiers.NoModifier, Location.Null, baset)
        {
          
        }
        public override string ToString()
        {
            return GetTypeName(this);
        }
        public static BuiltinTypeSpec Byte = new BuiltinTypeSpec("byte", BuiltinTypes.Byte);
        public static BuiltinTypeSpec SByte = new BuiltinTypeSpec("sbyte", BuiltinTypes.SByte);
        public static BuiltinTypeSpec Int = new BuiltinTypeSpec("int", BuiltinTypes.Int);
        public static BuiltinTypeSpec UInt = new BuiltinTypeSpec("uint", BuiltinTypes.UInt);

        public static BuiltinTypeSpec Bool = new BuiltinTypeSpec("bool", BuiltinTypes.Bool);
        public static BuiltinTypeSpec Void = new BuiltinTypeSpec("void", BuiltinTypes.Void);
        public static BuiltinTypeSpec String = new BuiltinTypeSpec("string", BuiltinTypes.String, TypeFlags.Pointer, Byte);
        public static BuiltinTypeSpec Null = new BuiltinTypeSpec("null", BuiltinTypes.Int, TypeFlags.Null);
    }
    /// <summary>
    /// Global Variable Specs
    /// </summary>
    public class FieldSpec : MemberSpec
    {
        public int FieldOffset { get; set; }
        public Namespace NS { get; set; }
        public FieldSpec(Namespace ns,string name, Modifiers mods, TypeSpec type, Location loc)
            : base(name, new MemberSignature(ns,name, loc), mods,ReferenceKind.Field)
        {
            FieldOffset = 0;
            NS = ns;
            memberType = type;
        }
    
        public override bool EmitFromStack(EmitContext ec)
        {
            if (memberType.Size <= 2)
            {
                ec.EmitComment("Pop Field @" + Signature.ToString() + " " + FieldOffset);
                ec.EmitInstruction(new Pop() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = FieldOffset, DestinationIsIndirect = true, Size = memberType.SizeInBits });
            }
            else // is composed type
            {
                ec.EmitComment("Pop Field [TypeOf " + MemberType.Name + "] Offset=" + FieldOffset);
                int s = MemberType.Size / 2;

              
                for (int i = 0; i < s ; i++)
                    ec.EmitInstruction(new Pop() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = FieldOffset + 2 * i, DestinationIsIndirect = true, Size = 16 });

                if (MemberType.Size % 2 != 0)
                    ec.EmitInstruction(new Pop() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = FieldOffset + MemberType.Size - 1, DestinationIsIndirect = true, Size = 8 });

            }
         
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (memberType.Size <= 2)
            {
                ec.EmitComment("Push Field @" + Signature.ToString() + " " + FieldOffset);
                ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = FieldOffset, DestinationIsIndirect = true, Size = memberType.SizeInBits });
            }
            else // is composed type
            {
                ec.EmitComment("Push Field [TypeOf " + MemberType.Name + "] Offset=" + FieldOffset);
                int s = MemberType.Size / 2;

                if (MemberType.Size % 2 != 0)
                    ec.EmitInstruction(new Push() {  DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = FieldOffset + MemberType.Size-1, DestinationIsIndirect = true, Size = 8 });

                for (int i = s - 1; i >= 0; i--)
                    ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(Signature.ToString()), DestinationDisplacement = FieldOffset + 2 * i, DestinationIsIndirect = true, Size = 16 });


            }
         
            return true;
        }


        public override bool LoadEffectiveAddress(EmitContext ec)
        {
         
            ec.EmitComment("AddressOf Field " );
            ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
            ec.EmitPush(EmitContext.SI);
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
            ec.EmitComment("ValueOf Field ");
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
            ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {
            ec.EmitComment("ValueOf Field ");
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceRef = ElementReference.New(Signature.ToString()) });
            ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            return true;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }
    }
    /// <summary>
    /// Method Specs
    /// </summary>
    public class MethodSpec : MemberSpec
    {
    
        public Namespace NS { get; set; }
        public List<ParameterSpec> Parameters { get; set; }

      
        public bool IsPrototype
        {
            get
            {
                return (Modifiers & Modifiers.Prototype) == Modifiers.Prototype;
            }
        }

        public MethodSpec(Namespace ns, string name, Modifiers mods, TypeSpec type, Location loc)
            : base(name, new MemberSignature(ns, name, loc), mods ,ReferenceKind.Method)
        {
            NS = ns;
            memberType = type;
            Parameters = new List<ParameterSpec>();
        }
        public override string ToString()
        {
            return Signature.ToString();
        }
    }

    /// <summary>
    /// Local Variable Specs
    /// </summary>
    public class VarSpec : MemberSpec
    {
       
        MethodSpec method;
        public int StackIdx { get; set; }
        public bool Initialized { get; set; }
 
        public MethodSpec MethodHost
        {
            get
            {
                return method;
            }
        }
        public Namespace NS { get; set; }
        public VarSpec(Namespace ns, string name, MethodSpec host, TypeSpec type, Location loc, Modifiers mods = VCC.Modifiers.NoModifier)
            : base(name, new MemberSignature(ns, host.Name + "_" + name, loc), mods, ReferenceKind.LocalVariable)
        {
            method = host;
            memberType = type;
            NS = ns;
            Initialized = false;
            StackIdx = 0;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }

        public override bool EmitToStack(EmitContext ec)
        {

            if (MemberType.Size <= 2)
            {
                ec.EmitComment("Push Var @BP" + StackIdx);
                ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx, DestinationIsIndirect = true, Size = MemberType.SizeInBits });
            }
            else // is composed type
            {
                ec.EmitComment("Push Var [TypeOf "+MemberType.Name+"] @BP" + StackIdx);
                int s = MemberType.Size / 2;
               
                if (MemberType.Size % 2 != 0)
                    ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx-1+MemberType.Size, DestinationIsIndirect = true, Size = 8 });

                for (int i = s-1; i >= 0; i--)
                    ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx + 2 * i, DestinationIsIndirect = true, Size = 16 });

               
            }
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (MemberType.Size <= 2)
            {
                ec.EmitComment("Pop Var @BP" + StackIdx);
                ec.EmitInstruction(new Pop() { Size = memberType.SizeInBits, DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx, DestinationIsIndirect = true });
            }
        
            else // composed type
            {
                ec.EmitComment("Pop Var [TypeOf " + MemberType.Name + "] @BP" + StackIdx);
                int s = MemberType.Size / 2;

             
                for (int i = 0; i < s; i++)
                    ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx + 2 * i, DestinationIsIndirect = true, Size = 16 });
                if (MemberType.Size % 2 != 0)
                    ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx - 1 + MemberType.Size, DestinationIsIndirect = true, Size = 8 });

               
            }
            

            return true;
        }

        public override bool LoadEffectiveAddress(EmitContext ec)
        {

            ec.EmitComment("AddressOf @BP" + StackIdx);
            ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = StackIdx });
            ec.EmitPush(EmitContext.SI);
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
            ec.EmitComment("ValueOf @BP" + StackIdx);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = StackIdx });
            ec.EmitPush(EmitContext.SI,  MemberType.BaseType.SizeInBits,true);

            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {
            ec.EmitComment("ValueOf @BP" + StackIdx);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = StackIdx });
            ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            return true;
        }
    }

    /// <summary>
    /// Local Variable Specs
    /// </summary>
    public class ParameterSpec : MemberSpec
    {
 
        MethodSpec method;
        public bool IsConstant { get; set; }
        public int StackIdx { get; set; }
       
      
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
        public ParameterSpec(string name, MethodSpec host, TypeSpec type,bool constant, Location loc, Modifiers mods = VCC.Modifiers.NoModifier)
            : base(name, new MemberSignature(Namespace.Default, host.Name + "_param_" + name, loc), mods,ReferenceKind.Parameter)
        {
            method = host;
            memberType = type;
            IsConstant = constant;
            StackIdx = 4;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }
      
 
        public override bool EmitFromStack(EmitContext ec)
        {

            if (memberType.Size <= 2)
            {
                ec.EmitComment("Pop Parameter @BP " + StackIdx);
                ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement = StackIdx, DestinationIsIndirect = true });
            }
            else
            {
                ec.EmitComment("Pop Parameter [TypeOf " + MemberType.Name + "] @BP" + StackIdx);
                int s = MemberType.Size / 2;

             
                for (int i = 0; i < s; i++)
                    ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx + 2 * i, DestinationIsIndirect = true, Size = 16 });
                if (MemberType.Size % 2 != 0)
                    ec.EmitInstruction(new Pop() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx - 1 + MemberType.Size, DestinationIsIndirect = true, Size = 8 });

               
            }
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (memberType.Size <= 2)
            {
                ec.EmitComment("Push Parameter @BP " + StackIdx);
                ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = memberType.SizeInBits, DestinationDisplacement = StackIdx, DestinationIsIndirect = true });
            }
            else // is composed type
            {
                ec.EmitComment("Push Parameter [TypeOf " + MemberType.Name + "] @BP" + StackIdx);
                int s = MemberType.Size / 2;

                if (MemberType.Size % 2 != 0)
                    ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx - 1 + MemberType.Size, DestinationIsIndirect = true, Size = 8 });

                for (int i = s - 1; i >= 0; i--)
                    ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, DestinationDisplacement = StackIdx + 2 * i, DestinationIsIndirect = true, Size = 16 });


            }
              
            return true;
        }

        public override bool LoadEffectiveAddress(EmitContext ec)
        {

            ec.EmitComment("AddressOf @BP+" + StackIdx);
            ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = StackIdx });
            ec.EmitPush(EmitContext.SI);
            return true;
        }
        public override bool ValueOf(EmitContext ec)
        {
            ec.EmitComment("ValueOf @BP+" + StackIdx);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = StackIdx });
            ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            return true;
        }
        public override bool ValueOfStack(EmitContext ec)
        {
            ec.EmitComment("ValueOf @BP+" + StackIdx);
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceIsIndirect = true, Size = 16, SourceReg = EmitContext.BP, SourceDisplacement = StackIdx });
            ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);

            return true;
        }
    }

    /// <summary>
    /// TypeMemberSpec Specs
    /// </summary>
    public class TypeMemberSpec : MemberSpec
    {

        TypeSpec th;
        public int Index { get; set; }
     
        public TypeSpec TypeHost
        {
            get
            {
                return th;
            }
        }
        public Namespace NS { get; set; }
        public TypeMemberSpec(Namespace ns, string name, TypeSpec host, TypeSpec type, Location loc, int idx)
            : base(name, new MemberSignature(ns,host.Name + "_" + name, loc), Modifiers.NoModifier,ReferenceKind.Member)
        {
            th = host;
            NS = ns;
            memberType = type;
            Index = 0;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }

        public override bool EmitToStack(EmitContext ec)
        {
            
            return base.EmitToStack(ec);
        }

    }

    /// <summary>
    ///  Struct Type Spec
    /// </summary>
    public class StructTypeSpec : TypeSpec
    {
        public List<TypeMemberSpec> Members { get; set; }

        public StructTypeSpec(Namespace ns,string name, List<TypeMemberSpec> mem, Location loc)
            : base(ns,name, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, loc)
        {
            Members = mem;
            Size = 0;
            foreach (TypeMemberSpec m in mem)
                Size += m.MemberType.Size;
            
        }

        public TypeMemberSpec ResolveMember(string name)
        {
            foreach (TypeMemberSpec kt in Members)
                if (kt.Name == name)
                    return kt;

            return null;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }

        public TypeMemberSpec GetMemberAt(int offset)
        {
            int off = 0;
            foreach (TypeMemberSpec tm in Members)
            {
                if (off == offset)
                    return tm;

                off += tm.MemberType.Size;
            }
            return null;
        }
    }

    /// <summary>
    /// Enum Member Spec
    /// </summary>
    public class EnumMemberSpec : MemberSpec
    {
      
        TypeSpec th;
        public bool IsAssigned { get; set; }
        public ushort Value { get; set; }
     
        public TypeSpec TypeHost
        {
            get
            {
                return th;
            }
            set { th = value; }
        }
        public Namespace NS { get; set; }
        public EnumMemberSpec(Namespace ns, string name, TypeSpec host, TypeSpec type, Location loc)
            : base(name, new MemberSignature(ns,host.Name + "_" + name, loc), Modifiers.NoModifier,ReferenceKind.EnumValue)
        {
            NS = ns;
            th = host;
            memberType = type;
            Value = 0;
            IsAssigned = false;
        }
        public EnumMemberSpec(Namespace ns, string name, ushort val, TypeSpec host, TypeSpec type, Location loc)
            : base(name, new MemberSignature(ns, host.Name + "_" + name, loc), Modifiers.NoModifier, ReferenceKind.EnumValue)
        {
            th = host;
            memberType = type;
            Value = val;
            IsAssigned = true;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }


        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = Value,Size = th.SizeInBits });
            return true;
        }
    }

    /// <summary>
    /// Enum Type Spec
    /// </summary>
    public class EnumTypeSpec : TypeSpec
    {
        public List<EnumMemberSpec> Members { get; set; }

        public EnumTypeSpec(Namespace ns, string name, int size, List<EnumMemberSpec> mem, Location loc)
            : base(ns,name, BuiltinTypes.Unknown, TypeFlags.Enum, Modifiers.NoModifier, loc)
        {
            Members = mem;
            Size = size;
            foreach (EnumMemberSpec m in mem)
                m.TypeHost = this;
          

        }

        public EnumMemberSpec ResolveMember(string name)
        {
            foreach (EnumMemberSpec kt in Members)
                if (kt.Name == name)
                    return kt;

            return null;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }
    }

    public enum ReferenceKind
    {
        Field,
        LocalVariable,
        Parameter,
        EnumValue,
        Type,
        Member,
        Method
    }
    public abstract class ReferenceSpec
    {
        public ReferenceKind ReferenceType { get; set; }
        public ReferenceSpec(ReferenceKind rs)
        {
            ReferenceType = rs;
        }


      
        public virtual bool LoadEffectiveAddress(EmitContext ec)
        {
            return true;
        }
        public virtual bool ValueOf(EmitContext ec)
        {
            return true;
        }
        public virtual bool ValueOfStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec) { return true; }
        public virtual bool EmitFromStack(EmitContext ec) { return true; }
    }
}
