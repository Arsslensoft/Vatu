using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VCC.Core
{
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
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
 

           return _value.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            // TODO:EMIT ENUM
            return true;
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

           return _value.Resolve(rc) && next_def.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }

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
           return _vardef.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
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
            bool ok = true;
            if (_vardef != null)
                ok &= _vardef.Resolve(rc);
            if (_nextvars != null)
               ok &= _nextvars.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    public class ArrayVariableDefinition : Definition
    {
        public int Size { get; set; }


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
                Size = ((int)(((ConstantExpression)_expr).ConvertImplicitly(rc, BuiltinTypeSpec.Int)).GetValue());

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
    
            return _expr.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
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
            //TODO:ARRAY SUPPORT
            bool ok = true;
            if (_avd != null)
               ok &= _avd.Resolve(rc);
            if (expr != null)
                ok &= expr.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            bool ok = true;
            if (_avd != null)
                ok &= _avd.Emit(ec);

            if (expr != null)
            {
                ok &= expr.Emit(ec);
         // TODO
            }
            return ok;
        }
    }

    public class StructDefinition : Definition
    {


        public VariableDeclaration _var;
        public StructDefinition _next_sdef;
        public int Size { get; set; }
        public StructVar SVar { get; set; }
        public List<StructVar> Variables { get; set; }
        [Rule(@"<Struct Def>   ::= <Var Decl> <Struct Def>")]
        public StructDefinition(VariableDeclaration var, StructDefinition sdef)
        {
            _var = var;
            _next_sdef = sdef;
            Size = 0;
            Variables = new List<StructVar>();
        }
        [Rule(@"<Struct Def>   ::= <Var Decl>")]
        public StructDefinition(VariableDeclaration var)
        {
            Variables = new List<StructVar>();
            _var = var;
            _next_sdef = null;
            Size = 0;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            SVar = new StructVar();
            _var = (VariableDeclaration)_var.DoResolve(rc);
            if (_var != null)
                Size += _var.Type.Size;
            SVar.IsStruct = _var.Type.IsStruct;
            SVar.Size = (_var.Type.Size == 2) ? 1 : _var.Type.Size;
            SVar.IsByte = (_var.Type.Size == 1) ;
            SVar.Name = _var.FieldOrLocal.Signature.ToString() ;
            Variables.Add(SVar);
          
            if (_var.Type.IsStruct)
            {
                
            }
            if (_next_sdef != null){


                _next_sdef = (StructDefinition)_next_sdef.DoResolve(rc);
                  foreach(StructVar sv in _next_sdef.Variables)
                      Variables.Add(sv);
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
           
          bool ok =  _var.Resolve(rc);
            if (_next_sdef != null)
            ok &=    _next_sdef.Resolve(rc);

            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }



    public class TypeIdentifierListDefinition : Definition
    {
        
       public TypeIdentifier _id;
       public TypeIdentifierListDefinition _nextid;
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

            _id = (TypeIdentifier)_id.DoResolve(rc);
            if (_nextid != null)
                _nextid = (TypeIdentifierListDefinition)_nextid.DoResolve(rc);
            return this;
        }
     
        public override bool Resolve(ResolveContext rc)
        {
            bool ok=            _id.Resolve(rc);
            if (_nextid != null)
           ok &= _nextid.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    public class ParameterDefinition : Definition
    {
        public TypeSpec ParameterType { get; set; }
        public ParameterSpec ParameterName { get; set; }


        Identifier _id;
        TypeIdentifier _type;
        bool constant = false;
        [Rule(@"<Param>      ::= const <Type> Id")]
        public ParameterDefinition(SimpleToken tok, TypeIdentifier ptr, Identifier var)
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
            ParameterName = new ParameterSpec(_id.Name, rc.CurrentMethod, ParameterType,constant, loc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            return _type.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
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
          
            _id = (ParameterDefinition)_id.DoResolve(rc);
            if (_nextid != null)
            _nextid = (ParameterListDefinition)_nextid.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
         bool ok =   _id.Resolve(rc);
            if(_nextid != null)
          ok &=  _nextid.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }

    }
