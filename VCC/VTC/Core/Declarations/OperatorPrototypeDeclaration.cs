using VTC.Base.GoldParser.Semantic;
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
        public List<ParameterSpec> Parameters;

        Modifier _mod;
        TypeToken _mtype;
        SimpleToken OpSym;
        TypeToken _casttype;

        public TypeIdentifierListDefinition _tidl;
        CastKind ckind;
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator <Operator> ~'(' <Types> ~')' ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, DefinedOperator oper, TypeIdentifierListDefinition tidl)
        {
            _mtype = type;
            _mod = mod;
            _tidl = tidl;
            if(oper.IsBinary)
                  OpSym = oper.Binary.Operator;
            else OpSym = oper.Unary.Operator;
        }
      

        // Operator Userdef
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator <Operator Def>  ~'(' <Types> ~')' ~';'  ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, OperatorDefinition oper, TypeIdentifierListDefinition tidl)
        {
            _mtype = type;
            _tidl = tidl;
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
            _mtype = t;
        }


        public void DefaultParams(MethodSpec host, params TypeSpec[] par)
        {
            int i = 0;
            foreach (TypeSpec p in par)
                Parameters.Add(new ParameterSpec(host.NS,"Param_oper_" + i.ToString(), host, p, p.Signature.Location, 4, Modifiers.NoModifier));


        }
       public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;


            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            mods = _mod.ModifierList | Modifiers.Prototype;
            if(_mtype != null)
                 _mtype = (TypeToken)_mtype.DoResolve(rc);
            List<TypeSpec> tp = new List<TypeSpec>();
            Parameters = new List<ParameterSpec>();

            if (_tidl != null)
                _tidl = (TypeIdentifierListDefinition)_tidl.DoResolve(rc);

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

                if (oper.IsLogic && _mtype == null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                OpName = _type.Type.NormalizedName + "_" + oper.Name;

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");

                if (oper.IsLogic && _mtype.Type != BuiltinTypeSpec.Bool && _tidl != null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                if(_tidl == null || _tidl.Types.Count != 2 || !_tidl.Types[0].Equals(_tidl.Types[1]))
                    ResolveContext.Report.Error(45, Location, "Binary operators must have 2 parameters with same type ");
                // operator checks
                if (_tidl != null)
                    tp.AddRange(_tidl.Types);
             

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
                if (oper.IsLogic && _mtype == null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                OpName = _type.Type.NormalizedName + "_" + oper.Name;

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method == null)
                    hasproto = true;

                if (oper.IsLogic && _mtype.Type != BuiltinTypeSpec.Bool && _tidl != null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");


                // match types      

                if (_tidl == null || _tidl.Types.Count != 1 )
                    ResolveContext.Report.Error(45, Location, "Unary operators must have 1 parameter");
                // operator checks
                if (_tidl != null)
                    tp.AddRange(_tidl.Types);



            }
            else if (_casttype == null && OpSym is BinaryOp)
            {
                BinaryOp bop = OpSym as BinaryOp;
                OpName = _type.Type.NormalizedName + "_" + bop.Operator.ToString();

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");

                // operator checks
                if (_tidl == null || _tidl.Types.Count != 2 || !_tidl.Types[0].Equals(_tidl.Types[1]))
                    ResolveContext.Report.Error(45, Location, "Binary operators must have 2 parameters with same type ");


                if (_mtype.Type != BuiltinTypeSpec.Bool && (bop.Operator & BinaryOperator.ComparisonMask) == BinaryOperator.ComparisonMask)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                if (_tidl != null)
                    tp.AddRange(_tidl.Types);
                
              
            }
            else if (OpSym is UnaryOp && ((OpSym as UnaryOp).Name == "new" || (OpSym as UnaryOp).Name == "delete"))
            {
                UnaryOp uop = OpSym as UnaryOp;
                OpName = "op_alloc_" + (OpSym as UnaryOp).Name;
                bool delete = (OpSym as UnaryOp).Name == "delete";
                if(delete)
                   rc.Resolver.TryResolveMethod(OpName, ref method, new TypeSpec[2]{BuiltinTypeSpec.Pointer, BuiltinTypeSpec.UInt});
                else
                   rc.Resolver.TryResolveMethod(OpName, ref method, new TypeSpec[1] { BuiltinTypeSpec.UInt });
            
                if (method != null)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
            
                // operator checks

                if (delete && (_tidl == null || _tidl.Types.Count != 2 || !_tidl.Types[0].Equals(BuiltinTypeSpec.Pointer) || !_tidl.Types[1].Equals(BuiltinTypeSpec.UInt) || !_mtype.Type.Equals(BuiltinTypeSpec.Bool)))
                    ResolveContext.Report.Error(45, Location, "Delete operator must have 2 parameters (pointer,uint) & returns bool ");
                else if (!delete && (_tidl == null || _tidl.Types.Count != 1 || !_tidl.Types[0].Equals(BuiltinTypeSpec.UInt) || !_mtype.Type.Equals(BuiltinTypeSpec.Pointer) ))
                    ResolveContext.Report.Error(45, Location, "New operator must have 1 parameter uint & returns pointer");
                    
                    if (_tidl != null)
                    tp.AddRange(_tidl.Types);
             

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

                if (_mtype.Type != BuiltinTypeSpec.Bool && (uop.Operator == UnaryOperator.ParityTest || uop.Operator == UnaryOperator.ZeroTest))
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                // match types
                if (_tidl == null || _tidl.Types.Count != 1)
                    ResolveContext.Report.Error(45, Location, "Unary operators must have 1 parameters");

                if (_tidl != null)
                    tp.AddRange(_tidl.Types);


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

                if (_mtype.Type is ArrayTypeSpec)
                    ResolveContext.Report.Error(45, Location, "Array types cast are unsupported");

                if (_mtype.Type.Equals(_casttype.Type))
                    ResolveContext.Report.Error(45, Location, "Cast operators can't have same parameter types");


                tp.Add(_casttype.Type);
            }
            method = new MethodSpec(rc.CurrentNamespace, OpName, mods, _mtype.Type, CallingConventions.StdCall, tp.ToArray(), this.Location);
            DefaultParams(method, tp.ToArray());
      
            CallingConventionsHandler ccvh = new CallingConventionsHandler();
            int last_param = 4;
            ccvh.SetParametersIndex(ref Parameters, CallingConventions.StdCall, ref last_param);
            method.LastParameterEndIdx = (ushort)last_param;
            method.Parameters = Parameters;
            method.IsOperator = true;
            rc.KnowMethod(method);

            rc.CurrentMethod = method;
            if (method.memberType is ArrayTypeSpec)
                ResolveContext.Report.Error(45, Location, "return type must be non array type " + method.MemberType.ToString() + " is user-defined type.");

            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            fc.AddNew(method);
            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            ec.DefineExtern(method);
            return true;
        }
    }
}
