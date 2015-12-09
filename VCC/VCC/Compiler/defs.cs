using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
{
    // ENUM

    /*
     
<Enum Decl>    ::= enum Id '{' <Enum Def> '}'  ';'
 
<Enum Def>     ::= <Enum Val> ',' <Enum Def>
                 | <Enum Val>

<Enum Val>     ::= Id
                 | Id '=' OctLiteral
                 | Id '=' HexLiteral
                 | Id '=' DecLiteral  

     */

    public class EnumValue : Definition
    {
        Identifier _id;
        Literal _value;
        [Rule(@"<Enum Val> ::= Id ~'=' HexLiteral")]
        public EnumValue(Identifier id, HexLiteral value)
        {
            _id = id;
            _value = value;
        }
        [Rule(@"<Enum Val> ::= Id ~'=' OctLiteral")]
        public EnumValue(Identifier id, OctLiteral value)
        {
            _id = id;
            _value = value;
        }
        [Rule(@"<Enum Val> ::= Id ~'=' DecLiteral")]
        public EnumValue(Identifier id, DecLiteral value)
        {
            _id = id;
            _value = value;
        }

        [Rule(@"<Enum Val>     ::= Id")]
        public EnumValue(Identifier id)
        {
            _id = id;
            _value = null;
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
    public class EnumDefinition : Definition
    {
        EnumDefinition next_def;
        EnumValue _value;
        [Rule(@"<Enum Def>     ::= <Enum Val> ~',' <Enum Def>")]
        public EnumDefinition(EnumValue val, EnumDefinition def)
        {
            _value = val;
            next_def = def;
        }
        [Rule(@"<Enum Def>     ::= <Enum Val>")]
         public EnumDefinition(EnumValue val)
        {
                _value = val;
            next_def = null;
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

    // VAR
    /*
     <Var Decl>     ::= <Mod> <Type> <Var> <Var List>  ';'
                 |       <Type> <Var> <Var List>  ';'
                 | <Mod>        <Var> <Var List>  ';'
             
<Var>      ::= ID <Array>
             | ID <Array> '=' <Op If> 

<Array>    ::= '[' <Expression> ']'
             | '[' ']'
             |
             
<Var List> ::=  ',' <Var Item> <Var List>
             | 

<Var Item> ::= <Pointers> <Var>
    */
    public class VariableItemDefinition : Definition
    {
        TypePointer _ptr;
        VariableDefinition _vardef;
        [Rule(@"<Var Item> ::= <Pointers> <Var>")]
        public VariableItemDefinition(TypePointer ptr, VariableDefinition var)
        {
            _vardef = var;
            _ptr = ptr;
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
    public class VariableListDefinition : Definition
    {
        VariableItemDefinition _vardef;
        VariableListDefinition _nextvars;
        [Rule(@"<Var List> ::=  ~',' <Var Item> <Var List>")]
        public VariableListDefinition(VariableItemDefinition ptr, VariableListDefinition var)
        {
            _vardef = ptr;
            _nextvars = var;
        }

        [Rule(@"<Var List> ::=  ")]
        public VariableListDefinition()
        {

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
    public class ArrayVariableDefinition : Definition
    {
        Expr _expr;
         [Rule(@"<Array>    ::= ~'[' <Expression> ~']'")]
        public ArrayVariableDefinition(Expr expr)
        {
            _expr = expr;
        }
    
        [Rule(@"<Array>    ::= ~'[' ~']'")]
         public ArrayVariableDefinition()
         {
             _expr = null;
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
    public class VariableDefinition : Definition
    {
        ArrayVariableDefinition _avd;
        Identifier _id;
        Expr expr;

        [Rule(@"<Var>      ::= Id <Array>")]
        public VariableDefinition(Identifier id, ArrayVariableDefinition avd)
        {
            expr = null;
            _id = id;
            _avd = avd;
        }
        [Rule(@"<Var>      ::= Id")]
        public VariableDefinition(Identifier id)
        {
            expr = null;
            _id = id;
            _avd = null;
        }

        [Rule(@"<Var>      ::= Id <Array> ~'=' <Op If> ")]
        public VariableDefinition(Identifier id, ArrayVariableDefinition avd, Expr ifexpr)
        {
            expr = ifexpr;
            _id = id;
            _avd = avd;
        }
        [Rule(@"<Var>      ::= Id ~'=' <Op If> ")]
        public VariableDefinition(Identifier id, Expr ifexpr)
        {
            expr = ifexpr;
            _id = id;
            _avd = null;
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

    // STRUCT
    public class StructDefinition : Definition
    {
        VariableDeclaration _var;
        StructDefinition _next_sdef;
        [Rule(@"<Struct Def>   ::= <Var Decl> <Struct Def>")]
        public StructDefinition(VariableDeclaration var, StructDefinition sdef)
        {
            _var = var;
            _next_sdef = sdef;
         }
        [Rule(@"<Struct Def>   ::= <Var Decl>")]
        public StructDefinition(VariableDeclaration var)
        {
            _var = var;
            _next_sdef = null;
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

    // Method
    public class IdentifierListDefinition : Definition
    {
        Identifier _id;
        IdentifierListDefinition _nextid;
        [Rule(@"<Id List>    ::= Id ~',' <Id List>")]
        public IdentifierListDefinition(Identifier ptr, IdentifierListDefinition var)
        {
            _id = ptr;
            _nextid = var;
        }

        [Rule(@"<Id List>    ::= Id")]
        public IdentifierListDefinition(Identifier id)
            : this(id,null)
        {

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
    public class TypeIdentifierListDefinition : Definition
    {
        TypeIdentifier _id;
        TypeIdentifierListDefinition _nextid;
        [Rule(@"<Types>      ::= <Type>  ~',' <Types>")]
        public TypeIdentifierListDefinition(TypeIdentifier ptr, TypeIdentifierListDefinition var)
        {
            _id = ptr;
            _nextid = var;
        }

        [Rule(@"<Types>      ::= <Type>")]
        public TypeIdentifierListDefinition(TypeIdentifier id)
            : this(id, null)
        {

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
    public class ParameterDefinition : Definition
    {
        Identifier _id;
        TypeIdentifier _type;
        bool constant = false;
        [Rule(@"<Param>      ::= const <Type> Id")]
        public ParameterDefinition(SimpleToken tok,TypeIdentifier ptr, Identifier var)
        {
            _id = var;
            _type = ptr;
            constant = true;
        }

         [Rule(@"<Param>      ::= <Type> Id")]
        public ParameterDefinition(TypeIdentifier ptr, Identifier var)
        {
            _id = var;
            _type = ptr;
            constant = false;
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
    public class ParameterListDefinition : Definition
    {
        ParameterDefinition _id;
        ParameterListDefinition _nextid;
        [Rule(@"<Params>     ::= <Param> ~',' <Params>")]
        public ParameterListDefinition(ParameterDefinition ptr, ParameterListDefinition var)
        {
            _id = ptr;
            _nextid = var;
        }

       [Rule(@"<Params>     ::= <Param>")]
        public ParameterListDefinition(ParameterDefinition id)
            : this(id, null)
        {

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
