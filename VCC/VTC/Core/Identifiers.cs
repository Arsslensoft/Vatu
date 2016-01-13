using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
    // START TYPE
  
    public class TypeIdentifier : TypeToken
    {
        BaseTypeIdentifier _base;
        TypePointer _pointers;
        [Rule(@"<Type>     ::= <Base> <Pointers>")]
        public TypeIdentifier(BaseTypeIdentifier tbase, TypePointer pointers)
        {
            _base = tbase;
            _pointers = pointers;
        }
        public override bool Resolve(ResolveContext rc)
        {
           
            _base.Resolve(rc);
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_pointers != null)
                _pointers = (TypePointer)_pointers.DoResolve(rc);

            _base = (BaseTypeIdentifier)_base.DoResolve(rc);
            Type = _base.Type;
            if (_pointers != null)
            {
          
                for (int i = 0; i < _pointers.PointerCount; i++)
                    Type = Type.MakePointer();
            }
            return this;
        }

    }
    public class ScalarTypeIdentifier : TypeToken
    {
       TypeToken _type;

        public override bool Resolve(ResolveContext rc)
        {
            _type.Resolve(rc);
            return true;
        }

        [Rule(@"<Scalar>   ::= byte")]
        [Rule(@"<Scalar>   ::= int")]
        [Rule(@"<Scalar>   ::= uint")]
        [Rule(@"<Scalar>   ::= sbyte")]
        [Rule(@"<Scalar>   ::= bool")]
        [Rule(@"<Scalar>   ::= void")]
        [Rule(@"<Scalar>   ::= string")]
        public ScalarTypeIdentifier(TypeToken type)
        {
          
          _type = type;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
           _type = (TypeToken)_type.DoResolve(rc);
           Type = _type.Type;
            return  this;
        }
       
        
    }
 
    public class BaseTypeIdentifier : TypeToken
    {
        TypeToken _typeid;
        Identifier _ident;
        StructDefinition _sdef;
        Expr exp;

        [Rule(@"<Base>     ::= struct Id")]
        [Rule(@"<Base>     ::= enum Id")]
        [Rule(@"<Base>     ::= union Id")]
        [Rule(@"<Base>     ::= delegate Id")]  
        public BaseTypeIdentifier(SimpleToken tok,Identifier type)
        {
            _ident = type;
        }


        [Rule(@"<Base> ::= ~typeof ~'(' <Expression> ~')'")]
        public BaseTypeIdentifier(Expr op)
        {
            exp = op;
        }
        [Rule(@"<Base>     ::= <Scalar>")]
        public BaseTypeIdentifier(ScalarTypeIdentifier type)
        {
            _typeid = type;
        }

        [Rule(@"<Base>     ::= ~'@'Id")]
        public BaseTypeIdentifier(Identifier type)
        {
            _ident = type;
        }
     
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (exp != null)
            {
                exp = (Expr)exp.DoResolve(rc);
                if (exp != null)
                    Type = exp.Type;
             
                else ResolveContext.Report.Error(0, Location, "Unresolved type");
            }
            if (_typeid != null)
            {
                _typeid = (TypeToken)_typeid.DoResolve(rc);
                Type = _typeid.Type;
            }
            if (_ident != null)
                Type = rc.Resolver.TryResolveType(_ident.Name);
            if (_sdef != null)
            {
                _sdef = (StructDefinition)_sdef.DoResolve(rc);
                if (_sdef._var != null)
                    Type = _sdef._var.Type;
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_typeid != null)
              _typeid.Resolve(rc);

            if (_ident != null)
                Type = rc.Resolver.TryResolveType(_ident.Name);
            if (_sdef != null)
                _sdef.Resolve(rc);
            return base.Resolve(rc);
        }
    }

    [Terminal("Id")]
    public class Identifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }

        public Identifier(string idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName;
        }


    }
    public class MethodIdentifier : Identifier
    {
        public Identifier Id { get; set; }
        public TypeToken TType { get; set; }
        public Modifiers Mods { get; set; }
        private CallingCV CCV { get; set; }
        public CallingConventions CV
        {
            get
            {
                if (CCV != null)
                    return CCV.CallingConvention;
                else return CallingConventions.Default;
            }
        }
        Modifier _mod;
        [Rule(@"<Func ID> ::= <CallCV> <Type> Id")]
        public MethodIdentifier(CallingCV ccv, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = ccv;
        }
        [Rule(@"<Func ID> ::= <Type> Id")]
        public MethodIdentifier( TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = null;
        }

        [Rule(@"<Func ID> ::= <Mod> <CallCV> <Type> Id")]
        public MethodIdentifier(Modifier mod,CallingCV ccv, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = ccv;
            _mod = mod;
        }
        [Rule(@"<Func ID> ::= <Mod> <Type> Id")]
        public MethodIdentifier(Modifier mod, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = null;
            _mod = mod;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_mod != null)
            {
                _mod = (Modifier)_mod.DoResolve(rc);
                Mods = _mod.ModifierList;
            }
            else Mods = Modifiers.Private;
            TType = (TypeToken)TType.DoResolve(rc);
            base.Type = TType.Type;
            if (CCV != null)
                CCV = (CallingCV)CCV.DoResolve(rc);
         
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            TType.Resolve(rc);

            return base.Resolve(rc);
        }
    }



    public class TypePointer : SimpleToken
    {


        public int PointerCount { get; set; }

        TypePointer _next;
        [Rule(@"<Pointers> ::= ~'*' <Pointers>")]
        public TypePointer(TypePointer ptr)
        {
            loc = CompilerContext.TranslateLocation(position);
            _next = ptr;
        }
        [Rule(@"<Pointers> ::=  ")]
        public TypePointer()
        {
            loc = CompilerContext.TranslateLocation(position);
            _next = null;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_next == null)
            {
                PointerCount = 0;
                return this;
            }
            PointerCount = 1 + ((TypePointer)(_next.DoResolve(rc))).PointerCount;
            return this;
        }


    }
   
    public class CallingCV : SimpleToken
    {
        public CallingConventions CallingConvention { get; set; }

   
        protected SimpleToken _mod;
        [Rule(@"<CallCV>      ::= stdcall")]
        [Rule(@"<CallCV>      ::= fastcall")]
        [Rule(@"<CallCV>      ::= cdecl")]
        [Rule(@"<CallCV>      ::= default")]
        [Rule(@"<CallCV>      ::= pascal")]
        [Rule(@"<CallCV>      ::= vfastcall")]
        public CallingCV(SimpleToken mod)
        {
            _mod = mod;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
           if (_mod.Name == "fastcall")
               CallingConvention = CallingConventions.FastCall;
            else if (_mod.Name == "cdecl")
               CallingConvention = CallingConventions.Cdecl;
           else if (_mod.Name == "default")
               CallingConvention = CallingConventions.Default;
           else if (_mod.Name == "pascal")
               CallingConvention = CallingConventions.Pascal;
           else if (_mod.Name == "vfastcall")
               CallingConvention = CallingConventions.VeryFastCall;
           else CallingConvention = CallingConventions.StdCall;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }
    // END TYPE
    // START MODIFIERs
    public class Modifier : SimpleToken
    {
        public Modifiers ModifierList { get; set; }

        /*<Mod>      ::= extern 
             | static
             | register
             | auto
             | volatile
             | const   */
       protected SimpleToken _mod;
        [Rule(@"<Mod>      ::= extern")]
        [Rule(@"<Mod>      ::= static")]
        [Rule(@"<Mod>      ::= const")]
        [Rule(@"<Mod>      ::= private")]
        [Rule(@"<Mod>      ::= public")]
        public Modifier(SimpleToken mod)
        {
            _mod = mod;
           
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_mod.Name == "extern")
                ModifierList = Modifiers.Extern;
            else if (_mod.Name == "static")
                ModifierList = Modifiers.Static;
            else if (_mod.Name == "const")
                ModifierList = Modifiers.Const;
            else if (_mod.Name == "private")
                ModifierList = Modifiers.Private;
            else if (_mod.Name == "public")
                ModifierList = Modifiers.Public;
            else ModifierList = Modifiers.NoModifier;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }

    public class FunctionSpecifier : SimpleToken
    {
        public Specifiers Specs { get; set; }

        SimpleToken tt;
        [Rule(@"<Func Spec>  ::= isolated")]
        [Rule(@"<Func Spec>  ::= entry")]
        public FunctionSpecifier(SimpleToken t)
        {
            tt = t;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (tt.Name == "entry")
                Specs = Specifiers.Entry;

            else if (tt.Name == "isolated")
                Specs = Specifiers.Isolated;

            return this;
        }
    }





    [Terminal("void")]
    [Terminal("byte")]
    [Terminal("sbyte")]
    [Terminal("int")]
    [Terminal("uint")]
    [Terminal("string")]
    [Terminal("bool")]
    public class TypeToken : SimpleToken, IResolve
    {
        TypeSpec _ts;
        public TypeSpec Type
        {
            get
            {
                if (_ts != null && _ts.IsTypeDef)
                    return _ts.GetTypeDefBase(_ts);
                else return _ts;
            }
            set
            {
                _ts = value;
            }
        }
        public TypeToken()
        {
            loc = CompilerContext.TranslateLocation(position);

        }

        public override bool Resolve(ResolveContext rc)
        {

            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Type = rc.Resolver.TryResolveType(this.symbol.Name);
            return this;
        }
    }
    // end modifiers
}
