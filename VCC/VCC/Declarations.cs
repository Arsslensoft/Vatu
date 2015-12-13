using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VCC.Core
{
    public class Declaration : DeclarationToken
    {
        public Declaration BaseDeclaration { get; set; }

        protected Identifier _name;
        protected TypeToken _type;
        public bool IsTypeDef { get { return (BaseDeclaration is StructDeclaration) || (BaseDeclaration is TypeDefDeclaration) || (BaseDeclaration is EnumDeclaration); } }
        public bool IsStruct { get { return (BaseDeclaration is StructDeclaration) ; } }
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
        public List<TypeMemberSpec> Members { get; set; }

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
        public VariableDeclaration(TypeIdentifier type, VariableDefinition var, VariableListDefinition valist)
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
            if (_valist != null)
                _valist = (VariableListDefinition)_valist.DoResolve(rc);

            if (_mod != null)
                _mod = (Modifier)_mod.DoResolve(rc);
            _type = (TypeIdentifier)_type.DoResolve(rc);
            this.Type = _type.Type;
            if (_mod != null)
                mods = _mod.ModifierList;
            else mods = Modifiers.NoModifier;
            if (rc.IsInGlobal() && !rc.IsInTypeDef)
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
            else if (!rc.IsInTypeDef && !rc.IsInStruct)
            {
                FieldOrLocal = new VarSpec(_vadef._id.Name, rc.CurrentMethod, Type, loc);
                ((VarSpec)FieldOrLocal).Initialized = (_vadef.expr != null);
                // Childs

                VariableListDefinition c = _valist;
                while (c != null)
                {
                    rc.KnowVar(new VarSpec(c._vardef._vardef._id.Name, rc.CurrentMethod, Type, loc));
                    c = _valist._nextvars;
                }
                rc.KnowVar((VarSpec)FieldOrLocal);
            }
            else if (rc.IsInStruct)
            {
                Members = new List<TypeMemberSpec>();
             
                // Childs

                VariableListDefinition c = _valist;
               while (c != null)
                {
                    Members.Add(new TypeMemberSpec(c._vardef.Name, rc.CurrentType, Type, loc,0));
                    c = _valist._nextvars;
                }
               Members.Add(new TypeMemberSpec(_vadef._id.Name, rc.CurrentType, Type, loc, 0));
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
         bool ok =   _vadef.Resolve(rc);
            if (_valist != null)
             ok &=   _valist.Resolve(rc);
            if (_mod != null)
              ok &=  _mod.Resolve(rc);

           ok &= _type.Resolve(rc);
            return ok;
        }

        public override bool Emit(EmitContext ec)
        {
        
            _vadef.Emit(ec);
            // handle const
            if (_vadef.expr is ConstantExpression)
            {
                VarSpec v = (VarSpec)FieldOrLocal;
                ec.EmitComment("Var decl assign " + v.Name + " stack index "+v.StackIndex);
                // push const
                _vadef.expr.EmitToRegister(ec, RegistersEnum.AX);               
                ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.BP, DestinationDisplacement = (int)v.StackIndex,DestinationIsIndirect = true, SourceReg = RegistersEnum.AX ,Size = 80});
            }
            else if (_vadef.expr == null && Type.IsBuiltinType)
            {

                VarSpec v = (VarSpec)FieldOrLocal;
                ec.EmitComment("Var decl assign " + v.Name + " stack index " + v.StackIndex);
                // push const
                ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.BP, DestinationDisplacement = (int)v.StackIndex, DestinationIsIndirect = true, SourceValue =0, Size = 80 });
            }
            else if (_vadef.expr == null && Type.IsStruct  && FieldOrLocal is FieldSpec)
                ec.AddInstanceOfStruct(FieldOrLocal.Signature.ToString(), ((FieldSpec)FieldOrLocal).MemberType);
            else if (Type.IsStruct && (_vadef.expr == null || !(FieldOrLocal is FieldSpec)))
            {
                // error struct cannot be a local var
               
            }
              

            if (_valist != null)
                _valist.Emit(ec);

            return true;
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
            TypeName = new TypeSpec(_name.Name, _typedef.Type.Size, BuiltinTypes.Unknown, TypeFlags.TypeDef, Modifiers.NoModifier, loc, _typedef.Type);
            rc.KnowType(TypeName);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {


            return _typedef.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }

    }




    // Working Emit and Resolve
    public class MethodDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.NoModifier;
        public bool EntryPoint = false;
        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }

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


        [Rule(@"<Func Decl> ::= entry <Func ID> ~'(' <Params> ~')' <Block>")]
        public MethodDeclaration(SimpleToken t, MethodIdentifier id, ParameterListDefinition pal, Block b)
        {
            EntryPoint = true;
            _name = id.Id;
            _id = id;
            _pal = pal;
            _b = b;
        }

        [Rule(@"<Func Decl> ::= entry <Func ID> ~'(' ~')' <Block>")]
        public MethodDeclaration(SimpleToken t, MethodIdentifier id, Block b)
        {
            EntryPoint = true;
            _name = id.Id;
            _id = id;
            _pal = null;
            _b = b;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _id = (MethodIdentifier)_id.DoResolve(rc);
            base._type = _id.Type;
         
    
     
            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            if (_pal != null)
            {
                _pal.Resolve(rc);
                _pal = (ParameterListDefinition)_pal.DoResolve(rc);
                ParameterListDefinition par = _pal;
                while (par != null)
                {
                    if (par._id != null)
                        Params.Push(par._id.ParameterName);
                    par = par._nextid;
                }
            }
            method = new MethodSpec(_id.Name, mods, _id.Type.Type, this.loc);
            method.Parameters = Parameters;
            rc.KnowMethod(method);
            rc.CurrentMethod = method;
            if (_b != null)
                _b = (Block)_b.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _id.Resolve(rc);

            if (_pal != null)
                ok &= _pal.Resolve(rc);
            if (_b != null)
                ok &= _b.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            if (EntryPoint)
                ec.SetEntry(method.Signature.ToString());
            Label mlb = ec.DefineLabel(method.Signature.ToString());
            ec.MarkLabel(mlb);
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = RegistersEnum.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.BP, SourceReg = RegistersEnum.SP, Size = 80 });
            // allocate variables

            uint size = 0;
            foreach (VarSpec v in ec.CurrentResolve.GetLocals())
                size += (uint)v.MemberType.Size;
        
            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = size, Size = 80 });
            //EMit params
            // Get Parameters Indexes
            int paramidx = 2; // Initial Stack Position
            ParameterSpec p = null;
            while (Params.Count > 0)
            {
                p = Params.Pop();
                p.StackIndex = paramidx;
                Parameters.Add(p);

                paramidx += 2;
            }

            ec.EmitComment("Block");
            // Emit Code
            if (_b != null)
                _b.Emit(ec);

            ec.EmitComment("return label");
            // Return Label
            ec.MarkLabel(ec.DefineLabel(method.Name + "_ret"));
            // Destroy Stack Frame
            ec.EmitComment("destroy stackframe");
            ec.EmitInstruction(new Leave());
            // ret
            ec.EmitInstruction(new SimpleReturn());
            return true;
        }
    }
    public class MethodPrototypeDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.NoModifier;
        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }


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
               method = new MethodSpec(_id.Name, mods | Modifiers.Prototype, _id.Type.Type, this.loc);
            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            if (_pal != null)
            {
                _pal.Resolve(rc);
                _pal = (ParameterListDefinition)_pal.DoResolve(rc);
                ParameterListDefinition par = _pal;
                while (par != null)
                {
                    if (par._id != null)
                        Params.Push(par._id.ParameterName);
                    par = par._nextid;
                    Parameters.Add(par._id.ParameterName);
                }
            }
            else if (_tdl != null)
            {
                _tdl.Resolve(rc);
                _tdl = (TypeIdentifierListDefinition)_tdl.DoResolve(rc);
                TypeIdentifierListDefinition par = _tdl;
                int paid = 0;
                while (par != null)
                {
                    if (par._id != null)
                    {
                        ParameterSpec p = new ParameterSpec("param_" + paid, method, par._id.Type, false, par.loc);
                        Parameters.Add(p);
                        Params.Push(p);
                    }
                    par = par._nextid;
                    paid++;
                }
            }


            method.Parameters = Parameters;
            rc.KnowMethod(method);


            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _id.Resolve(rc);

            if (_pal != null)
                ok &= _pal.Resolve(rc);

            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.DefineExtern(method);
            return true;
        }
    }
    public class StructDeclaration : Declaration
    {
        public StructTypeSpec TypeName { get; set; }
        public int Size { get; set; }
        StructDefinition _def;
        [Rule(@"<Struct Decl>  ::= ~struct Id ~'{' <Struct Def> ~'}'  ~';' ")]
        public StructDeclaration(Identifier id, StructDefinition sdef)
        {
            _name = id;
            _def = sdef;
            Size = 0;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _def = (StructDefinition)_def.DoResolve(rc);
            if (_def != null)
                Size = _def.Size;
            TypeName = new StructTypeSpec(_name.Name, Size, _def.Members, loc);
            rc.KnowType(TypeName);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {


            return _def.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {

            ec.EmitStructDef(TypeName);

            return true;
        }
    }
}
