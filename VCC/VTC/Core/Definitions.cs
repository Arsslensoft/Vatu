using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
 

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
            _ptr = (TypePointer)_ptr.DoResolve(rc);
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
            if (_nextvars != null)
            {
                _nextvars = (VariableListDefinition)_nextvars.DoResolve(rc);

                _vardef = (VariableItemDefinition)_vardef.DoResolve(rc);
                return this;
            }
            else return null;
       
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
            bool conv = false;
            if (_expr != null)
            _expr = (Expr)_expr.DoResolve(rc);

            if (_expr != null && _expr is ConstantExpression)
                Size = int.Parse((((ConstantExpression)_expr).ConvertImplicitly(rc, BuiltinTypeSpec.Int, ref conv)).GetValue().ToString());
            else Size = 0;

            if (Size < 0)
                ResolveContext.Report.Error(47, Location, "Invalid array size");
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
      if (_expr != null)
            return _expr.Resolve(rc);
      return true;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    public class VariableDefinition : Definition
    {
        public int ArraySize { get; set; }
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
            ArraySize = -1;
            if (_avd != null)
            {
                
                _avd = (ArrayVariableDefinition)_avd.DoResolve(rc);
                if (_avd != null)
                    ArraySize = _avd.Size;
            }
           
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



    public class FunctionExtensionDefinition : Definition
    {
        public TypeSpec ExtendedType { get; set; }
        public bool Static = false;
        TypeToken tt;
        [Rule(@"<Func Ext>  ::= ~extends <Type>")]
        public FunctionExtensionDefinition(TypeToken t)
      {
          tt = t;
      }
        [Rule(@"<Func Ext>  ::= static ~extends <Type>")]
        public FunctionExtensionDefinition(SimpleToken st,TypeToken t)
        {
            tt = t; Static = true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            tt = (TypeToken)tt.DoResolve(rc);
            ExtendedType = tt.Type;
            return this;
        }
    }

    public class FunctionBodyDefinition : Definition
    {
       public Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }
        public List<TypeSpec> ParamTypes { get; set; }

        public ParameterListDefinition _pdef;
        public Block _b;
        public FunctionExtensionDefinition _ext;
      [Rule(@"<Func Body>  ::= ~'(' <Params>  ~')' <Block>")]
        public FunctionBodyDefinition(ParameterListDefinition pdef, Block b)
        {
            _pdef = pdef;
            _b = b;
        }
      [Rule(@"<Func Body>  ::= ~'('  ~')' <Block>")]
      public FunctionBodyDefinition( Block b)
      {
          _pdef = null;
          _b = b;
      }
      [Rule(@"<Func Body>  ::= ~'(' <Params>  ~')' <Func Ext>  <Block>")]
      public FunctionBodyDefinition(ParameterListDefinition pdef, FunctionExtensionDefinition ext,Block b)
      {
          _pdef = pdef;
          _b = b;
          _ext = ext;
      }
      [Rule(@"<Func Body>  ::= ~'('  ~')' <Func Ext> <Block>")]
      public FunctionBodyDefinition( FunctionExtensionDefinition ext,Block b)
      {
          _pdef = null;
          _b = b;
          _ext = ext;
      }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if(_ext != null)
            _ext = (FunctionExtensionDefinition)_ext.DoResolve(rc);
           
            ParamTypes = new List<TypeSpec>();

            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            if (_pdef != null)
            {
                _pdef.Resolve(rc);
                _pdef = (ParameterListDefinition)_pdef.DoResolve(rc);
                ParameterListDefinition par = _pdef;
                while (par != null)
                {
                    if (par._id != null)
                    {
                        Params.Push(par._id.ParameterName);
                        Parameters.Add(par._id.ParameterName);
                        ParamTypes.Add(par._id.ParameterName.MemberType);
                    }
                    par = par._nextid;
                }
            }
            return this;
        }

        public override bool Resolve(ResolveContext rc)
        {
        
            return true;
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

    // Ready

    public class ParameterDefinition : Definition
    {
        public TypeSpec ParameterType { get; set; }
        public ParameterSpec ParameterName { get; set; }


        Identifier _id;
        TypeIdentifier _type;
        bool constant = false;
        bool REF = false;

        [Rule(@"<Param>      ::= ref <Type> Id")]
        [Rule(@"<Param>      ::= const <Type> Id")]
        public ParameterDefinition(SimpleToken tok, TypeIdentifier ptr, Identifier var)
        {
            _id = var;
            _type = ptr;
            if (tok.Name == "const")
                constant = true;
            else REF = true;
        }

        [Rule(@"<Param>      ::= <Type> Id")]
        public ParameterDefinition(TypeIdentifier ptr, Identifier var)
        {
            _id = var;
            _type = ptr;
            constant = false;
            REF = false;
        }
        
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _type = (TypeIdentifier)_type.DoResolve(rc);
            ParameterType = _type.Type;
            Modifiers mods = constant? Modifiers.Const: Modifiers.NoModifier;
            mods |= REF? Modifiers.Ref: 0;
            ParameterName = new ParameterSpec(_id.Name, rc.CurrentMethod, ParameterType, loc,4,mods );
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


    // Emited and Resolved
    public class StructDefinition : Definition
    {


        public VariableDeclaration _var;
        public StructDefinition _next_sdef;
        public int Size { get; set; }
        public List<TypeMemberSpec> Members { get; set; }


        [Rule(@"<Struct Def>   ::= <Var Decl> <Struct Def>")]
        public StructDefinition(VariableDeclaration var, StructDefinition sdef)
        {
            _var = var;
            _next_sdef = sdef;
            Size = 0;

        }
        [Rule(@"<Struct Def>   ::= <Var Decl>")]
        public StructDefinition(VariableDeclaration var)
        {

            _var = var;
            _next_sdef = null;
            Size = 0;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Members = new List<TypeMemberSpec>();
            _var = (VariableDeclaration)_var.DoResolve(rc);
            if (_var != null)
            {

                if (_var.Members[0].MemberType is ArrayTypeSpec)
                    Size += (_var.Members[0].MemberType as ArrayTypeSpec).ArrayCount * _var.Members[0].MemberType.BaseType.Size;
                else
                  Size += _var.Type.Size;
            }
            foreach (TypeMemberSpec sv in _var.Members)
                Members.Add(sv);

            if (_next_sdef != null)
            {


                _next_sdef = (StructDefinition)_next_sdef.DoResolve(rc);
                foreach (TypeMemberSpec sv in _next_sdef.Members)
                    Members.Add(sv);
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            bool ok = _var.Resolve(rc);
            if (_next_sdef != null)
                ok &= _next_sdef.Resolve(rc);

            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    public class EnumValue : Definition
    {
        public Identifier _id;
        private Literal _value;
        public ConstantExpression Value;
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
            if (_value != null)
                Value = (ConstantExpression)_value.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            if (_value != null)

                return _value.Resolve(rc);
            else return true;
        }
        public override bool Emit(EmitContext ec)
        {
            // TODO:EMIT ENUM
            return true;
        }
    }
    public class EnumDefinition : Definition
    {
  
        public int Size { get; set; }
        public List<EnumMemberSpec> Members { get; set; }
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

        EnumMemberSpec GetMember(ResolveContext rc,EnumValue v, TypeSpec host)
        {

            object val = null;
            if (v.Value != null && ((val = v.Value.GetValue()) != null))
            {
                ushort uval = ushort.Parse(val.ToString());
                if (uval > 255)
                    Size = 2;
                return new EnumMemberSpec(rc.CurrentNamespace,v._id.Name, uval, host, (uval < 256) ? BuiltinTypeSpec.Byte : BuiltinTypeSpec.UInt, v.Location);

            }
            else return new EnumMemberSpec(rc.CurrentNamespace, v._id.Name, host, BuiltinTypeSpec.UInt, v.Location);
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Size = 1;
            Members = new List<EnumMemberSpec>();
            _value = (EnumValue)_value.DoResolve(rc);
            if (next_def != null)
                next_def = (EnumDefinition)next_def.DoResolve(rc);
            Members.Add(GetMember(rc,_value, rc.CurrentType));
            EnumDefinition m = next_def;
            while (m != null)
            {
                if (m._value != null)
                    Members.Add(GetMember(rc,m._value, rc.CurrentType));
                if (m.Size == 2)
                    Size = 2;
                m = m.next_def;
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _value.Resolve(rc);
            if (next_def != null)
                ok &= next_def.Resolve(rc);

            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    }
