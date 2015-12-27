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
                c = _valist._nextvars;
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
                ResolveContext.Report.Error(6,Location,"Struct members cannot have initial values");
            // modifiers
            if(mods != Modifiers.NoModifier)
                ResolveContext.Report.Error(7, Location, "Struct members cannot have modifiers");

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
            if (ArraySize > -1 && Type.IsStruct)
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

            if (rc.IsInGlobal() && !rc.IsInTypeDef && !rc.IsInEnum && !rc.IsInStruct) // field def
                ResolveField(rc);
            else if (!rc.IsInTypeDef && !rc.IsInEnum && !rc.IsInStruct) // local var definition
                ResolveLocalVariable(rc);
            else if (rc.IsInStruct) // struct member def
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
                if (_vadef.expr is ConstantExpression)
                {

                    ec.EmitComment("Var decl assign '" + v.Name + "' @BP" + v.StackIdx);
                    // push const
                    _vadef.expr.EmitToStack(ec);
                    v.EmitFromStack(ec);
                }
                else if (_vadef.expr == null && Type.IsBuiltinType)
                {


                    ec.EmitComment("Var decl assign '" + v.Name + "' @BP" + v.StackIdx);
                    // push const
                    ec.EmitPush((ushort)0);
                    v.EmitFromStack(ec);

                }
                else if (_vadef.expr == null && Type.IsStruct)
                    ec.EmitComment("Struct var decl '" + v.Name + "' @BP" + v.StackIdx);
            }
           
            return true;
        }
       public bool EmitField(EmitContext ec)
       {
           FieldSpec f = (FieldSpec)FieldOrLocal; 
           if (ArraySize <= 0)
           {
               if (_vadef.expr == null && Type.IsStruct)
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
                   else ec.EmitData(new DataMember(f.Signature.ToString(), new byte[f.MemberType.Size]), f);
               }
           }
           else ec.EmitData(new DataMember(f.Signature.ToString(), new byte[ArraySize]), f);
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
        Modifiers mods = Modifiers.NoModifier;
        public string OpName;
        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }

 
        ParameterListDefinition _pal;
        Block _b;
        TypeToken _mtype;
        BinaryOp OpSym;
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '==' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '!=' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '>=' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '<=' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '>' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '<' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '+' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '*' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '-' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '/' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '%' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '^' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '&' ~'(' <Params>  ~')' <Block>")]
        [Rule(@"<Oper Decl> ::= ~override <Type> ~operator '|' ~'(' <Params>  ~')' <Block>")]
        public OperatorDeclaration(TypeToken type,BinaryOp oper, ParameterListDefinition pal, Block b)
        {
            _mtype = type;
            _pal = pal;
            _b = b;
            OpSym = oper;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
           
            _mtype = (TypeToken)_mtype.DoResolve(rc);
         

          
            base._type = _mtype;
            OpName = _type.Type.Name + "_" + OpSym.Operator.ToString();

            method = rc.Resolver.TryResolveMethod(OpName);
            if (method != null)
                ResolveContext.Report.Error(9, Location, "Duplicate operator name, multiple operator overloading is not allowed");
            List<TypeSpec> tp = new List<TypeSpec>();

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
                    {
                        Params.Push(par._id.ParameterName);
                        tp.Add(par._id.ParameterName.MemberType);
                    }
                    par = par._nextid;
                }
            }
            // operator checks
            if(tp.Count != 2)
                ResolveContext.Report.Error(45, Location, "Operator must have 2 parameters with same type");
            if (_mtype.Type != BuiltinTypeSpec.Bool && (OpSym.Operator & BinaryOperator.ComparisonMask) == BinaryOperator.ComparisonMask)
                ResolveContext.Report.Error(45, Location, "Comparison operator must return bool");
            else if ((OpSym.Operator & BinaryOperator.ComparisonMask) != BinaryOperator.ComparisonMask)
            {
                // match types
                if(_mtype.Type != tp[0] || _mtype.Type != tp[1] || tp[0] != tp[1])
                    ResolveContext.Report.Error(45, Location, "Non comparison operators must have same return and parameters type");
            }
            method = new MethodSpec(rc.CurrentNamespace, OpName, mods, _mtype.Type, CallingConventions.StdCall, tp.ToArray(), this.loc);
            method.Parameters = Params.ToList<ParameterSpec>();
            rc.KnowMethod(method);
            rc.CurrentMethod = method;
            if (!method.MemberType.IsBuiltinType && !method.MemberType.IsPointer)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type " + method.MemberType.ToString() + " is user-defined type.");
            if (_b != null)
                _b = (Block)_b.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;

            if (_pal != null)
                ok &= _pal.Resolve(rc);
            if (_b != null)
                ok &= _b.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
    


            Label mlb = ec.DefineLabel(method.Signature.ToString());
            ec.MarkLabel(mlb);
            ec.EmitComment("Operator: Name = " + method.Name + " ");
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });
            // allocate variables

            uint size = 0;
            foreach (VarSpec v in ec.CurrentResolve.GetLocals())
                size += (uint)v.MemberType.Size;

            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });
            //EMit params
            // Get Parameters Indexes
            int paramidx = 4; // Initial Stack Position
            ParameterSpec p = null;
            while (Params.Count > 0)
            {
                p = Params.Pop();
                p.StackIdx = paramidx;
                Parameters.Add(p);

                paramidx += 2;
            }
            if (Parameters.Count > 0)
            {
                ec.EmitComment("Parameters Definitions");
                foreach (ParameterSpec par in Parameters)
                    ec.EmitComment("Parameter " + par.Name + " @BP" + par.StackIdx);

            }
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
            // ret
            ec.EmitInstruction(new SimpleReturn());
            return true;
        }
    }


    public class MethodDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.NoModifier;
        CallingConventions ccv = CallingConventions.StdCall;

        public bool EntryPoint = false;
        Stack<ParameterSpec> Params { get; set; }
        Queue<ParameterSpec> StdParams { get; set; }
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
            ccv = _id.CCV.CallingConvention;
            base._type = _id.TType;

        
            List<TypeSpec> tp = new List<TypeSpec>();
            StdParams = new Queue<ParameterSpec>();
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
                    {
                        Params.Push(par._id.ParameterName);
                        tp.Add(par._id.ParameterName.MemberType);
                        StdParams.Enqueue(par._id.ParameterName);
                    }
                    par = par._nextid;
                }
            }
            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods, _id.TType.Type, ccv, tp.ToArray(),this.loc);

      
            method.Parameters = Params.ToList<ParameterSpec>();
         

            MethodSpec m = rc.Resolver.TryResolveMethod(method.Signature.ToString());
            if (m != null)
                ResolveContext.Report.Error(9, Location, "Duplicate method signature");


            rc.KnowMethod(method);
            rc.CurrentMethod = method;
            if (!method.MemberType.IsBuiltinType)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type "+method.MemberType.ToString() + " is user-defined type.");
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
            ec.EmitComment("Method: Name = " + method.Name + ", EntryPoint = "+ EntryPoint);
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });
            // allocate variables

            uint size = 0;
            foreach (VarSpec v in ec.CurrentResolve.GetLocals())
                size += (uint)v.MemberType.Size;
        
            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });
            //EMit params
            // Get Parameters Indexes
            int paramidx = 4; // Initial Stack Position
            ParameterSpec p = null;
            if (ccv == CallingConventions.StdCall)
            {
                while (Params.Count > 0)
                {
                    p = Params.Pop();
                    p.StackIdx = paramidx;
                    Parameters.Add(p);

                    paramidx += 2;
                }
            }
            else if(ccv == CallingConventions.Cdecl)
            {
                while (StdParams.Count > 0)
                {
                    p = StdParams.Dequeue();
                    p.StackIdx = paramidx;
                    Parameters.Add(p);

                    paramidx += 2;
                }

            }
            if (Parameters.Count > 0)
            {
                ec.EmitComment("Parameters Definitions");
                foreach (ParameterSpec par in Parameters)
                    ec.EmitComment("Parameter " + par.Name + " @BP" + par.StackIdx);

            }
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
            // ret
            ec.EmitInstruction(new SimpleReturn());
            return true;
        }
    }
    public class MethodPrototypeDeclaration : Declaration
    {
        MethodSpec method;
        CallingConventions ccv = CallingConventions.StdCall;
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
            ccv = _id.CCV.CallingConvention;
            base._type = _id.TType;
            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods | Modifiers.Prototype, _id.TType.Type, ccv, null ,this.loc);
            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            List<TypeSpec> tp = new List<TypeSpec>();
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
                        ParameterSpec p = new ParameterSpec("param_" + paid, method, par._id.Type, false, par.loc);
                        Parameters.Add(p);
                        Params.Push(p);
                        tp.Add(p.MemberType);
                    }
                    par = par._nextid;
                    paid++;
                }
            }


            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods, _id.TType.Type, ccv,tp.ToArray(), this.loc);
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
            int idx = 0;
            foreach (TypeMemberSpec m in _def.Members)
            {
                m.Index = idx;
                idx += m.MemberType.Size;
            }
            TypeName = new StructTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, loc);
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
    public class TypeDefDeclaration : Declaration
    {

        public TypeSpec TypeName
        {
            get;
            set;

        }
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
            TypeName = new TypeSpec(rc.CurrentNamespace, _name.Name, _typedef.Type.Size, BuiltinTypes.Unknown, TypeFlags.TypeDef, Modifiers.NoModifier, loc, _typedef.Type);
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
        [Rule("<Namespace> ::= ~namespace Id ~';'")]
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

        [Rule("<GLOBAL> ::= <Decls>")]
        public Global(DeclarationSequence<Declaration> ds)
        {
            Used = new List<Namespace>();
            Declarations = ds;
            Namespace = Namespace.Default;

        }
        [Rule("<GLOBAL> ::= <Namespace> <Imports> <Decls>")]
        public Global(NamespaceDeclaration ndcl, Imports imp, DeclarationSequence<Declaration> ds)
        {
            Declarations = ds;
            Namespace = ndcl.Namespace;
            Used = imp.Used;

        }
        [Rule("<GLOBAL> ::= <Imports> <Decls>")]
        public Global(Imports imp, DeclarationSequence<Declaration> ds)
        {
            Declarations = ds;
            Namespace = Namespace.Default;
            Used = imp.Used;
        }

        [Rule("<GLOBAL> ::= <Namespace> <Decls>")]
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
        public EnumTypeSpec TypeName { get; set; }
        public int Size { get; set; }


        EnumDefinition _def;
        [Rule(@"<Enum Decl>    ::= ~enum Id ~'{' <Enum Def> ~'}'  ~';'")]
        public EnumDeclaration(Identifier id, EnumDefinition edef)
        {
            _name = id;
            _def = edef;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
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
}
