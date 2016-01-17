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
        NameIdentifier ni;
        [Rule(@"<Type>     ::= <Base> <Pointers>")]
        public TypeIdentifier(BaseTypeIdentifier tbase, TypePointer pointers)
        {
            _base = tbase;
            _pointers = pointers;
        }
        [Rule(@"<Type>     ::= <Name> ~'::' <Base> <Pointers>")]
        public TypeIdentifier(NameIdentifier ns,BaseTypeIdentifier tbase, TypePointer pointers)
        {
            ni = ns;
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
            if (ni != null)
            {
                Namespace ns = new Namespace(ni.Name);
                rc.CurrentScope |= ResolveScopes.AccessOperation;
                Namespace lastns = rc.CurrentNamespace;
                rc.CurrentNamespace = ns;
                _base = (BaseTypeIdentifier)_base.DoResolve(rc);
                rc.CurrentNamespace = lastns;
                rc.CurrentScope &= ~ResolveScopes.AccessOperation;
            }
            else _base = (BaseTypeIdentifier)_base.DoResolve(rc);
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
        [Rule(@"<Scalar>   ::= pointer")]
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
        Expr exp;

         [Rule(@"<Base>     ::= ~'@'Id")]
        [Rule(@"<Base>     ::= ~typeof Id")]  
        public BaseTypeIdentifier(Identifier type)
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
            {
                rc.Resolver.TryResolveType(_ident.Name, ref _ts);
                  if(_ts == null)
                      ResolveContext.Report.Error(0, Location, "Unresolved type");
            }
          

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_typeid != null)
              _typeid.Resolve(rc);

            if (_ident != null)
               rc.Resolver.TryResolveType(_ident.Name,ref _ts);
           
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
    public class NameIdentifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }
        [Rule(@"<Name> ::= Id")]
        public NameIdentifier(Identifier idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName.Name;
        }
        [Rule(@"<Name> ::= ~global")]
        public NameIdentifier()
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = "global";
        }
        [Rule(@"<Name> ::= <QualifiedName>")]
        public NameIdentifier(QualifiedNameIdentifier idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName.Name;
        }

    }
    public class QualifiedNameIdentifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }
        [Rule(@"<QualifiedName> ::= <Name> ~'::' Id")]
        public QualifiedNameIdentifier(NameIdentifier nid,Identifier idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = nid.Name + "::"+idName.Name ;
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
        [Rule(@"<Modifier>      ::= extern")]
        [Rule(@"<Modifier>      ::= static")]
        [Rule(@"<Modifier>      ::= const")]
        [Rule(@"<Modifier>      ::= private")]
        [Rule(@"<Modifier>      ::= public")]
        public Modifier(SimpleToken mod)
        {
            _mod = mod;
           
        }

        Modifier nmod;
         [Rule(@"<Mod>      ::= <Modifier>")]
        public Modifier(Modifier mod)
        {
            nmod = mod;

        }

         Modifier nxt;
         [Rule(@"<Mod>      ::= <Modifier> <Mod>")]
         public Modifier(Modifier mod,Modifier next)
         {
             nxt = next;
             _mod = mod;

         }


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            ModifierList = 0;
            if (nxt != null)
            {
                nxt = (Modifier)nxt.DoResolve(rc);
                ModifierList |= nxt.ModifierList;
            }
          
            if (_mod == null)
            {
                if (nmod != null)
                {
                    nmod = (Modifier)nmod.DoResolve(rc);
                    ModifierList |= nmod.ModifierList;

                }
                else                ModifierList |= Modifiers.Private;
            }
            else if (_mod.Name == "extern")
                ModifierList |= Modifiers.Extern;
            else if (_mod.Name == "static")
                ModifierList |= Modifiers.Static;
            else if (_mod.Name == "const")
                ModifierList |= Modifiers.Const;
            else if (_mod.Name == "private")
                ModifierList |= Modifiers.Private;
            else if (_mod.Name == "public")
                ModifierList |= Modifiers.Public;

            if ((ModifierList & Modifiers.Private) == Modifiers.Private && (ModifierList & Modifiers.Extern) == Modifiers.Extern)
                ResolveContext.Report.Error(0, Location, "A member cannot be private and extern at the same time");
            else if ((ModifierList & Modifiers.Private) == Modifiers.Private && (ModifierList & Modifiers.Public) == Modifiers.Public)
                ResolveContext.Report.Error(0, Location, "A member cannot be private and public at the same time");

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
    [Terminal("pointer")]
    public class TypeToken : SimpleToken, IResolve
    {
       protected TypeSpec _ts;
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
             rc.Resolver.TryResolveType(this.symbol.Name,ref _ts);
             if (_ts == null)
                 ResolveContext.Report.Error(0, loc, "Unresolved type");
            return this;
        }
    }
    // end modifiers
}
