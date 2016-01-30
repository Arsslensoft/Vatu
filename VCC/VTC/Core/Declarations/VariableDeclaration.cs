using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class VariableDeclaration : Declaration
    {
        public int ArraySize { get; set; }
        public Modifiers mods;
        public bool IsAbstract = false;
      
        public List<TypeMemberSpec> Members { get; set; }

        Modifier _mod;
        TypeIdentifier _stype;
        VariableDefinition _vadef;
        VariableListDefinition _valist;
        FunctionExtensionDefinition _ext;
        [Rule(@"<Var Decl>     ::= <Mod> <Type> <Var> <Var List> <Func Ext> ~';'")]
        public VariableDeclaration(Modifier mod, TypeIdentifier type, VariableDefinition var, VariableListDefinition valist,FunctionExtensionDefinition ext )
        {
            _mod = mod;
            _stype = type;
            _vadef = var;
            _valist = valist;
            _ext = ext;

        }
        [Rule(@"<Struct Var Decl>     ::=  <Type> <Var> <Var List>  ~';'")]
        public VariableDeclaration(TypeIdentifier type, VariableDefinition var, VariableListDefinition valist)
        {
            _mod = null;
            _stype = type;
            _vadef = var;
            _valist = valist;


        }
    


        void ConvertConstant(ResolveContext rc, VariableDefinition vadef)
        {
            bool conv = false;
            // convert implicitly
            if (vadef.expr is ConstantExpression)
                vadef.expr = ((ConstantExpression)vadef.expr).ConvertImplicitly(rc, Type, ref conv);
            else if (vadef.expr is CastOperator)
                vadef.expr = ((CastOperator)vadef.expr).Target;

            if (rc.IsInGlobal() && !rc.IsInTypeDef && !rc.IsInEnum && !rc.IsInStruct && !rc.IsInUnion) // field def
                ResolveField(rc, vadef);
            else if (!rc.IsInTypeDef && !rc.IsInEnum && !rc.IsInStruct && !rc.IsInUnion) // local var definition
                ResolveLocalVariable(rc, vadef);
            else if (rc.IsInStruct || rc.IsInUnion) // struct, union member def
            {
                ResolveStructMember(rc, vadef);
                Members.Add(vadef.Member);
            }
            else
                ResolveContext.Report.Error(8, Location, "Unresolved variable declaration");


        }
        void ResolveField(ResolveContext rc, VariableDefinition vadef)
        {
            
            vadef.FieldOrLocal = new FieldSpec(rc.CurrentNamespace, vadef._id.Name, mods, Type, loc);
            // Childs


            rc.KnowField((FieldSpec)vadef.FieldOrLocal);

            // extend
            if (_ext != null)
            {
                if (!rc.Extend(_ext.ExtendedType, (FieldSpec)vadef.FieldOrLocal))
                    ResolveContext.Report.Error(0, Location, "Another field with same signature has already extended this type.");
            }

            // initial value
            if (!Type.IsBuiltinType && !Type.IsPointer && vadef.expr != null && vadef.expr is ConstantExpression)
                ResolveContext.Report.Error(2, Location, "Only builtin types and pointers can have initial value");
            // const
            if ((mods & Modifiers.Const) == Modifiers.Const && !Type.IsBuiltinType && !Type.IsPointer)
                ResolveContext.Report.Error(3, Location, "Only builtin types and pointers can have constant value");
            else if ((mods & Modifiers.Const) == Modifiers.Const && vadef.expr == null)
                ResolveContext.Report.Error(4, Location, "Constant fields must be initialized");

            if ((vadef.expr is ArrayConstant) && ArraySize != 0)
                ResolveContext.Report.Error(49, Location, "Array value cannot be used without the array specifier,  ex : (type k[] = 65a;)");
            // emit init priority to string
            if (Type.Equals(BuiltinTypeSpec.String) && vadef.expr != null && vadef.expr is ConstantExpression) // conert string to const
                vadef.expr = ConstantExpression.CreateConstantFromValue(BuiltinTypeSpec.String, ((ConstantExpression)(vadef.expr)).GetValue(), vadef.expr.Location);

            else if (Type.IsPointer && vadef.expr != null && vadef.expr is ConstantExpression) // convert constant to uint (pointers)
            {
                if (vadef.expr is StringConstant)
                {
                    ResolveContext.Report.Error(5, Location, "Cannot convert string to " + Type.ToString());
                    return;
                }
                if (!(vadef.expr is ArrayConstant))
                    vadef.expr = ConstantExpression.CreateConstantFromValue(BuiltinTypeSpec.UInt, ((ConstantExpression)(vadef.expr)).GetValue(), vadef.expr.Location);
            }
            else if (Type.IsBuiltinType && vadef.expr != null && vadef.expr is ConstantExpression) // convert constant to type (builtin)
            {
                if (vadef.expr is StringConstant)
                {
                    ResolveContext.Report.Error(5, Location, "Cannot convert string to " + Type.ToString());
                    return;
                }
                if (!(vadef.expr is ArrayConstant))
                    vadef.expr = ConstantExpression.CreateConstantFromValue(Type, ((ConstantExpression)(vadef.expr)).GetValue(), vadef.expr.Location);
            }




        }
        void ResolveLocalVariable(ResolveContext rc, VariableDefinition vadef)
        {
         

            vadef.FieldOrLocal = new VarSpec(rc.CurrentNamespace, vadef._id.Name, rc.CurrentMethod, Type, loc, rc.Resolver.KnownLocalVars.Count, mods);
            ((VarSpec)vadef.FieldOrLocal).Initialized = (vadef.expr != null);

           
            vadef.IsAssigned = vadef.expr != null;
            if (rc.KnowVar((VarSpec)vadef.FieldOrLocal))
                vadef.FlowVarIndex = rc.Resolver.KnownLocalVars.Count - 1;

            else
                ResolveContext.Report.Error(0, Location, vadef.Name + " is already defined in the current context");

            if (Type.IsForeignType || Type.IsArray)
                vadef.IsAssigned = true;
        }

        void ResolveStructMember(ResolveContext rc, VariableDefinition vadef)
        {
       


            vadef.Member = new TypeMemberSpec(rc.CurrentNamespace, vadef._id.Name, rc.CurrentType, Type, loc, 0);

            // value
            if (vadef.expr is ConstantExpression)
                ResolveContext.Report.Error(6, Location, "Struct and Union members cannot have initial values");
            // modifiers
            if (mods != Modifiers.NoModifier)
                ResolveContext.Report.Error(7, Location, "Struct and Union members cannot have modifiers");

        }
       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _vadef.Resolve(rc);
            if (_valist != null)
                ok &= _valist.Resolve(rc);
            if (_mod != null)
                ok &= _mod.Resolve(rc);

            ok &= _stype.Resolve(rc);
            return ok;
        }
         public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.IsInVarDeclaration = true;
            Members = new List<TypeMemberSpec>();

            if (_ext != null)
                _ext = (FunctionExtensionDefinition)_ext.DoResolve(rc);


            _vadef = (VariableDefinition)_vadef.DoResolve(rc);
          
            if (ArraySize > 0 && _vadef.expr != null)
                ResolveContext.Report.Error(48, Location, "Fixed size arrays cannot have initial values");

            // resolve valist
            if (_valist != null)
            {
                _valist.IsAbstract = IsAbstract;
                _valist = (VariableListDefinition)_valist.DoResolve(rc);
            }
            // global
            if (_vadef.expr != null && IsAbstract)
                ResolveContext.Report.Error(0, Location, "Global fields cannot have values");
            // modifiers
            if (_mod != null)
                _mod = (Modifier)_mod.DoResolve(rc);
            if (_mod != null)
                mods = _mod.ModifierList;
            else mods = Modifiers.NoModifier;

            // type
            _stype = (TypeIdentifier)_stype.DoResolve(rc);
            this.Type = _stype.Type;

            if (ArraySize > -1 && Type.IsForeignType && !Type.IsPointer)
                ResolveContext.Report.Error(52, Location, "Only builtin type arrays are allowed");

            ConvertConstant(rc, _vadef);
            VariableListDefinition val = _valist;
            while (val != null)
            {
                if (val._vardef._vardef != null)
                    ConvertConstant(rc, val._vardef._vardef);



                val = val._nextvars;
            }
           if(_vadef.expr != null && !(_vadef.expr is ConstantExpression) && !TypeChecker.CompatibleTypes(Type,_vadef.expr.Type))
               ResolveContext.Report.Error(35, Location, "Source and target must have same types");
            rc.IsInVarDeclaration = false;
            return this;
        }
      
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            fc.AddNew(_vadef.FieldOrLocal);
            if (_vadef.IsAssigned)
                fc.MarkAsAssigned(_vadef.FieldOrLocal);


            VariableListDefinition val = _valist;
            while (val != null)
            {
                fc.AddNew(val._vardef._vardef.FieldOrLocal);
                if (val._vardef._vardef != null && val._vardef._vardef.IsAssigned)
                    fc.MarkAsAssigned(val._vardef._vardef.FieldOrLocal);

                val = val._nextvars;
            }
            return base.DoFlowAnalysis(fc);
        }

        public bool EmitLocalVariable(EmitContext ec, VariableDefinition vadef)
        {
            VarSpec v = (VarSpec)vadef.FieldOrLocal;
            if (ArraySize <= 0)
            {
                // handle const
                if (vadef.expr != null)
                {

                    ec.EmitComment("Var decl assign '" + v.Name + "' @BP" + v.StackIdx);
                    // push const
                    vadef.expr.EmitToStack(ec);
                    v.EmitFromStack(ec);
                }
                /*   else if (_vadef.expr == null && Type.IsBuiltinType)
                   {


                       ec.EmitComment("Var decl assign '" + v.Name + "' @BP" + v.StackIdx);
                       // push const
                       ec.EmitPush((ushort)0);
                       v.EmitFromStack(ec);

                   }*/


            }

            return true;
        }
        public bool EmitField(EmitContext ec, VariableDefinition vadef)
        {
            FieldSpec f = (FieldSpec)vadef.FieldOrLocal;
            if (ArraySize <= 0)
            {
                if (vadef.expr == null && Type.IsForeignType)
                    ec.EmitData(new DataMember(f.Signature.ToString(), new byte[f.MemberType.Size]), f);
                //   ec.AddInstanceOfStruct(FieldOrLocal.Signature.ToString(), f.MemberType);
                // assign struct
                else if (Type.IsBuiltinType)
                {
                    if (vadef.expr != null && vadef.expr is ConstantExpression)
                    {
                        object val = ((ConstantExpression)vadef.expr).GetValue();
                        if (f.MemberType.Equals(BuiltinTypeSpec.String))
                        {
                            if (((mods & Modifiers.Const) == Modifiers.Const))
                                ec.EmitDataWithConv(f.Signature.ToString(), val, f, ((mods & Modifiers.Const) == Modifiers.Const));
                            else
                            {
                                string datasig = f.Signature.ToString() + "_data_value";
                                ec.EmitDataWithConv(datasig, val, f, ((mods & Modifiers.Const) == Modifiers.Const));
                                ec.EmitDataWithConv(f.Signature.ToString(), f, datasig);
                            }
                            // ec.ag.InitInstructions.Add(new Mov() { SourceRef = ElementReference.New(datasig), DestinationRef = ElementReference.New(f.Signature.ToString()), DestinationIsIndirect = true, Size = 16 });

                        }
                        else
                        {
                            ec.EmitDataWithConv(f.Signature.ToString(), val, f, ((mods & Modifiers.Const) == Modifiers.Const));
                        }
                    }
                    else if (f.MemberType.Equals( BuiltinTypeSpec.String))
                    {
                        string datasig = f.Signature.ToString() + "_" + DateTime.Now.ToFileTime().ToString();
                        ec.EmitDataWithConv(datasig, "", f, ((mods & Modifiers.Const) == Modifiers.Const));
                        ec.EmitDataWithConv(f.Signature.ToString(), new byte[2], f, ((mods & Modifiers.Const) == Modifiers.Const));
                        ec.ag.InitInstructions.Add(new Mov() { SourceRef = ElementReference.New(datasig), DestinationRef = ElementReference.New(f.Signature.ToString()), DestinationIsIndirect = true, Size = 16 });

                    }
                    else ec.EmitDataWithConv(f.Signature.ToString(), new byte[f.MemberType.Size], f, ((mods & Modifiers.Const) == Modifiers.Const));

                }
            }

            else ec.EmitData(new DataMember(f.Signature.ToString(), new byte[f.MemberType.GetSize(f.MemberType)]), f);
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            VariableListDefinition val = _valist;
            if ((mods & Modifiers.Extern) == Modifiers.Extern)
                ec.DefineGlobal(_vadef.FieldOrLocal);
            else if (IsAbstract)
            {
                ec.DefineExtern(_vadef.FieldOrLocal);

                while (val != null)
                {
                    if (val._vardef != null && val._vardef._vardef != null)
                        ec.DefineExtern(val._vardef._vardef.FieldOrLocal);
                    val = val._nextvars;
                }
                return true;
            }


            if (_vadef.FieldOrLocal is VarSpec) // Local Variable
                EmitLocalVariable(ec, _vadef);
            else if (_vadef.FieldOrLocal is FieldSpec) // Global var
                EmitField(ec, _vadef);

            // emit next declarations
            val = _valist;
            while (val != null)
            {
                if (val._vardef != null && val._vardef._vardef != null)
                {
                    if (val._vardef._vardef.FieldOrLocal is VarSpec) // Local Variable
                        EmitLocalVariable(ec, val._vardef._vardef);
                    else if (val._vardef._vardef.FieldOrLocal is FieldSpec) // Global var
                        EmitField(ec, val._vardef._vardef);
                }
                val = val._nextvars;
            }



            return true;
        }

      
    }
}
