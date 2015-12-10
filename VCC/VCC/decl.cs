using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VCC.Core
{
    public class Declaration : DeclarationToken
    {
      public  Declaration BaseDeclaration {get;set;}

      protected  Identifier _name;
       protected TypeToken _type;

       public TypeToken Type
       {
           get { return _type; }
       }
       public Identifier Identifier
       {
           get { return _name; }
       }

       public Declaration()
       {

       }

        [Rule(@"<Decl>  ::= <Func Decl>")]
        [Rule(@"<Decl>  ::= <Func Proto>")]
        [Rule(@"<Decl>  ::= <Struct Decl>")]
        [Rule(@"<Decl>  ::= <Enum Decl>")]
        [Rule(@"<Decl>  ::= <Var Decl>")]
        [Rule(@"<Decl>  ::= <Typedef Decl>")]
        public Declaration(Declaration decl)
        {
            BaseDeclaration = decl;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (BaseDeclaration != null)
                return BaseDeclaration.DoResolve(rc);
            else return base.DoResolve(rc);
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (BaseDeclaration != null)
                return BaseDeclaration.Resolve(rc);
            return base.Resolve(rc);
        }

        public override bool Emit(EmitContext ec)
        {
            if (BaseDeclaration != null)
                return BaseDeclaration.Emit(ec);

            return base.Emit(ec);
        }
  
    }


  
    public class EnumDeclaration : Declaration
    {

        EnumDefinition _def;
        [Rule(@"<Enum Decl>    ::= ~enum Id ~'{' <Enum Def> ~'}'  ~';'")]
        public EnumDeclaration(Identifier id, EnumDefinition edef)
        {
            _name = id;
            _def = edef;

        }

    }
    

    // RESOLVED
  
    
    public class VariableDeclaration : Declaration
    {
        public Modifiers mods;
        public TypeSpec Type { get; set; }
        public MemberSpec FieldOrLocal { get; set; }

        Modifier _mod;
        TypeIdentifier _type;
        VariableDefinition _vadef;
        VariableListDefinition _valist;
        [Rule(@"<Var Decl>     ::= <Mod> <Type> <Var> <Var List>  ~';'")]
        public VariableDeclaration(Modifier mod, TypeIdentifier type, VariableDefinition var, VariableListDefinition valist)
        {
            _mod = mod;
            _type = type;
            _vadef = var;
            _valist = valist;


        }
        [Rule(@"<Var Decl>     ::=  <Type> <Var> <Var List>  ~';'")]
        public VariableDeclaration( TypeIdentifier type, VariableDefinition var, VariableListDefinition valist)
        {
            _mod = null;
            _type = type;
            _vadef = var;
            _valist = valist;


        }
        [Rule(@"<Var Decl>     ::= <Mod>        <Var> <Var List> ~';'")]
        public VariableDeclaration(Modifier mod, VariableDefinition var, VariableListDefinition valist)
        {
            _mod = mod;
            _type = null;
            _vadef = var;
            _valist = valist;


        }


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _vadef = (VariableDefinition)_vadef.DoResolve(rc);
            if(_valist != null)
            _valist = (VariableListDefinition)_valist.DoResolve(rc);
           
            if (_mod != null)
                _mod = (Modifier)_mod.DoResolve(rc);
            _type = (TypeIdentifier)_type.DoResolve(rc);
            this.Type = _type.Type;
            if (_mod != null)
                mods = _mod.ModifierList;
            else mods = Modifiers.NoModifier;
            if (rc.IsInGlobal())
            {
                FieldOrLocal = new FieldSpec(_vadef._id.Name, mods, Type, loc);
                // Childs

                VariableListDefinition c = _valist;
                while (c != null)
                {
                    rc.KnowField(new FieldSpec(c._vardef.Name, mods, Type, loc));
                    c = _valist._nextvars;
                }
                rc.KnowField((FieldSpec)FieldOrLocal);
            }
            else
            {
                FieldOrLocal = new VarSpec(_vadef._id.Name,rc.CurrentMethod ,Type, loc);
                // Childs

                VariableListDefinition c = _valist;
                while (c != null)
                {
                    rc.KnowVar(new VarSpec(c._vardef._vardef._id.Name, rc.CurrentMethod, Type, loc));
                    c = _valist._nextvars;
                }
                rc.KnowVar((VarSpec)FieldOrLocal);
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _vadef.Resolve(rc);
            if (_valist != null)
                _valist.Resolve(rc);
            if(_mod != null)
              _mod.Resolve(rc);

            _type.Resolve(rc);
            return base.Resolve(rc);
        }

        public override bool Emit(EmitContext ec)
        {
            DataMember dm;
            _vadef.Emit(ec);
            // handle const
            if (_vadef.expr is ConstantExpression)
            {
                object value = ((ConstantExpression)(_vadef.expr)).GetValue();
                if (value is string)
                    dm = new DataMember(FieldOrLocal.Signature.ToString(), Encoding.ASCII.GetBytes(value.ToString()));
                else if(value is bool)
                    dm = new DataMember(FieldOrLocal.Signature.ToString(), ((bool)value)?(new byte[1] {1 }):(new byte[1] {0 }));
               else if(value is byte)
                    dm = new DataMember(FieldOrLocal.Signature.ToString(), new byte[1] { (byte)value });
                else if (value is ushort)
                    dm = new DataMember(FieldOrLocal.Signature.ToString(), new ushort[1] { (ushort)value });
                else dm = new DataMember(FieldOrLocal.Signature.ToString(), new object[1] { value });
                ec.EmitData(dm);
            }

            if (_valist != null)
                _valist.Emit(ec);

            return base.Emit(ec);
        }
    }
    public class TypeDefDeclaration : Declaration
    {
        public TypeSpec TypeName { get; set; }

        TypeIdentifier _typedef;
        [Rule(@"<Typedef Decl> ::= ~typedef <Type> Id ~';'")]
        public TypeDefDeclaration(TypeIdentifier type, Identifier id)

        {
            _name = id;
            _typedef = type;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _typedef = (TypeIdentifier)_typedef.DoResolve(rc);
            TypeName = new TypeSpec(_name.Name, BuiltinTypes.Unknown, TypeFlags.TypeDef, Modifiers.NoModifier, loc, _typedef.Type);
            rc.KnowType(TypeName);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            _typedef.Resolve(rc);
            return base.Resolve(rc);
        }

    }
    public class StructDeclaration : Declaration
    {
        public TypeSpec TypeName { get; set; }

        StructDefinition _def;
        [Rule(@"<Struct Decl>  ::= ~struct Id ~'{' <Struct Def> ~'}'  ~';' ")]
        public StructDeclaration(Identifier id, StructDefinition sdef)
        {
            _name = id;
            _def = sdef;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _def = (StructDefinition)_def.DoResolve(rc);
            TypeName = new TypeSpec(_name.Name, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, loc);
            rc.KnowType(TypeName);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _def.Resolve(rc);
            return base.Resolve(rc);
        }
    }
    public class MethodDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.NoModifier;
        TypeSpec[] parameters;

        MethodIdentifier _id; ParameterListDefinition _pal; Block _b;
        [Rule(@"<Func Decl> ::= <Func ID> ~'(' <Params> ~')' <Block>")]
        public MethodDeclaration(MethodIdentifier id, ParameterListDefinition pal, Block b)
        {
            _name = id.Id;
            _id = id;
            _pal = pal;
            _b = b;
        }

        [Rule(@"<Func Decl> ::= <Func ID> ~'(' ~')' <Block>")]
        public MethodDeclaration(MethodIdentifier id, Block b)
        {
            _name = id.Id;
            _id = id;
            _pal = null;
            _b = b;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _id = (MethodIdentifier)_id.DoResolve(rc);
            base._type= _id.Type;


            List<TypeSpec> PARAM = new List<TypeSpec>();
            if (_pal != null)
            {
                _pal.Resolve(rc);
                _pal = (ParameterListDefinition)_pal.DoResolve(rc);
                ParameterListDefinition c = _pal;
                while (c != null)
                {
                    PARAM.Add(_pal._id.ParameterType);
                    c = _pal._nextid;
                }
                parameters = PARAM.ToArray();
            }
            method = new MethodSpec(_id.Name, mods, _id.Type.Type, parameters, this.loc);

            rc.KnowMethod(method);

            if (_b != null)
               _b = (Block) _b.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _id.Resolve(rc);
       
          if (_pal != null)
                _pal.Resolve(rc);
            if(_b != null)
              _b.Resolve(rc);
            return base.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
       Label mlb =     ec.DefineLabel(method.Signature.ToString());
       ec.MarkLabel(mlb);
      //EMit params
            if (_b != null)
             _b.Emit(ec);

            return base.Emit(ec);
        }
    }
    public class MethodPrototypeDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.NoModifier;
        TypeSpec[] parameters;


        MethodIdentifier _id; ParameterListDefinition _pal;
        [Rule(@" <Func Proto> ::= <Func ID> ~'(' <Types> ~')' ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id, TypeIdentifierListDefinition tdl)
        {
            _id = id;
            _tdl = tdl;
        }

        TypeIdentifierListDefinition _tdl;
        [Rule(@"<Func Proto> ::= <Func ID> ~'(' <Params> ~')' ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id, ParameterListDefinition pal)
        {
            _id = id;
            _pal = pal;
      
        }

        [Rule(@"<Func Proto> ::= <Func ID> ~'(' ~')' ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id)
        {
            _id = id;
            _pal = null;
    
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _id = (MethodIdentifier)_id.DoResolve(rc);
            base._type = _id.Type;


            List<TypeSpec> PARAM = new List<TypeSpec>();
            if (_pal != null)
            {
                _pal.Resolve(rc);
                _pal = (ParameterListDefinition)_pal.DoResolve(rc);
                ParameterListDefinition c = _pal;
                while (c != null)
                {
                    PARAM.Add(_pal._id.ParameterType);
                    c = _pal._nextid;
                }
                parameters = PARAM.ToArray();
            }
            method = new MethodSpec(_id.Name, mods | Modifiers.Prototype, _id.Type.Type, parameters, this.loc);

            rc.KnowMethod(method);

       
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _id.Resolve(rc);

            if (_pal != null)
                _pal.Resolve(rc);
 
            return base.Resolve(rc);
        }
    }


   
}
