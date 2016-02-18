using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class OperatorDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.Public;
        public string OpName;

        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }


        FunctionBodyDefinition _fbd;

        Modifier _mod;
        TypeToken _mtype;
        SimpleToken OpSym;
        TypeToken _casttype;
        CastKind ckind;
        [Rule(@"<Oper Decl> ::=<Mod> ~override <Type> ~operator <Operator> <Func Body>")]
        public OperatorDeclaration(Modifier mod, TypeToken type, DefinedOperator oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            if(oper.IsBinary)
            OpSym = oper.Binary.Operator;
            else OpSym = oper.Unary.Operator;
        }

        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type>  ~operator <Cast Kind> <Func Body> ")]
        public OperatorDeclaration(Modifier mod,TypeToken ct, CastKind ck, FunctionBodyDefinition fbd)
        {
            ckind = ck;
            _fbd = fbd;
            _mod = mod;
            _casttype = ct;

        }


        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator <Operator Def> <Func Body>")]
        public OperatorDeclaration(Modifier mod, TypeToken type, OperatorDefinition oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            if(oper.IsBinary)
                OpSym = new ExtendedBinaryOperator(oper.Binary.Sym, oper.Binary.Value.GetValue().ToString());
            else OpSym = new ExtendedUnaryOperator(oper.Unary.Sym, oper.Unary.Value.GetValue().ToString());
        }



       public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;

            if (_fbd._pdef != null)
                ok &= _fbd._pdef.Resolve(rc);
            if (_fbd._b != null)
                ok &= _fbd._b.Resolve(rc);
            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _fbd = (FunctionBodyDefinition)_fbd.DoResolve(rc);
            if(_mtype != null)
            _mtype = (TypeToken)_mtype.DoResolve(rc);
            _mod = (Modifier)_mod.DoResolve(rc);
            mods = _mod.ModifierList;
            Params = _fbd.Params;
            Parameters = _fbd.Parameters;
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

                OpName = _type.Type.NormalizedName + "_" + oper.Name;

                 rc.Resolver.TryResolveMethod(OpName,ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
                if (_fbd.ParamTypes.Count != 2)
                    ResolveContext.Report.Error(45, Location, "user defined binary operators must have 2 parameters with same type");

                // match types
                if (!_fbd.ParamTypes[0].Equals(_fbd.ParamTypes[1]))
                    ResolveContext.Report.Error(45, Location, "user defined binary operators must have same parameters type");

                if (oper.IsLogic && _mtype.Type != BuiltinTypeSpec.Bool)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");
                else if (!oper.IsLogic)
                    if (!_mtype.Type.Equals(_fbd.ParamTypes[0]) || !_mtype.Type.Equals(_fbd.ParamTypes[1]) || !_fbd.ParamTypes[0].Equals(_fbd.ParamTypes[1]))
                        ResolveContext.Report.Error(45, Location, "Non comparison operators must have same return and parameters type");
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
                OpName = _type.Type.NormalizedName + "_" + oper.Name;


                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
                if (_fbd.ParamTypes.Count != 1)
                    ResolveContext.Report.Error(45, Location, "user defined unary operators must have 1 parameters with same return type");


                // match types
                if (oper.IsLogic && _mtype.Type != BuiltinTypeSpec.Bool)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");
                else if (!oper.IsLogic)
                    if (!_mtype.Type.Equals(_fbd.ParamTypes[0]))
                        ResolveContext.Report.Error(45, Location, "user defined unary operators must have same return and parameters type");


            }
            else if (_casttype == null && OpSym is BinaryOp)
            {
                BinaryOp bop = OpSym as BinaryOp;
                OpName = _type.Type.NormalizedName + "_" + bop.Operator.ToString();

               rc.Resolver.TryResolveMethod(OpName,ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
                if (_fbd.ParamTypes.Count != 2)
                    ResolveContext.Report.Error(45, Location, "Binary operators must have 2 parameters with same type");
                if (_mtype.Type != BuiltinTypeSpec.Bool && (bop.Operator & BinaryOperator.ComparisonMask) == BinaryOperator.ComparisonMask)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");
                else if ((bop.Operator & BinaryOperator.ComparisonMask) != BinaryOperator.ComparisonMask)
                {
                    // match types
                    if (!_mtype.Type.Equals(_fbd.ParamTypes[0]) || !_mtype.Type.Equals(_fbd.ParamTypes[1]) || !_fbd.ParamTypes[0].Equals(_fbd.ParamTypes[1]))
                        ResolveContext.Report.Error(45, Location, "Non comparison operators must have same return and parameters type");
                }

            }

            else if (OpSym is UnaryOp && ((OpSym as UnaryOp).Name == "new" || (OpSym as UnaryOp).Name == "delete"))
            {
                UnaryOp uop = OpSym as UnaryOp;
                OpName = "op_alloc_" + (OpSym as UnaryOp).Name;
                bool delete = (OpSym as UnaryOp).Name == "delete";
                rc.Resolver.TryResolveMethod(OpName, ref method, _fbd.ParamTypes.ToArray());
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                

                // match types
                if (!delete && (!_mtype.Type.Equals(BuiltinTypeSpec.Pointer) || !_fbd.ParamTypes[0].Equals(BuiltinTypeSpec.UInt)))
                    ResolveContext.Report.Error(45, Location, "New allocation operator must have pointer as return type and uint as parameter type");
                else if (delete && (_fbd.ParamTypes.Count != 2 || !_mtype.Type.Equals(BuiltinTypeSpec.Bool) || !_fbd.ParamTypes[0].Equals(BuiltinTypeSpec.Pointer) || !_fbd.ParamTypes[1].Equals(BuiltinTypeSpec.UInt)))
                    ResolveContext.Report.Error(45, Location, "Delete allocation operator must have bool as return type and pointer, uint as parameter types");
            }
            else if (_casttype == null && OpSym is UnaryOp)
            {
                UnaryOp uop = OpSym as UnaryOp;
                OpName = _type.Type.NormalizedName + "_" + uop.Operator.ToString();

                rc.Resolver.TryResolveMethod(OpName, ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
                if (_fbd.ParamTypes.Count != 1)
                    ResolveContext.Report.Error(45, Location, "Unary operators must have 1 parameters with same return type");


                // match types
                if (uop.Operator != UnaryOperator.ParityTest && uop.Operator != UnaryOperator.ZeroTest && !_mtype.Type.Equals(_fbd.ParamTypes[0]))
                    ResolveContext.Report.Error(45, Location, "Unary operators must have same return and parameters type");


            }
            else if (_casttype == null && OpSym is SimpleToken)
            {

                OpName = _type.Type.NormalizedName + "_IndexedAccess";

                rc.Resolver.TryResolveMethod(OpName,ref method);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
                if (_fbd.ParamTypes.Count != 2)
                    ResolveContext.Report.Error(45, Location, "Indexed access operator must have 2 parameters 1st  same as return type, 2nd uint");


                // match types
                if (!_mtype.Type.Equals(_fbd.ParamTypes[0]) || _fbd.ParamTypes[1] != BuiltinTypeSpec.UInt)
                    ResolveContext.Report.Error(45, Location, "Indexed access operator must have same return and 1st parameter type, the 2nd parameter must be type of uint");


            }
            else
            {
                if (_fbd.ParamTypes.Count != 1)
                    ResolveContext.Report.Error(45, Location, "Cast operator must have 1 parameters (source)");
                _casttype = (TypeToken)_casttype.DoResolve(rc);

                OpName = "Op_" + (ckind.IsImplicit ? "implicit" : "explicit") + "_Cast_" + _casttype.Type.NormalizedName;

                rc.Resolver.TryResolveMethod(OpName, ref method, _fbd.ParamTypes.ToArray());
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
              
             if(_casttype.Type is ArrayTypeSpec)
                 ResolveContext.Report.Error(45, Location,"Array types cast are unsupported");
             
                if (_casttype.Type.Equals(_fbd.ParamTypes[0]))
                    ResolveContext.Report.Error(45, Location, "Cast operators can't have same parameter types");


            }
            if(_mtype != null)
             method = new MethodSpec(rc.CurrentNamespace, OpName, mods, _mtype.Type, CallingConventions.StdCall, _fbd.ParamTypes.ToArray(), this.Location);
            else
                method = new MethodSpec(rc.CurrentNamespace, OpName, mods, _casttype.Type, CallingConventions.StdCall, _fbd.ParamTypes.ToArray(), this.Location);
            List<ParameterSpec> parameters = Params.ToList<ParameterSpec>();
 
            CallingConventionsHandler ccvh = new CallingConventionsHandler();
            int last_param = 4;
            ccvh.SetParametersIndex(ref parameters, CallingConventions.StdCall, ref last_param);
            method.LastParameterEndIdx = (ushort)last_param;
            method.Parameters = parameters;
            if (!hasproto)
            {
                method.IsOperator = true;
                rc.KnowMethod(method);
            }
            rc.CurrentMethod = method;


            if (method.memberType is ArrayTypeSpec)
                ResolveContext.Report.Error(45, Location, "return type must be non array type " + method.MemberType.ToString() + " is user-defined type.");

            if (_fbd._b != null)
                _fbd._b = (Block)_fbd._b.DoResolve(rc);
            return this;
        }
     
        public override bool Emit(EmitContext ec)
        {
            if ((mods & Modifiers.Extern) == Modifiers.Extern)
                ec.DefineGlobal(method);

            string opt = "None";
            if (OpSym is UnaryOp)
                opt = "Unary";
            else if (OpSym is BinaryOp)
                opt = "Binary";
            else if (OpSym is SimpleToken)
                opt = "Indexed Access";

            Label mlb = ec.DefineLabel(method.Signature.ToString());
            mlb.Method = true;
            ec.MarkLabel(mlb);
            ec.EmitComment("Operator: Name = " + method.Name + " IsCast = " + (ckind != null).ToString() + " Nature = " + opt);
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });

            // allocate variables

            ushort size = 0;
            foreach (VarSpec v in ec.CurrentResolve.GetLocals())
                size += (ushort)(v.memberType.IsArray ? v.memberType.GetSize(v.memberType) : v.MemberType.Size);

            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });
            int cleansize = 0;
            //EMit params
            // Get Parameters Indexes
            int paramidx = 4; // Initial Stack Position
            ParameterSpec p = null;
            while (Params.Count > 0)
            {
                p = Params.Pop();
                p.StackIdx = paramidx;
                Parameters.Add(p);
                cleansize += (p.MemberType.Size == 1) ? 2 : p.MemberType.Size;
                if (p.MemberType.Size % 2 != 0)
                {
                    paramidx++;
                    cleansize++;
                }
                paramidx += (p.MemberType.Size == 1) ? 2 : p.MemberType.Size;
            }
            if (Parameters.Count > 0)
            {
                ec.EmitComment("Parameters Definitions");
                foreach (ParameterSpec par in Parameters)
                    ec.EmitComment("Parameter " + par.Name + " @BP" + par.StackIdx);

            }
            ec.EmitComment("Block");
            // Emit Code
            if (_fbd._b != null)
                _fbd._b.Emit(ec);

            ec.EmitComment("return label");
            // Return Label
            ec.MarkLabel(ec.DefineLabel(method.Signature + "_ret"));
            // Destroy Stack Frame
            ec.EmitComment("destroy stackframe");
            ec.EmitInstruction(new Leave());
            // ret

            ec.EmitInstruction(new Return() { DestinationValue = (ushort)cleansize });
            return true;
        }
     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
       
            fc.AddNew(method);


            fc.LookForUnreachableBrace = !fc.NoReturnCheck;
            fc.NoReturnCheck = false;
            FlowState fs = FlowState.Valid;
            if (_fbd != null && _fbd._b != null)
                fs = _fbd._b.DoFlowAnalysis(fc);
            else
                fs = base.DoFlowAnalysis(fc);

            if (!fs.Reachable.IsUnreachable && !fc.NoReturnCheck)
                fc.ReportNotAllCodePathsReturns(Location);

            fc.LookForUnreachableBrace = false;

            return base.DoFlowAnalysis(fc);
        }
    }
}
