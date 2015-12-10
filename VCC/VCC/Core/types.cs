using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VCC.Core
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

        [Rule(@"<Scalar>   ::= char")]
        [Rule(@"<Scalar>   ::= int")]
        [Rule(@"<Scalar>   ::= uint")]
        [Rule(@"<Scalar>   ::= short")]
        [Rule(@"<Scalar>   ::= long")]
        [Rule(@"<Scalar>   ::= ushort")]
        [Rule(@"<Scalar>   ::= ulong")]
        [Rule(@"<Scalar>   ::= schar")]
        [Rule(@"<Scalar>   ::= bool")]
        [Rule(@"<Scalar>   ::= extended")]
        [Rule(@"<Scalar>   ::= float")]
        [Rule(@"<Scalar>   ::= double")]
        [Rule(@"<Scalar>   ::= void")]
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
        [Rule(@"<Base>     ::= union Id")]
        public BaseTypeIdentifier(SimpleToken tok,Identifier type)
        {
            _ident = type;
        }
      
        [Rule(@"<Base>     ::= <Scalar>")]
        public BaseTypeIdentifier(ScalarTypeIdentifier type)
        {
            _typeid = type;
        }

     
        [Rule(@"<Base>     ::= ~struct ~'{' <Struct Def> ~'}'")]
        public BaseTypeIdentifier(StructDefinition sdef)
        {
            isstruct = true;
        _sdef = sdef;
        }
        [Rule(@"<Base>     ::= union ~'{' <Struct Def> ~'}'")]
        public BaseTypeIdentifier(SimpleToken tok,StructDefinition sdef)
        {
            _sdef = sdef;
            isstruct = false;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_typeid != null)
            {
                _typeid = (TypeToken)_typeid.DoResolve(rc);
                Type = _typeid.Type;
            }
            if (_ident != null)
                Type = rc.ResolveType(_ident.Name);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_typeid != null)
              _typeid.Resolve(rc);

            if (_ident != null)
                Type = rc.ResolveType(_ident.Name);
            return base.Resolve(rc);
        }
    }

    [Terminal("struct")]
    public class StructTypeIdentifer : SimpleToken
    {

    }
    [Terminal("enum")]
    public class EnumTypeIdentifer : SimpleToken
    {

    }
    [Terminal("union")]
    public class UnionTypeIdentifer : SimpleToken
    {

    }

    // END TYPE
    // START MODIFIERs
    public class Modifier : ModifierToken
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
        [Rule(@"<Mod>      ::= register")]
        [Rule(@"<Mod>      ::= auto")]
        [Rule(@"<Mod>      ::= volatile")]
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
            else if (_mod.Name == "register")
                ModifierList = Modifiers.Register;
            else if (_mod.Name == "auto")
                ModifierList = Modifiers.Auto;
            else if (_mod.Name == "volatile")
                ModifierList = Modifiers.Volatile;
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

    [Terminal("extern")]
    public class ExternModifier : SimpleToken
    {

    }
    [Terminal("static")]
    public class StaticModifier : SimpleToken
    {

    }
    [Terminal("register")]
    public class RegisterModifier : SimpleToken
    {

    }
    [Terminal("auto")]
    public class AutoModifier : SimpleToken
    {

    }
    [Terminal("volatile")]
    public class VolatileModifier : SimpleToken
    {

    }
    [Terminal("const")]
    public class ConstModifier : SimpleToken
    {

    }
    // end modifiers
}
