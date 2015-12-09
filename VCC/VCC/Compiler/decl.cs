using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
{

  
    public class EnumDeclaration : Declaration
    {

        EnumDefinition _def;
        [Rule(@"<Enum Decl>    ::= ~enum Id ~'{' <Enum Def> ~'}'  ~';'")]
        public EnumDeclaration(Identifier id, EnumDefinition edef)
        {
            _name = id;
            _def = edef;

        }

        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }

    }
    public class VariableDeclaration : Declaration
    {
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

        public override bool Resolve(ResolveContext rc)
        {
            _type.Resolve(rc);
            return base.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
      
      
    }
    public class TypeDefDeclaration : Declaration
    {

        TypeIdentifier _typedef;
        [Rule(@"<Typedef Decl> ::= ~typedef <Type> Id ~';'")]
        public TypeDefDeclaration(TypeIdentifier type, Identifier id)

        {
            _name = id;
            _typedef = type;

        }
        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }
    public class StructDeclaration : Declaration
    {

        StructDefinition _def;
        [Rule(@"<Struct Decl>  ::= ~struct Id ~'{' <Struct Def> ~'}'  ~';' ")]
        public StructDeclaration(Identifier id, StructDefinition sdef)
        {
            _name = id;
            _def = sdef;

        }
        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }
    public class UnionDeclaration : Declaration
    {

        StructDefinition _def;
        [Rule(@"<Union Decl>   ::= ~union Id ~'{' <Struct Def> ~'}'  ~';' ")]
        public UnionDeclaration(Identifier id, StructDefinition sdef)
        {
            _name = id;
            _def = sdef;

        }
        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }
    public class MethodDeclaration : Declaration
    {
      

        MethodIdentifier _id; ParameterListDefinition _pal; Block _b;
        [Rule(@"<Func Decl> ::= <Func ID> ~'(' <Params> ~')' <Block>")]
        public MethodDeclaration(MethodIdentifier id, ParameterListDefinition pal, Block b)
        {
            _id = id;
            _pal = pal;
            _b = b;
        }

        IdentifierListDefinition _idl; StructDefinition _sdef;
        [Rule(@"<Func Decl> ::= <Func ID> ~'(' <Id List> ~')' <Struct Def> <Block>")]
        public MethodDeclaration(MethodIdentifier id, IdentifierListDefinition idl, StructDefinition sdef, Block b)
        {
            _id = id;
            _pal = null;
            _b = b;
            _idl = idl;
            _sdef = sdef;
        }

        [Rule(@"<Func Decl> ::= <Func ID> ~'(' ~')' <Block>")]
        public MethodDeclaration(MethodIdentifier id, Block b)
        {
            _id = id;
            _pal = null;
            _b = b;
        }
        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
     
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_b != null)
                _b.Resolve(rc);
            return base.Resolve(rc);
        }
    }
    public class MethodPrototypeDeclaration : Declaration
    {

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

        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }


   
}
