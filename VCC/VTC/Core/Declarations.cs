using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
 


    // RESOLVED


    public class VariableDeclaration : Declaration
    {
        public int ArraySize { get; set; }
        public Modifiers mods;
     
        public MemberSpec FieldOrLocal { get; set; }
        public List<TypeMemberSpec> Members { get; set; }

        Modifier _mod;
        TypeIdentifier _stype;
        VariableDefinition _vadef;
        VariableListDefinition _valist;
        [Rule(@"<Var Decl>     ::= <Mod> <Type> <Var> <Var List>  ~';'")]
        public VariableDeclaration(Modifier mod, TypeIdentifier type, VariableDefinition var, VariableListDefinition valist)
        {
            _mod = mod;
            _stype = type;
            _vadef = var;
            _valist = valist;


        }
        [Rule(@"<Var Decl>     ::=  <Type> <Var> <Var List>  ~';'")]
        public VariableDeclaration(TypeIdentifier type, VariableDefinition var, VariableListDefinition valist)
        {
            _mod = null;
            _stype = type;
            _vadef = var;
            _valist = valist;


        }
        [Rule(@"<Var Decl>     ::= <Mod>        <Var> <Var List> ~';'")]
        public VariableDeclaration(Modifier mod, VariableDefinition var, VariableListDefinition valist)
        {
            _mod = mod;
            _stype = null;
            _vadef = var;
            _valist = valist;


        }

        void ResolveField(ResolveContext rc)
        {
            if (ArraySize > 0)
                Type = new ArrayTypeSpec(Type.NS, Type, ArraySize);
            else if (ArraySize == 0)
                Type = new PointerTypeSpec(Type.NS, Type);
            FieldOrLocal = new FieldSpec(rc.CurrentNamespace, _vadef._id.Name, mods, Type, loc);
            // Childs

            VariableListDefinition c = _valist;
            while (c != null)
            {
                rc.KnowField(new FieldSpec(rc.CurrentNamespace, c._vardef.Name, mods, Type, loc));
                c = _valist._nextvars;
            }
            rc.KnowField((FieldSpec)FieldOrLocal);

            // initial value
            if (!Type.IsBuiltinType && !Type.IsPointer && _vadef.expr != null && _vadef.expr is ConstantExpression)
                ResolveContext.Report.Error(2,Location,"Only builtin types and pointers can have initial value");
            // const
            if((mods & Modifiers.Const ) == Modifiers.Const && !Type.IsBuiltinType  && !Type.IsPointer)
                ResolveContext.Report.Error(3,Location,"Only builtin types and pointers can have constant value");
            else if ((mods & Modifiers.Const) == Modifiers.Const && _vadef.expr == null)
                ResolveContext.Report.Error(4,Location,"Constant fields must be initialized");

            if ((_vadef.expr is ArrayConstant) && ArraySize != 0)
                ResolveContext.Report.Error(49, Location, "Array value cannot be used without the array specifier,  ex : (type k[] = 65a;)");
            // emit init priority to string
             if(Type == BuiltinTypeSpec.String && _vadef.expr != null && _vadef.expr is ConstantExpression) // conert string to const
                 _vadef.expr = ConstantExpression.CreateConstantFromValue(BuiltinTypeSpec.String, ((ConstantExpression)(_vadef.expr)).GetValue(), _vadef.expr.Location);

             else if (Type.IsPointer && _vadef.expr != null && _vadef.expr is ConstantExpression) // convert constant to uint (pointers)
             {
                 if (_vadef.expr is StringConstant)
                 {
                     ResolveContext.Report.Error(5, Location, "Cannot convert string to " + Type.ToString());
                     return;
                 }
                 if (!(_vadef.expr is ArrayConstant))
                 _vadef.expr = ConstantExpression.CreateConstantFromValue(BuiltinTypeSpec.UInt, ((ConstantExpression)(_vadef.expr)).GetValue(), _vadef.expr.Location);
             }
             else if (Type.IsBuiltinType && _vadef.expr != null && _vadef.expr is ConstantExpression) // convert constant to type (builtin)
             {
                 if (_vadef.expr is StringConstant)
                 {
                     ResolveContext.Report.Error(5, Location, "Cannot convert string to " + Type.ToString());
                     return;
                 }
                 if (!(_vadef.expr is ArrayConstant))
                 _vadef.expr = ConstantExpression.CreateConstantFromValue(Type, ((ConstantExpression)(_vadef.expr)).GetValue(), _vadef.expr.Location);
             }
            

    

        }
        void ResolveLocalVariable(ResolveContext rc)
        {
            if (ArraySize > 0)
                Type = new ArrayTypeSpec(Type.NS, Type, ArraySize);
            else if(ArraySize == 0)
                Type = new PointerTypeSpec(Type.NS, Type);

            FieldOrLocal = new VarSpec(rc.CurrentNamespace, _vadef._id.Name, rc.CurrentMethod, Type, loc,mods);
            ((VarSpec)FieldOrLocal).Initialized = (_vadef.expr != null);
            // Childs
            VariableListDefinition c = _valist;
            while (c != null)
            {
                rc.KnowVar(new VarSpec(rc.CurrentNamespace, c._vardef._vardef._id.Name, rc.CurrentMethod, Type, loc, mods));
                c = c._nextvars;
            }
            rc.KnowVar((VarSpec)FieldOrLocal);
        }
   
        void ResolveStructMember(ResolveContext rc)
        {
            if (ArraySize > 0)
                Type = new ArrayTypeSpec(Type.NS, Type, ArraySize);
            else if (ArraySize == 0)
                Type = new PointerTypeSpec(Type.NS, Type);
            Members = new List<TypeMemberSpec>();

            // Childs

            VariableListDefinition c = _valist;
            while (c != null)
            {
                Members.Add(new TypeMemberSpec(rc.CurrentNamespace, c._vardef.Name, rc.CurrentType, Type, loc, 0));
                c = _valist._nextvars;
            }
            Members.Add(new TypeMemberSpec(rc.CurrentNamespace, _vadef._id.Name, rc.CurrentType, Type, loc, 0));
         
            // value
            if (_vadef.expr is ConstantExpression)
                ResolveContext.Report.Error(6,Location,"Struct and Union members cannot have initial values");
            // modifiers
            if(mods != Modifiers.NoModifier)
                ResolveContext.Report.Error(7, Location, "Struct and Union members cannot have modifiers");

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.IsInVarDeclaration = true;
            _vadef = (VariableDefinition)_vadef.DoResolve(rc);
            ArraySize = _vadef.ArraySize;
            if (ArraySize > 0 && _vadef.expr != null)
                ResolveContext.Report.Error(48, Location, "Fixed size arrays cannot have initial values");
      
            if (_valist != null)
                _valist = (VariableListDefinition)_valist.DoResolve(rc);

            if (_mod != null)
                _mod = (Modifier)_mod.DoResolve(rc);
            _stype = (TypeIdentifier)_stype.DoResolve(rc);
            this.Type = _stype.Type;
            if (ArraySize > -1 && Type.IsForeignType && !Type.IsPointer)
                ResolveContext.Report.Error(52, Location, "Only builtin type arrays are allowed"); 
            if (_mod != null)
                mods = _mod.ModifierList;
            else mods = Modifiers.NoModifier;
            bool conv = false;
            // convert implicitly
            if (_vadef.expr is ConstantExpression)
                _vadef.expr = ((ConstantExpression)_vadef.expr).ConvertImplicitly(rc, Type, ref conv);
            else if (_vadef.expr is CastOperator)
                _vadef.expr = ((CastOperator)_vadef.expr).Target;

            if (rc.IsInGlobal() && !rc.IsInTypeDef && !rc.IsInEnum && !rc.IsInStruct && !rc.IsInUnion) // field def
                ResolveField(rc);
            else if (!rc.IsInTypeDef && !rc.IsInEnum && !rc.IsInStruct && !rc.IsInUnion) // local var definition
                ResolveLocalVariable(rc);
            else if (rc.IsInStruct || rc.IsInUnion) // struct, union member def
                ResolveStructMember(rc);

            else
                ResolveContext.Report.Error(8, Location, "Unresolved variable declaration");


            rc.IsInVarDeclaration = false;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
         bool ok =   _vadef.Resolve(rc);
            if (_valist != null)
             ok &=   _valist.Resolve(rc);
            if (_mod != null)
              ok &=  _mod.Resolve(rc);

            ok &= _stype.Resolve(rc);
            return ok;
        }

       public bool EmitLocalVariable(EmitContext ec)
        {
            VarSpec v = (VarSpec)FieldOrLocal;
            if (ArraySize <= 0)
            {
                // handle const
                if (_vadef.expr != null)
                {

                    ec.EmitComment("Var decl assign '" + v.Name + "' @BP" + v.StackIdx);
                    // push const
                    _vadef.expr.EmitToStack(ec);
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
       public bool EmitField(EmitContext ec)
       {
           FieldSpec f = (FieldSpec)FieldOrLocal; 
           if (ArraySize <= 0)
           {
               if (_vadef.expr == null && Type.IsForeignType)
                   ec.EmitData(new DataMember(f.Signature.ToString(), new byte[f.MemberType.Size]), f);
               //   ec.AddInstanceOfStruct(FieldOrLocal.Signature.ToString(), f.MemberType);
               // assign struct
               else if (Type.IsBuiltinType)
               {
                   if (_vadef.expr != null && _vadef.expr is ConstantExpression)
                   {
                       object val = ((ConstantExpression)_vadef.expr).GetValue();
                       ec.EmitDataWithConv(f.Signature.ToString(), val, f, ((mods & Modifiers.Const) == Modifiers.Const));

                   }
                   else if(f.MemberType == BuiltinTypeSpec.String)
                       ec.EmitDataWithConv(f.Signature.ToString(), "", f, ((mods & Modifiers.Const) == Modifiers.Const));

                   else ec.EmitDataWithConv(f.Signature.ToString(), new byte[f.MemberType.Size], f, ((mods & Modifiers.Const) == Modifiers.Const));

               }
           }
           
           else ec.EmitData(new DataMember(f.Signature.ToString(), new byte[ArraySize*f.MemberType.Size]), f);
           return true;
       }
       public override bool Emit(EmitContext ec)
        {
        
       
            if (FieldOrLocal is VarSpec) // Local Variable
                EmitLocalVariable(ec);
            else if (FieldOrLocal is FieldSpec) // Global var
                EmitField(ec);


          // emit next declarations
            if (_valist != null)
                _valist.Emit(ec);

            return true;
        }
    }





    // Working Emit and Resolve
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
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '==' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '!=' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '>=' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '<=' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '>' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '<' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '+' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '*' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '-' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '/' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '%' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '^' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '&' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '|' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '<<' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '>>' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '<~' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '~>' <Func Body> ")]
        public OperatorDeclaration(Modifier mod,TypeToken type,BinaryOp oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            OpSym = oper;
        }

        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator OperatorLiteralBinary <Func Body> ")]
        public OperatorDeclaration(Modifier mod, TypeToken type, OperatorLiteralBinary oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            OpSym = new ExtendedBinaryOperator(oper.Sym, oper.Value.GetValue().ToString());
        }
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator OperatorLiteralUnary <Func Body> ")]
        public OperatorDeclaration(Modifier mod, TypeToken type, OperatorLiteralUnary oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            OpSym = new ExtendedUnaryOperator(oper.Sym,oper.Value.GetValue().ToString());
        }

        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '++' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '--' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '¤' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '??' <Func Body> ")]
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '~' <Func Body> ")]
        public OperatorDeclaration(Modifier mod, TypeToken type, UnaryOp oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            OpSym = oper;
        }
        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator '[]' <Func Body> ")]
        public OperatorDeclaration(Modifier mod, TypeToken type, SimpleToken oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            OpSym = oper;
        }

        [Rule(@"<Oper Decl> ::= <Mod> ~override <Type> ~operator <Type> <Func Body> ")]
        public OperatorDeclaration(Modifier mod, TypeToken type, TypeToken oper, FunctionBodyDefinition fbd)
        {
            _mtype = type;
            _fbd = fbd;
            _mod = mod;
            _casttype = oper;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _fbd = (FunctionBodyDefinition)_fbd.DoResolve(rc);
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

                method = rc.Resolver.TryResolveMethod(OpName);
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
                else if(!oper.IsLogic)
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
             

                method = rc.Resolver.TryResolveMethod(OpName);
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
                 else if(!oper.IsLogic)
                    if (!_mtype.Type.Equals(_fbd.ParamTypes[0]))
                    ResolveContext.Report.Error(45, Location, "user defined unary operators must have same return and parameters type");


            }
            else  if (_casttype == null && OpSym is BinaryOp)
            {
                BinaryOp bop = OpSym as BinaryOp;
                OpName = _type.Type.NormalizedName + "_" + bop.Operator.ToString();

                method = rc.Resolver.TryResolveMethod(OpName);
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
            else if (_casttype == null && OpSym is UnaryOp)
            {
                UnaryOp uop = OpSym as UnaryOp;
                OpName = _type.Type.NormalizedName + "_" + uop.Operator.ToString();

                method = rc.Resolver.TryResolveMethod(OpName);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
                if (_fbd.ParamTypes.Count!= 1)
                    ResolveContext.Report.Error(45, Location, "Unary operators must have 1 parameters with same return type");
          
              
                    // match types
                    if (uop.Operator != UnaryOperator.ParityTest && uop.Operator != UnaryOperator.ZeroTest && !_mtype.Type.Equals(_fbd.ParamTypes[0]) )
                        ResolveContext.Report.Error(45, Location, "Unary operators must have same return and parameters type");


            }
            else if (_casttype == null && OpSym is SimpleToken)
            {

                OpName = _type.Type.NormalizedName + "_IndexedAccess" ;

                method = rc.Resolver.TryResolveMethod(OpName);
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
                _casttype = (TypeToken)_casttype.DoResolve(rc);
                OpName = _type.Type.NormalizedName + "_OpCast_" + _casttype.Type.NormalizedName;

                method = rc.Resolver.TryResolveMethod(OpName);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
                else if (method != null)
                    hasproto = true;
                // operator checks
                if (_fbd.ParamTypes.Count != 1)
                    ResolveContext.Report.Error(45, Location, "Cast operator must have 1 parameters with same type as source");
            
                    // match types
                if (_casttype.Type != _fbd.ParamTypes[0])
                        ResolveContext.Report.Error(45, Location, "Cast operators must have same cast type and parameter type");
                
            }
            method = new MethodSpec(rc.CurrentNamespace, OpName, mods, _mtype.Type, CallingConventions.StdCall, _fbd.ParamTypes.ToArray(), this.loc);
            method.Parameters = Params.ToList<ParameterSpec>();
            if(!hasproto)
             rc.KnowMethod(method);

            rc.CurrentMethod = method;
            if (!method.MemberType.IsBuiltinType && !method.MemberType.IsPointer)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type " + method.MemberType.ToString() + " is user-defined type.");
            if (_fbd._b != null)
                _fbd._b = (Block)_fbd._b.DoResolve(rc);
            return this;
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
        public override bool Emit(EmitContext ec)
        {
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
            ec.EmitComment("Operator: Name = " + method.Name + " IsCast = " + (_casttype != null).ToString() + " Nature = " + opt);
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });

            // allocate variables

            ushort size = 0;
            foreach (VarSpec v in ec.CurrentResolve.GetLocals())
                size += (ushort)v.MemberType.Size;

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
    }

    public class InterruptDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.NoModifier;
        public string ItName;

        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }

        ushort interrupt;
        Block _b;
 
        [Rule(@"<Inter Decl> ::= ~interrupt <CONSTANT> <Block>")]
        public InterruptDeclaration(Literal hlit,  Block b)
        {
            interrupt = ushort.Parse(hlit.Value.GetValue().ToString());
            _b = b;
            ItName = "INTERRUPT_" + interrupt.ToString("X2")+"H";
        }
 
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            ItName = "INTERRUPT_" + interrupt.ToString("X2") + "H";

            method = rc.Resolver.TryResolveMethod(ItName);
                if (method != null)
                    ResolveContext.Report.Error(9, Location, "Duplicate interrupt name, multiple interrupt definition is not allowed");

             
            
            method = new MethodSpec(rc.CurrentNamespace, ItName, mods, BuiltinTypeSpec.Void, CallingConventions.StdCall,null, this.loc);
       
            rc.KnowMethod(method);
            rc.CurrentMethod = method;
       
            if (_b != null)
                _b = (Block)_b.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;

            if (_b != null)
                ok &= _b.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {



            Label mlb = ec.DefineLabel(method.Signature.ToString());
            mlb.Method = true;
            ec.MarkLabel(mlb);
            ec.EmitComment("Interrupt: Number = " + interrupt.ToString() );
            // save flags
            ec.EmitComment("save flags");
            ec.EmitInstruction(new Pushad());
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });

            // allocate variables

            ushort size = 0;
            foreach (VarSpec v in ec.CurrentResolve.GetLocals())
                size += (ushort)v.MemberType.Size;

            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });
    
          
            ec.EmitComment("Block");
            // Emit Code
            if (_b != null)
                _b.Emit(ec);

            ec.EmitComment("return label");
            // Return Label
            ec.MarkLabel(ec.DefineLabel(method.Signature + "_ret"));
            // Destroy Stack Frame
            ec.EmitComment("destroy stackframe");
            ec.EmitInstruction(new Leave());
          
            // restore flags
            ec.EmitComment("restore flags");
            ec.EmitInstruction(new Popad());
            // ret
            ec.EmitInstruction(new IRET() );

            ec.EmitINT(interrupt, mlb);
            return true;
        }
    }

    public class MethodDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.Private;
        CallingConventions ccv = CallingConventions.StdCall;
        CallingConventionsHandler ccvh;
        FunctionBodyDefinition _fbd;
        FunctionSpecifier _spec;
        Specifiers specs;
        public List<ParameterSpec> Parameters;

        MethodIdentifier _id; 
        [Rule(@"<Func Decl> ::= <Func ID> <Func Body>")]
        public MethodDeclaration(MethodIdentifier id, FunctionBodyDefinition fbd)
        {
            _name = id.Id;
            _id = id;
            _spec = null;
            _fbd = fbd;
        }

        [Rule(@"<Func Decl> ::= <Func Spec> <Func ID> <Func Body>")]
        public MethodDeclaration(FunctionSpecifier spec,MethodIdentifier id, FunctionBodyDefinition fbd)
        {
            _name = id.Id;
            _id = id;
            _spec = spec;
            _fbd = fbd;
        }


   
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            ccvh = new CallingConventionsHandler();
            _id = (MethodIdentifier)_id.DoResolve(rc);
            ccv = _id.CV;
            mods = _id.Mods;
            _fbd = (FunctionBodyDefinition)_fbd.DoResolve(rc);
            Parameters = _fbd.Parameters;
            if (_spec != null)
            {
                _spec = (FunctionSpecifier)_spec.DoResolve(rc);
                specs = _spec.Specs;
            }
            else specs = Specifiers.NoSpec;
            base._type = _id.TType;



            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods, _id.TType.Type, ccv, _fbd.ParamTypes.ToArray(), this.loc);

            // Calling Convention
            ccvh.SetParametersIndex(ref Parameters, ccv);
            if(ccv == CallingConventions.FastCall)
                 ccvh.ReserveFastCall(rc, Parameters);
            method.Parameters = Parameters;
         
                 if(ccv == CallingConventions.FastCall && Parameters.Count >= 1 && Parameters[0].MemberType.Size > 2)
                     ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1");
                 else if (ccv == CallingConventions.FastCall && Parameters.Count >= 2 && (Parameters[0].MemberType.Size > 2 || Parameters[1].MemberType.Size > 2))
                     ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1 or 2");


            MethodSpec m = rc.Resolver.TryResolveMethod(method.Signature.ToString());
            if (m != null && (m.Modifiers & Modifiers.Prototype) == 0)
                ResolveContext.Report.Error(9, Location, "Duplicate method signature"); 
            else if(m == null)
                 rc.KnowMethod(method);
            // extension
            if (_fbd._ext != null)
            {
                if(_fbd._ext.Static && _fbd.ParamTypes.Count > 0&& _fbd.ParamTypes[0] != _fbd._ext.ExtendedType)
                    ResolveContext.Report.Error(45, Location, "non static method extensions must have first parameter with same extended type.");
                else  if (!rc.Extend(_fbd._ext.ExtendedType, method, _fbd._ext.Static))
                    ResolveContext.Report.Error(45, Location, "Another method with same signature has already extended this type.");
            }
            rc.CurrentMethod = method;
            if (specs == Specifiers.Isolated && method.MemberType != BuiltinTypeSpec.Void)
                ResolveContext.Report.Error(45, Location, "only void methods can be isolated.");

            if (!method.MemberType.IsBuiltinType)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type "+method.MemberType.ToString() + " is user-defined type.");
            if (_fbd._b != null)
                _fbd._b = (Block)_fbd._b.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _id.Resolve(rc);

            if (_fbd._pdef != null)
                ok &= _fbd._pdef.Resolve(rc);
            if (_fbd._b != null)
                ok &= _fbd._b.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            
            if (specs == Specifiers.Entry)
                ec.SetEntry(method.Signature.ToString());

        
            Label mlb = ec.DefineLabel(method.Signature.ToString());
            mlb.Method = true;
            ec.MarkLabel(mlb);
            ec.EmitComment("Method: Name = " + method.Name + ", EntryPoint = " + (specs == Specifiers.Entry));
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });
            if (specs == Specifiers.Isolated)
                ec.EmitInstruction(new Pushad());
            // allocate variables

            ushort size = 0;
            List<VarSpec> locals = ec.CurrentResolve.GetLocals();
            foreach (VarSpec v in locals)
                size += (ushort)v.MemberType.Size;

            // fast call
            if (ccv == CallingConventions.FastCall)
            {
                if (Parameters.Count >= 2)
                {
                    size += 4;
                    ccvh.EmitFastCall(ec, 2);
                }
                else if (Parameters.Count == 1)
                {
                    size += 2;
                    ccvh.EmitFastCall(ec, 1);
                }
            }
        
            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });
            //EMit params
            if (Parameters.Count > 0)
            {
                ec.EmitComment("Parameters Definitions");
                foreach (ParameterSpec par in Parameters)
                    ec.EmitComment("Parameter " + par.Name + " @BP" + par.StackIdx);

            }
            if (locals.Count > 0)
            {   ec.EmitComment("Local Vars Definitions");
                     foreach (VarSpec v in locals)
                         ec.EmitComment("Parameter " + v.Name + " @BP" + v.StackIdx);
            }
            ec.EmitComment("Block");
            // Emit Code
            if (_fbd._b != null)
                _fbd._b.Emit(ec);

            ec.EmitComment("return label");
            // Return Label
            ec.MarkLabel(ec.DefineLabel(method.Signature + "_ret"));
            // entry infinite loop
            if (specs == Specifiers.Entry)
            {
                ec.EmitComment("entry infinite loop");
                ec.EmitInstruction(new Jump() { DestinationLabel = method.Signature + "_ret" });
            }
            // Destroy Stack Frame
            if (specs == Specifiers.Isolated)
                ec.EmitInstruction(new Popad());
            
            ec.EmitComment("destroy stackframe");
            ec.EmitInstruction(new Leave());       
            // ret
            ccvh.EmitDecl(ec, ref Parameters, ccv);
            return true;
        }
    }


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
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '==' <Type> ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '!=' <Type> ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '>=' <Type> ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '<=' <Type> ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '>' <Type> ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '<' <Type> ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod,TypeToken type, BinaryOp oper, TypeToken cmp)
        {
            _mtype = type;
            _mod = mod;
            _comptype = cmp;
            OpSym = oper;
        }
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '¤' <Type> ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '??' <Type> ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, UnaryOp oper, TypeToken cmp)
        {
            _mtype = type;
            _mod = mod;
            _comptype = cmp;
            OpSym = oper;
        }
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '+' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '*' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '-' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '/' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '%' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '^' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '&' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '|' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '<<' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '>>' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '<~' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '~>' ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, BinaryOp oper)
        {
            _mtype = type;
            _mod = mod;

            OpSym = oper;
        }

        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '++' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '--' ~';' ")]
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '~' ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, UnaryOp oper)
        {
            _mtype = type;

            _mod = mod;
            OpSym = oper;
        }
    
        // Operator Userdef
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator OperatorLiteralUnary ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, OperatorLiteralUnary oper)
        {
            _mtype = type;

            _mod = mod;
            OpSym = new ExtendedUnaryOperator(oper.Sym, oper.Value.GetValue().ToString());
        }

        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator OperatorLiteralBinary ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, OperatorLiteralBinary oper)
        {
            _mtype = type;
            _mod = mod;

            OpSym = new ExtendedBinaryOperator(oper.Sym, oper.Value.GetValue().ToString());
        }
        // Operator userdef compare
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator OperatorLiteralBinary <Type> ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, OperatorLiteralBinary oper,TypeToken cmp)
        {
            _mtype = type;
            _mod = mod;
            _comptype = cmp;
            OpSym = new ExtendedBinaryOperator(oper.Sym, oper.Value.GetValue().ToString());
        }

        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator OperatorLiteralUnary <Type> ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, OperatorLiteralUnary oper, TypeToken cmp)
        {
            _mtype = type;
            _comptype = cmp;
            _mod = mod;
            OpSym = new ExtendedUnaryOperator(oper.Sym, oper.Value.GetValue().ToString());
        }

        // Index Operator
        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator '[]' ~';' ")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, SimpleToken oper)
        {
            _mtype = type;

            _mod = mod;
            OpSym = oper;
        }

        [Rule(@"<Oper Proto> ::= <Mod> ~override <Type> ~operator <Type> ~';'")]
        public OperatorPrototypeDeclaration(Modifier mod, TypeToken type, TypeToken oper)
        {
            _mtype = type;
            _mod = mod;
            _casttype = oper;
        }
        public void DefaultParams( MethodSpec host,params TypeSpec[] par)
        {
            int i =0;
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
               
                if(oper.IsLogic && _comptype == null)
                    ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");

                OpName = _type.Type.NormalizedName + "_" + oper.Name;

                method = rc.Resolver.TryResolveMethod(OpName);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");

                if(_mtype.Type != BuiltinTypeSpec.Bool && _comptype != null)
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

                method = rc.Resolver.TryResolveMethod(OpName);
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

                method = rc.Resolver.TryResolveMethod(OpName);
                if (method != null && (method.Modifiers & Modifiers.Prototype) ==  Modifiers.Prototype)
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
            else if (_casttype == null && OpSym is UnaryOp)
            {
                UnaryOp uop = OpSym as UnaryOp;
                OpName = _type.Type.NormalizedName + "_" + uop.Operator.ToString();

                method = rc.Resolver.TryResolveMethod(OpName);
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

                method = rc.Resolver.TryResolveMethod(OpName);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
               
               

                // match types
                tp.Add(_mtype.Type);
                tp.Add(BuiltinTypeSpec.UInt);
            }
            else
            {
                _casttype = (TypeToken)_casttype.DoResolve(rc);
                OpName = _type.Type.NormalizedName + "_OpCast_" + _casttype.Type.NormalizedName;

                method = rc.Resolver.TryResolveMethod(OpName);
                if (method != null && (method.Modifiers & Modifiers.Prototype) == Modifiers.Prototype)
                    ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");


                tp.Add(_casttype.Type);
            }
            method = new MethodSpec(rc.CurrentNamespace, OpName, mods, _mtype.Type, CallingConventions.StdCall,tp.ToArray(), this.loc);
            DefaultParams(method, tp.ToArray());
            method.Parameters =Parameters;
         
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
    public class MethodPrototypeDeclaration : Declaration
    {
        FunctionExtensionDefinition ext;
        MethodSpec method;
        CallingConventions ccv = CallingConventions.StdCall;
        Modifiers mods = Modifiers.Private;
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

        [Rule(@" <Func Proto> ::= <Func ID> ~'(' <Types> ~')' <Func Ext> ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id, TypeIdentifierListDefinition tdl, FunctionExtensionDefinition _ext)
        {
            _id = id;
            _tdl = tdl;
            ext = _ext;
        }

   
        [Rule(@"<Func Proto> ::= <Func ID> ~'(' <Params> ~')' <Func Ext> ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id, ParameterListDefinition pal, FunctionExtensionDefinition _ext)
        {
            _id = id;
            _pal = pal;
            ext = _ext;
        }

        [Rule(@"<Func Proto> ::= <Func ID> ~'(' ~')' <Func Ext> ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id,FunctionExtensionDefinition _ext)
        {
            ext = _ext;
            _id = id;
            _pal = null;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _id = (MethodIdentifier)_id.DoResolve(rc);
         
            ccv = _id.CV;
            mods = _id.Mods;
            mods |= Modifiers.Prototype;
            base._type = _id.TType;
            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods | Modifiers.Prototype, _id.TType.Type, ccv, null ,this.loc);
            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            List<TypeSpec> tp = new List<TypeSpec>();

            if (ext != null)
                ext = (FunctionExtensionDefinition)ext.DoResolve(rc);

            if (_pal != null)
            {
                _pal.Resolve(rc);
                _pal = (ParameterListDefinition)_pal.DoResolve(rc);
                ParameterListDefinition par = _pal;
                while (par != null)
                {
                    if (par._id != null)
                    {
                        Params.Push(par._id.ParameterName);
                        tp.Add(par._id.ParameterName.MemberType);
                    }
                    par = par._nextid;
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
                        ParameterSpec p = new ParameterSpec("param_" + paid, method, par._id.Type,  par.loc,4);
                        Parameters.Add(p);
                        Params.Push(p);
                        tp.Add(p.MemberType);
                    }
                    par = par._nextid;
                    paid++;
                }
            }
            if (ccv == CallingConventions.FastCall && Parameters.Count >= 1 && Parameters[0].MemberType.Size > 2)
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1");
            else if (ccv == CallingConventions.FastCall && Parameters.Count >= 2 && (Parameters[0].MemberType.Size > 2 || Parameters[1].MemberType.Size > 2))
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1 or 2");

           
            rc.CurrentMethod = method;
          

            if (!method.MemberType.IsBuiltinType)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type " + method.MemberType.ToString() + " is user-defined type.");

            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods | Modifiers.Prototype, _id.TType.Type, ccv,tp.ToArray(), this.loc);
            method.Parameters = Parameters;
            // extension
            if (ext != null)
            {
                if (ext.Static && tp.Count > 0 && tp[0] != ext.ExtendedType)
                    ResolveContext.Report.Error(45, Location, "non static method extensions must have first parameter with same extended type.");
                else if (!rc.Extend(ext.ExtendedType, method, ext.Static))
                    ResolveContext.Report.Error(45, Location, "Another method with same signature has already extended this type.");
            }
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
    public class UnionDeclaration : Declaration
    {
        public UnionTypeSpec TypeName { get; set; }
        public int Size { get; set; }
        StructDefinition _def;
        Modifier _mod;
        bool istypedef=false;
        [Rule(@"<Union Decl>  ::= <Mod> ~union Id ~'{' <Struct Def> ~'}'  ~';' ")]
        public UnionDeclaration(Modifier mod,Identifier id, StructDefinition sdef)
        {
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
        }
        [Rule(@"<Union Decl>  ::= <Mod> ~typedef ~union  ~'{' <Struct Def> ~'}' Id ~';' ")]
        public UnionDeclaration(Modifier mod, StructDefinition sdef, Identifier id)
        {
            istypedef =true;
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            TypeName = new UnionTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), loc);
            rc.KnowType(TypeName);
            _def = (StructDefinition)_def.DoResolve(rc);
            if (_def != null)
                Size = _def.Size;
            int idx = 0;
            int i = 0;
            List<int> tobeupdated = new List<int>();
            TypeSpec ts = null;
            foreach (TypeMemberSpec m in _def.Members)
            {
                m.Index = 0;
                idx += m.MemberType.Size;
                m.MemberType.GetBase(m.MemberType, ref ts);
                if (ts == TypeName)
                    tobeupdated.Add(i);

                i++;
            }

            UnionTypeSpec NewType = new UnionTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, loc);
            NewType.Modifiers = _mod.ModifierList;
            foreach (int id in tobeupdated)
                _def.Members[id].MemberType.MakeBase(ref _def.Members[id].memberType, NewType);

            rc.UpdateType(TypeName, NewType);


        
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {


            return _def.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {

            //ec.EmitStructDef(TypeName);

            return true;
        }
    }
    public class StructDeclaration : Declaration
    {
        public StructTypeSpec TypeName { get; set; }
        public int Size { get; set; }
        StructDefinition _def;
        Modifier _mod;
        bool istypedef=false;
        [Rule(@"<Struct Decl>  ::= <Mod> ~struct Id ~'{' <Struct Def> ~'}'  ~';' ")]
        public StructDeclaration(Modifier mod,Identifier id, StructDefinition sdef)
        {
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
        }
        [Rule(@"<Struct Decl>  ::= <Mod> ~typedef ~struct  ~'{' <Struct Def> ~'}' Id ~';' ")]
        public StructDeclaration(Modifier mod, StructDefinition sdef, Identifier id)
        {
            istypedef = true;
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
        }


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);

            TypeName = new StructTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), loc);
           
            rc.KnowType(TypeName);
            _def = (StructDefinition)_def.DoResolve(rc);
            if (_def != null)
                Size = _def.Size;
            int idx = 0;
            int i=0;
            List<int> tobeupdated = new List<int>();
            TypeSpec ts=null;
            foreach (TypeMemberSpec m in _def.Members)
            {
              
                m.Index = idx;
                idx += m.MemberType.Size;
                m.MemberType.GetBase(m.MemberType,ref ts);
                if ( ts== TypeName)
                    tobeupdated.Add(i);

                  i++;
            }

            StructTypeSpec NewType = new StructTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, loc);
            NewType.Modifiers = _mod.ModifierList;
            foreach (int id in tobeupdated)
                _def.Members[id].MemberType.MakeBase(ref _def.Members[id].memberType, NewType);
   

            rc.UpdateType(TypeName, NewType);

        
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
    public class DelegateDeclaration : Declaration
    {
        public DelegateTypeSpec TypeName { get; set; }

        TypeIdentifierListDefinition _tdl;
        CallingCV _ccv;
        TypeToken _ret;
        Modifier _mod;
        [Rule(@"<Delegate Decl>  ::=  <Mod> <CallCV> ~delegate <Type> Id ~'(' <Types>  ~')' ~';' ")]
        public DelegateDeclaration(Modifier mod,CallingCV ccv,TypeToken ret,Identifier id,TypeIdentifierListDefinition tid )
        {
            _ccv = ccv;
            _name = id;
            _mod = mod;
            _tdl = tid;
            _ret = ret;
        }
        [Rule(@"<Delegate Decl>  ::=  <Mod> ~typedef <CallCV> ~delegate <Type> ~'(' <Types>  ~')' Id ~';' ")]
        public DelegateDeclaration(Modifier mod, CallingCV ccv, TypeToken ret, TypeIdentifierListDefinition tid, Identifier id)
        {
            _ccv = ccv;
            _name = id;
            _mod = mod;
            _tdl = tid;
            _ret = ret;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            TypeName = new DelegateTypeSpec(rc.CurrentNamespace, _name.Name, _ret.Type, new List<TypeSpec>(), _ccv.CallingConvention, loc);
            TypeName.Modifiers = _mod.ModifierList;
            rc.KnowType(TypeName);


            List<TypeSpec> tp = new List<TypeSpec>();
            _tdl = (TypeIdentifierListDefinition)_tdl.DoResolve(rc);
            _ret = (TypeToken)_ret.DoResolve(rc);
            if (_ccv != null)
                _ccv = (CallingCV)_ccv.DoResolve(rc);
          if (_tdl != null)
            {
                _tdl.Resolve(rc);
                _tdl = (TypeIdentifierListDefinition)_tdl.DoResolve(rc);
                TypeIdentifierListDefinition par = _tdl;
                int paid = 0;
                while (par != null)
                {
                    if (par._id != null)         
                        tp.Add(par._id.Type);
                    
                    par = par._nextid;
                    paid++;
                }
            }

          DelegateTypeSpec NT = new DelegateTypeSpec(rc.CurrentNamespace, _name.Name, _ret.Type, tp, _ccv.CallingConvention, loc);

          rc.UpdateType(TypeName, NT);

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
    public class OperatorDefinitionDeclaration : Declaration
    {
        OperatorSpec ops;
        string _name=null;
        OperatorLiteralBinary _operb;
        OperatorLiteralUnary _operu;
        Modifier _mod;
        bool islogic;
        [Rule(@"<Oper Def Decl>  ::=  <Mod> ~define Id ~operator OperatorLiteralBinary ~';' ")]
        public OperatorDefinitionDeclaration(Modifier mod, Identifier name, OperatorLiteralBinary oper)
        {
            _name = name.Name;
            _mod = mod;
            _operb = oper;
        }
        [Rule(@"<Oper Def Decl>  ::=  <Mod> ~define Id ~operator OperatorLiteralUnary ~';' ")]
        public OperatorDefinitionDeclaration(Modifier mod, Identifier name, OperatorLiteralUnary oper)
        {
            _name = name.Name;
            _mod = mod;
            _operu = oper;
        }

        // Comparison Operator
        [Rule(@"<Oper Def Decl>  ::=  <Mod> ~define bool Id ~operator OperatorLiteralBinary ~';' ")]
        public OperatorDefinitionDeclaration(Modifier mod, SimpleToken t, Identifier name, OperatorLiteralBinary oper)
        {
            islogic = true;
            _name = name.Name;
            _mod = mod;
            _operb = oper;
        }
        [Rule(@"<Oper Def Decl>  ::=  <Mod> ~define bool Id ~operator OperatorLiteralUnary ~';' ")]
        public OperatorDefinitionDeclaration(Modifier mod, SimpleToken t, Identifier name, OperatorLiteralUnary oper)
        {
            islogic = true;
            _name = name.Name;
            _mod = mod;
            _operu = oper;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            string val;
            if(_operb != null)
                val = _operb.Value.GetValue().ToString();
            else val = _operu.Value.GetValue().ToString();
            ops = new OperatorSpec(rc.CurrentNamespace, _name, val, _mod.ModifierList, loc);
            ops.IsBinary = (_operb != null);
            ops.IsLogic = islogic;
            rc.Resolver.KnowOperator(ops);
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
    public class TypeDefDeclaration : Declaration
    {

        public TypeSpec TypeName
        {
            get;
            set;

        }
        Modifier _mod;
        TypeIdentifier _typedef;
        [Rule(@"<Typedef Decl> ::= <Mod> ~typedef <Type> Id ~';'")]
        public TypeDefDeclaration(Modifier mod,TypeIdentifier type, Identifier id)
        {
            _mod = mod;
            _name = id;
            _typedef = type;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc); 
            _typedef = (TypeIdentifier)_typedef.DoResolve(rc);
            TypeName = new TypeSpec(rc.CurrentNamespace, _name.Name, _typedef.Type.Size, BuiltinTypes.Unknown, TypeFlags.TypeDef, Modifiers.NoModifier, loc, _typedef.Type);
            TypeName.Modifiers = _mod.ModifierList;
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

    public class NamespaceDeclaration : SimpleToken
    {
        public Namespace Namespace { get; set; }
        [Rule("<Namespace> ::= ~namespace Id ")]
        public NamespaceDeclaration(Identifier id)
        {
            Namespace = new Namespace(id.Name);
        }


    }
    public class ImportDeclaration : SimpleToken
    {
        public Namespace Import { get; set; }
        [Rule("<Import>   ::= ~use Id ~';'")]
        public ImportDeclaration(Identifier id)
        {
            Import = new Namespace(id.Name);
        }
    }
    public class Imports : SimpleToken
    {
        public List<Namespace> Used { get; set; }
        [Rule("<Imports>  ::= <Import> <Imports>")]
        public Imports(ImportDeclaration im, Imports imp)
        {
            Used = new List<Namespace>();
            Used.Add(im.Import);
            foreach (Namespace id in imp.Used)
                Used.Add(id);

        }
        [Rule("<Imports>  ::= <Import>")]
        public Imports(ImportDeclaration im)
        {
            Used = new List<Namespace>();
            Used.Add(im.Import);


        }
    }

    public class Global : SimpleToken
    {
        public DeclarationSequence<Declaration> Declarations { get; set; }
        public Namespace Namespace { get; set; }
        public List<Namespace> Used { get; set; }

    
        [Rule("<GLOBAL> ::= <Namespace> ~'{' <Imports> <Decls> ~'}'")]
        public Global(NamespaceDeclaration ndcl, Imports imp, DeclarationSequence<Declaration> ds)
        {
            Declarations = ds;
            Namespace = ndcl.Namespace;
            Used = imp.Used;

        }
        [Rule("<GLOBAL> ::=  <Decls> ")]
        public Global( DeclarationSequence<Declaration> ds)
        {
            Declarations = ds;
            Namespace =Namespace.Default;
            Used = new List<Namespace>();

        }
        [Rule("<GLOBAL> ::= <Namespace> ~'{' <Decls> ~'}'")]
        public Global(NamespaceDeclaration ndcl, DeclarationSequence<Declaration> ds)
        {
            Declarations = ds;
            Namespace = ndcl.Namespace;
            Used = new List<Namespace>();

        }
    }
    public class Declaration : DeclarationToken
    {
        public Declaration BaseDeclaration { get; set; }
        TypeSpec _ts;
        public TypeSpec Type {
            get
            {
                if (_ts != null && _ts.IsTypeDef)
                    return _ts.GetTypeDefBase(_ts);
                else return _ts;
            }
            set
            {
                _ts = value;
            }
        }

        protected Identifier _name;
       protected TypeToken _type;
        public bool IsTypeDef { get { return (BaseDeclaration is StructDeclaration) || (BaseDeclaration is TypeDefDeclaration) || (BaseDeclaration is EnumDeclaration); } }
        public bool IsStruct { get { return (BaseDeclaration is StructDeclaration); } }
        public bool IsUnion { get { return (BaseDeclaration is UnionDeclaration); } }
        public TypeToken TypeTok
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
        [Rule(@"<Decl>  ::= <ASM Decl>")]
        [Rule(@"<Decl>  ::= <Oper Decl>")]
        [Rule(@"<Decl>  ::= <Func Decl>")]
        [Rule(@"<Decl>  ::= <Func Proto>")]
        [Rule(@"<Decl>  ::= <Oper Proto>")]
        [Rule(@"<Decl>  ::= <Union Decl>")]
        [Rule(@"<Decl>  ::= <Struct Decl>")]
        [Rule(@"<Decl>  ::= <Enum Decl>")]
        [Rule(@"<Decl>  ::= <Var Decl>")]
        [Rule(@"<Decl>  ::= <Typedef Decl>")]
        [Rule(@"<Decl>  ::= <Extension Decl>")]
        [Rule(@"<Decl>  ::= <Preproc Decl>")]
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
        public EnumTypeSpec TypeName { get; set; }
        public int Size { get; set; }

        Modifier _mod;
        EnumDefinition _def;
        [Rule(@"<Enum Decl>    ::= <Mod> ~enum Id ~'{' <Enum Def> ~'}'  ~';'")]
        public EnumDeclaration(Modifier mod,Identifier id, EnumDefinition edef)
        {
            _mod = mod;
            _name = id;
            _def = edef;

        }
        [Rule(@"<Enum Decl>    ::= <Mod> ~typedef ~enum ~'{' <Enum Def> ~'}' Id ~';'")]
        public EnumDeclaration(Modifier mod, EnumDefinition edef, Identifier id)
        {
            _mod = mod;
            _name = id;
            _def = edef;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);

            List<ushort> UsedValues = new List<ushort>();
            List<EnumMemberSpec> mem = new List<EnumMemberSpec>();
            _def = (EnumDefinition)_def.DoResolve(rc);

            if (_def != null)
                Size = _def.Size;
            if (_def.Members.Count > 255)
                Size = 2;
            // Get Values
            foreach (EnumMemberSpec em in _def.Members)
                if (em.IsAssigned)
                {
                    if (UsedValues.Contains(em.Value))
                        ResolveContext.Report.Error(10,Location,"Each enum member must have a unique value");
                    UsedValues.Add(em.Value);
                }
            // Auto-Assign

            foreach (EnumMemberSpec em in _def.Members)
            {
                if (!em.IsAssigned)
                {
                    for (ushort v = 0; v < ushort.MaxValue; v++)
                    {
                        if (!UsedValues.Contains(v))
                        {
                            em.Value = v;
                            UsedValues.Add(v);
                            break;
                        }
                    }
                }

                mem.Add(em);
            }

            TypeName = new EnumTypeSpec(rc.CurrentNamespace, _name.Name, Size, mem, loc);
            TypeName.Modifiers = _mod.ModifierList;
            if (TypeName.Members.Count >= 65536)
                ResolveContext.Report.Error(11, Location, "Max enum values exceeded, only 65536 values are allowed");
            rc.KnowType(TypeName);
            return this;
        }
    }

    public class AsmDeclaration : Declaration
    {
        public List<string> Instructions { get; set; }
        public bool IsDefault;
        AsmInstructions _stmt;
        [Rule(@"<ASM Decl>        ::= extern ~asm ~'{' <INSTRUCTIONS>  ~'}'")]
        [Rule(@"<ASM Decl>        ::= default ~asm ~'{' <INSTRUCTIONS>  ~'}'")]
        public AsmDeclaration(SimpleToken tok,AsmInstructions stmt)
        {
            Instructions = new List<string>();
            IsDefault = tok.Name == "default";
            _stmt = stmt;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            AsmInstructions st = _stmt;
            while (st != null)
            {
                AsmInstruction ins = st.ins;
                if (ins != null)
                    Instructions.Add(ins.Value);
                st = st.nxt;
            }
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (!IsDefault)
            {
                foreach (string ins in Instructions)
                    ec.EmitInstruction(new InlineInstruction(ins));
            }
            else
            {
                foreach (string ins in Instructions)
                ec.ag.AddDefault(new InlineInstruction(ins));
            }
            return true;
        }
    }

    public class ExtendDeclaration  : Declaration
    {
        Expr _varexpr;
        TypeToken tt;
        [Rule("<Extension Decl> ::= <Value> ~extends <Type>  ~';'")]
        public ExtendDeclaration(Expr varexp, TypeToken t)
        {
            tt = t;
            _varexpr = varexp;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            tt = (TypeToken)tt.DoResolve(rc);
            _varexpr = (Expr)_varexpr.DoResolve(rc);
            if (_varexpr is VariableExpression)
            {
                VariableExpression v = _varexpr as VariableExpression;
                if (v.variable is FieldSpec)
                {
                    if (!rc.Extend(tt.Type, (FieldSpec)v.variable))
                        ResolveContext.Report.Error(0, Location, "Another field with same signature has already extended this type.");

                }
                else ResolveContext.Report.Error(0, Location, "Only fields can be extended");
            }
            else ResolveContext.Report.Error(0, Location, "Only fields can be extended");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }

    }
}
