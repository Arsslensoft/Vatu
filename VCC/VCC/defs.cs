using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC.Core
{


    // ENUM
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


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _id = (Identifier)_id.DoResolve(rc);
            _value = (Literal)_value.DoResolve(rc);
            return base.DoResolve(rc);
        }
        public override bool Resolve(ResolveContext rc)
        {
            _value.Resolve(rc);

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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _value = (EnumValue)_value.DoResolve(rc);
            next_def = (EnumDefinition)next_def.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _value.Resolve(rc);
            next_def.Resolve(rc);
            return base.Resolve(rc);
        }
    }

    // VAR
    public class VariableItemDefinition : Definition
    {

        TypePointer _ptr;
        public VariableDefinition _vardef;
        [Rule(@"<Var Item> ::= <Pointers> <Var>")]
        public VariableItemDefinition(TypePointer ptr, VariableDefinition var)
        {
            _vardef = var;
            _ptr = ptr;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _vardef = (VariableDefinition)_vardef.DoResolve(rc);
            _ptr = _ptr.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _vardef.Resolve(rc);

            return base.Resolve(rc);
        }
    }
    public class VariableListDefinition : Definition
    {
       public VariableItemDefinition _vardef;
       public VariableListDefinition _nextvars;
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_nextvars == null)
                return null;
            else
            {
                _nextvars = (VariableListDefinition)_nextvars.DoResolve(rc);
                _vardef = (VariableItemDefinition)_vardef.DoResolve(rc);
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_vardef != null)
            _vardef.Resolve(rc);
            if (_nextvars != null)
            _nextvars.Resolve(rc);
            return base.Resolve(rc);
        }
    }
    public class ArrayVariableDefinition : Definition
    {
        public long Size { get; set; }


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
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _expr = (Expr)_expr.DoResolve(rc);
            if (_expr != null && _expr is ConstantExpression)
                Size = ((long)(((ConstantExpression)_expr).ConvertImplicitly(rc, BuiltinTypeSpec.Long)).GetValue());

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _expr.Resolve(rc);
            return base.Resolve(rc);
        }
    }
    public class VariableDefinition : Definition
    {
        public ArrayVariableDefinition _avd;
        public Identifier _id;
        public Expr expr;

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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_avd != null)
                _avd = (ArrayVariableDefinition)_avd.DoResolve(rc);

            if (expr != null)
                expr = (Expr)expr.DoResolve(rc);


            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_avd != null)
                _avd.Resolve(rc);
            if (expr != null)
                expr.Resolve(rc);
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _var = (VariableDeclaration)_var.DoResolve(rc);
            if (_next_sdef != null)
                _next_sdef = (StructDefinition)_next_sdef.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _var.Resolve(rc);
            if (_next_sdef != null)
                _next_sdef.Resolve(rc);

            return base.Resolve(rc);
        }
    }

    // Method
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_nextid == null) return null;
            _id = (TypeIdentifier)_id.DoResolve(rc);
      
            _nextid = (TypeIdentifierListDefinition)_nextid.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _id.Resolve(rc);
            _nextid.Resolve(rc);
            return base.Resolve(rc);
        }
    }
    public class ParameterDefinition : Definition
    {
        public TypeSpec ParameterType { get; set; }
        public VarSpec ParameterName { get; set; }


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

         public override SimpleToken DoResolve(ResolveContext rc)
         {
             _type = (TypeIdentifier)_type.DoResolve(rc);
             ParameterType = _type.Type;
             ParameterName = new VarSpec(_id.Name, ParameterType, loc);
             return this;
         }
         public override bool Resolve(ResolveContext rc)
         {
             _type.Resolve(rc);
             
      
             return base.Resolve(rc);
         }
    }
    public class ParameterListDefinition : Definition
    {
        public ParameterDefinition _id;
       public ParameterListDefinition _nextid;
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

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           if (_nextid == null) return null;
           _id = (ParameterDefinition)_id.DoResolve(rc);
           _nextid = (ParameterListDefinition)_nextid.DoResolve(rc);
           return this;
       }
       public override bool Resolve(ResolveContext rc)
       {
           _id.Resolve(rc);
           _nextid.Resolve(rc);
           return base.Resolve(rc);
       }
    }


}
