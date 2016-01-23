using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class OperatorPrototypeDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.Public | Modifiers.Prototype;
        public string OpName;

        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }

        Modifier _mod;
        TypeToken _comptype;
        TypeToken _mtype;
        SimpleToken OpSym;
        TypeToken _casttype;

        public TypeIdentifierListDefinition _tidl;
        CastKind ckind;
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator <Operator>  <Type> ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, DefinedOperator oper, TypeToken cmp)
        {
            _mtype = type;
            _mod = mod;
            _comptype = cmp;
            if(oper.IsBinary)
                  OpSym = oper.Binary.Operator;
            else OpSym = oper.Unary.Operator;
        }
      

        // Operator Userdef
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator <Operator Def>  <Type> ~';'  ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, OperatorDefinition oper, TypeToken cmp)
        {
            _mtype = type;
            _comptype = cmp;
            _mod = mod;
            if(!oper.IsBinary)
                OpSym = new ExtendedUnaryOperator(oper.Unary.Sym, oper.Unary.Value.GetValue().ToString());
            else OpSym = new ExtendedBinaryOperator(oper.Binary.Sym, oper.Binary.Value.GetValue().ToString());

        }



        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator <Cast Kind> ~'(' <Type>  ~')' ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken t, CastKind ck, TypeToken tidl)
        {
            _casttype = tidl;
            ckind = ck;
            _mod = mod;
            _type = t;
        }


        public void DefaultParams(MethodSpec host, params TypeSpec[] par)
        {
            int i = 0;
            foreach (TypeSpec p in par)
                Parameters.Add(new ParameterSpec("Param_oper_" + i.ToString(), host, p, Location.Null, 4, Modifiers.NoModifier));


        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            mods = _mod.ModifierList | Modifiers.Prototype;
            _mtype = (TypeToken)_mtype.DoResolve(rc);
            List<TypeSpec> tp = new List<TypeSpec>();
            Parameters = new List<ParameterSpec>();

            if (_comptype != null)
                _comptype = (TypeToken)_comptype.DoResolve(rc);

            base._type = _mtype;
            bool hasproto = false;
            if (_casttype == null && OpSym is ExtendedBinaryOperator)
            {
                ExtendedBinaryOperator bop = OpSym as ExtendedBinaryOperator;
                OperatorSpec oper = rc.Resolver.TryResolveOperator(bop.SymbolName);
                if (oper == null)
                {
                    ResolveContext.Report.Error(0, Location, "Unknown operator");
                    return this;
                }

                if (oper.IsLogic && _comptype == null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                OpName = _type.Type.NormalizedName + "_" + oper.Name;

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");

                if (_mtype.Type != BuiltinTypeSpec.Bool && _comptype != null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                // operator checks
                if (_comptype != null)
                {
                    tp.Add(_comptype.Type);
                    tp.Add(_comptype.Type);
                }
                else
                {
                    tp.Add(_mtype.Type);
                    tp.Add(_mtype.Type);
                }

            }
            else if (_casttype == null && OpSym is ExtendedUnaryOperator)
            {

                ExtendedUnaryOperator uop = OpSym as ExtendedUnaryOperator;
                OperatorSpec oper = rc.Resolver.TryResolveOperator(uop.SymbolName);
                if (oper == null)
                {
                    ResolveContext.Report.Error(0, Location, "Unknown operator");
                    return this;
                }
                if (oper.IsLogic && _comptype == null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                OpName = _type.Type.NormalizedName + "_" + oper.Name;

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method == null)
                    hasproto = true;

                if (_mtype.Type != BuiltinTypeSpec.Bool && _comptype != null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");


                // match types      

                if (_comptype != null)
                    tp.Add(_comptype.Type);
                else tp.Add(_mtype.Type);



            }
            else if (_casttype == null && OpSym is BinaryOp)
            {
                BinaryOp bop = OpSym as BinaryOp;
                OpName = _type.Type.NormalizedName + "_" + bop.Operator.ToString();

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");

                // operator checks

                if (_mtype.Type != BuiltinTypeSpec.Bool && (bop.Operator & BinaryOperator.ComparisonMask) == BinaryOperator.ComparisonMask)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");
                else if ((bop.Operator & BinaryOperator.ComparisonMask) != BinaryOperator.ComparisonMask)
                {
                    // match types
                    tp.Add(_mtype.Type);
                    tp.Add(_mtype.Type);

                }
                else if ((bop.Operator & BinaryOperator.ComparisonMask) == BinaryOperator.ComparisonMask)
                {
                    tp.Add(_comptype.Type);
                    tp.Add(_comptype.Type);
                }
            }
            else if (OpSym is UnaryOp && ((OpSym as UnaryOp).Name == "new" || (OpSym as UnaryOp).Name == "delete"))
            {
                UnaryOp uop = OpSym as UnaryOp;
                OpName = "op_alloc_" + (OpSym as UnaryOp).Name;

                rc.Resolver.TryResolveMethod(OpName, ref method, new TypeSpec[1]{_comptype.Type});
                if (method != null)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
            
                // operator checks
              
                // match types
                if (!_mtype.Type.Equals(BuiltinTypeSpec.Pointer) || !_comptype.Type.Equals(BuiltinTypeSpec.UInt))
                    ResolveContext.Report.Error(45, Location, "Allocation operators must have pointer as return type and uint as parameter type");


            }
            else if (_casttype == null && OpSym is UnaryOp)
            {
                UnaryOp uop = OpSym as UnaryOp;
                OpName = _type.Type.NormalizedName + "_" + uop.Operator.ToString();

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method == null)
                    hasproto = true;

                if (_mtype.Type != BuiltinTypeSpec.Bool && _comptype != null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                // match types
                if (uop.Operator == UnaryOperator.ParityTest || uop.Operator == UnaryOperator.ZeroTest)
                    tp.Add(_comptype.Type);
                else tp.Add(_mtype.Type);


            }
            else if (_casttype == null && OpSym is SimpleToken)
            {

                OpName = _type.Type.NormalizedName + "_IndexedAccess";

                 rc.Resolver.TryResolveMethod(OpName,ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");



                // match types
                tp.Add(_mtype.Type);
                tp.Add(BuiltinTypeSpec.UInt);
            }
            else
            {
            
                _casttype = (TypeToken)_casttype.DoResolve(rc);

                OpName = "Op_" + (ckind.IsImplicit ? "implicit" : "explicit") + "_Cast_" + _mtype.Type.NormalizedName;

                rc.Resolver.TryResolveMethod(OpName, ref method, new TypeSpec[1]{_casttype.Type});
                if (method != null)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
               
                // operator checks

                if (_mtype.Type.IsForeignType && !_mtype.Type.IsPointer)
                    ResolveContext.Report.Error(45, Location, "Non builtin types cast must return it's pointer type ");

                if (_mtype.Type.Equals(_casttype.Type))
                    ResolveContext.Report.Error(45, Location, "Cast operators can't have same parameter types");


                tp.Add(_casttype.Type);
            }
            method = new MethodSpec(rc.CurrentNamespace, OpName, mods, _mtype.Type, CallingConventions.StdCall, tp.ToArray(), this.loc);
            DefaultParams(method, tp.ToArray());
            method.Parameters = Parameters;
            method.IsOperator = true;
            rc.KnowMethod(method);

            rc.CurrentMethod = method;
            if (!method.MemberType.IsBuiltinType && !method.MemberType.IsPointer)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type " + method.MemberType.ToString() + " is user-defined type.");

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;


            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.DefineExtern(method);
            return true;
        }
    }
}
