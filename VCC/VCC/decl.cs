using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
{
    public class Declaration : DeclarationToken
    {
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
        [Rule(@"<Decl>  ::= <Union Decl>")]
        [Rule(@"<Decl>  ::= <Enum Decl>")]
        [Rule(@"<Decl>  ::= <Var Decl>")]
        [Rule(@"<Decl>  ::= <Typedef Decl>")]
        public Declaration(Declaration decl)
        {

        }

  
    }
    public class MethodIdentifier : Identifier
    {
        public Identifier Id { get; set; }
        public TypeToken Type { get; set; }

        [Rule(@"<Func ID> ::= <Type> Id")]
        public MethodIdentifier(TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            Type = type;
        }

                [Rule(@"<Func ID> ::= Id")]
        public MethodIdentifier( Identifier id)
            : base(id.Name)
        {
            Id = id;
            Type = null;
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
    }


   
}
