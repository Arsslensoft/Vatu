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
                _pointers = _pointers.DoResolve(rc);

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

        public bool Resolve(ResolveContext rc)
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
        bool isstruct = false;

        [Rule(@"<Base>     ::= struct Id")]
        [Rule(@"<Base>     ::= enum Id")]  
        public BaseTypeIdentifier(SimpleToken tok,Identifier type)
        {
            _ident = type;
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
 

   
    public class CallingCV : SimpleToken
    {
        public CallingConventions CallingConvention { get; set; }

   
        protected SimpleToken _mod;
        [Rule(@"<CallCV>      ::= stdcall")]
        [Rule(@"<CallCV>      ::= fastcall")]
        [Rule(@"<CallCV>      ::= cdecl")]
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
            else ModifierList = Modifiers.NoModifier;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }

  
    // end modifiers
}
