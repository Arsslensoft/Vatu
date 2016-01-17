using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
[assembly: RuleTrim("<Value> ::= '(' <Expression> ')'", "<Expression>", SemanticTokenType = typeof(VTC.Core.SimpleToken))]
namespace VTC.Core
{

    public class ConstantExpression : Expr
    {
       

    

        public ConstantExpression(TypeSpec type, Location loc)
            : base(type, loc)
        {

        }




        /// <summary>
        ///  This is used to obtain the actual value of the literal
        ///  cast into an object.
        /// </summary>
        public virtual object GetValue()
        {
            return null;
        }

        public override bool EmitToStack(EmitContext ec)
        {
       
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual ConstantExpression ConvertExplicitly(ResolveContext rc, TypeSpec type, ref bool cv)
        {
            cv = false;
            if (this.type == type)
            {
                cv = true;
                return this;
            }
            if (type is RegisterTypeSpec)
                type = type.BaseType;
            if (!type.IsBuiltinType)
            {
                ResolveContext.Report.Error(12, Location, "Cannot convert type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (type.IsPointer && (this.type != BuiltinTypeSpec.Int && this.type != BuiltinTypeSpec.UInt))
            {
                ResolveContext.Report.Error(13, Location, "Cannot convert pointer type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (!CanConvertExplicitly(this.type, type))
                return this;
            else
            {
                cv = true;
                return ConstantExpression.CreateConstantFromValueExplicitly(type, this.GetValue(), loc);
            }
        }
        public void ConvertArrays(byte[] src, ref byte[] dst, int typesize)
        {
            if (src.Length % typesize != 0)
                dst = new byte[src.Length + 1];
            else dst = new byte[src.Length];

            System.Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            

        }
        public virtual ConstantExpression ConvertImplicitly(ResolveContext rc, TypeSpec type,ref bool cv)
        {
            if (this is ArrayConstant)
            {
                cv = true;
                byte[] a = null;
                  ConvertArrays((byte[])GetValue(), ref a,type.Size);
                return new ArrayConstant(a,Location);
            }

            cv = false;
            if (TypeChecker.Equals(this.type, type))
            {
                cv = true;
                return this;
            }

            if (type is RegisterTypeSpec)
                type = type.BaseType;
            if (!type.IsBuiltinType)
            {
                ResolveContext.Report.Error(12, Location, "Cannot convert type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (type.IsPointer && (this.type != BuiltinTypeSpec.Int && this.type != BuiltinTypeSpec.UInt))
            {
                ResolveContext.Report.Error(13, Location, "Cannot convert pointer type " + this.type.Name + " to " + type.Name);
                return null;
            }
            if (!CanConvert(this.type, type))
                return this;
            else
            {
                cv = true;
                return ConstantExpression.CreateConstantFromValue(type, this.GetValue(), loc);
            }
        }
        public static bool CanConvert(TypeSpec src, TypeSpec dst)
        {
            if (src.Size > dst.Size)
                return false;
            else if (!src.IsBuiltinType && !dst.IsBuiltinType)
                return (src == dst);
            else if (src.IsBuiltinType && dst.IsBuiltinType)
            {
                // try convert
                if(src.BuiltinType == BuiltinTypes.Byte)
                    return (dst.BuiltinType == BuiltinTypes.Int || dst.BuiltinType == BuiltinTypes.SByte || dst.BuiltinType == BuiltinTypes.UInt);
                else if (src.BuiltinType == BuiltinTypes.SByte)
                    return (dst.BuiltinType == BuiltinTypes.Int || dst.BuiltinType == BuiltinTypes.Byte || dst.BuiltinType == BuiltinTypes.UInt);
                
                return false;
            }
            else if (src.IsPointer && dst.IsBuiltinType && dst.BuiltinType == BuiltinTypes.UInt) // convert pointer to uint
                return true;
            else if (dst.IsPointer && src.IsBuiltinType && src.BuiltinType == BuiltinTypes.UInt) // convert uint to pointer
                return true;
            else return false;
           

        }
        public static bool CanConvertExplicitly(TypeSpec src, TypeSpec dst)
        {
             if (!src.IsBuiltinType && !dst.IsBuiltinType)
                return (src == dst);
            else if (src.IsBuiltinType && dst.IsBuiltinType)
            {
                // try convert
                return true;
            }
            else if (src.IsPointer && dst.IsBuiltinType && dst.BuiltinType == BuiltinTypes.UInt) // convert pointer to uint
                return true;
            else if (dst.IsPointer && src.IsBuiltinType && src.BuiltinType == BuiltinTypes.UInt) // convert uint to pointer
                return true;
            else return false;


        }
        public static ConstantExpression CreateConstantFromValue(TypeSpec t, object v, Location loc)
        {
           
            switch (t.BuiltinType)
            {
                case BuiltinTypes.Int:
                    return new IntConstant(short.Parse(v.ToString()), loc);
                case BuiltinTypes.String:
                    return new StringConstant(v.ToString(), loc);
                case BuiltinTypes.UInt:
                    return new UIntConstant(ushort.Parse(v.ToString()), loc);
                case BuiltinTypes.SByte:
                    return new SByteConstant(sbyte.Parse(v.ToString()), loc);
                case BuiltinTypes.Byte:
                    return new ByteConstant(byte.Parse(v.ToString()), loc);
                case BuiltinTypes.Bool:
                    return new BoolConstant(bool.Parse(v.ToString()), loc);

            }


            return null;

        }
        public static ConstantExpression CreateConstantFromValueExplicitly(TypeSpec t, object v, Location loc)
        {
            if (t.BuiltinType == BuiltinTypes.String)
                return new StringConstant(v.ToString(), loc);
        
            long l = long.Parse(v.ToString());
            switch (t.BuiltinType)
            {
                case BuiltinTypes.Int:
                    return new IntConstant((short)l, loc);
                case BuiltinTypes.Pointer:
                case BuiltinTypes.UInt:
                    return new UIntConstant((ushort)l, loc);
                case BuiltinTypes.SByte:
                    return new SByteConstant((sbyte)l, loc);
                case BuiltinTypes.Byte:
                    return new ByteConstant((byte)l, loc);
                case BuiltinTypes.Bool:
                    return new BoolConstant(l > 0, loc);

            }
            

            return null;

        }
    }

    /// <summary>
    /// Method Expression
    /// </summary>
    public class MethodExpression : Expr
    {
        public MethodSpec Method { get; set; }
        public List<Expr> Parameters { get; set; }
        bool isdelegate = false;
        protected Identifier _id;
        protected ParameterSequence<Expr> _param;
        [Rule(@"<Method Expr>       ::= Id ~'(' <PARAM EXPR> ~')'")]
        public MethodExpression(Identifier id, ParameterSequence<Expr> expr)
        {
            _id = id;
            _param = expr;
        }
       
        MemberSpec DelegateVar;
        bool ResolveDelegate(ResolveContext rc)
        {
            MemberSpec r = rc.Resolver.TryResolveName(_id.Name);
            if (r is MethodSpec)
                return false;
            else if(r != null)
            {
                if (!r.MemberType.IsDelegate)
                    ResolveContext.Report.Error(0, Location, "Only delegate can be called with parameters");
                else
                {
                    isdelegate = true;
                    if (MatchParameterTypes(r.MemberType as DelegateTypeSpec))
                    {
                        DelegateVar = r;
                        return true;
                    }
                    else { ResolveContext.Report.Error(0, Location, "Delegate parameters mismatch"); return false; }
                }
            }
            return false;
        }
        public override bool Resolve(ResolveContext rc)
        {
        
            bool ok = true;
            if (_param != null)
                ok &= _param.Resolve(rc);
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            ccvh = new CallingConventionsHandler();
            List<TypeSpec> tp = new List<TypeSpec>();
            Parameters = new List<Expr>();
            if (_param != null)
            {
                foreach (Expr p in _param)
                {
                    Expr e =(Expr)p.DoResolve(rc);
                    Parameters.Add(e);
                    tp.Add(e.Type);

                }
        
            }
        
            MemberSignature msig= new MemberSignature();
            if (tp.Count > 0)
            {
                if (!ResolveDelegate(rc))
                    Method = rc.Resolver.TryResolveMethod(_id.Name, tp.ToArray());
                else
                {
                    Type = (DelegateVar.MemberType as DelegateTypeSpec).ReturnType;
                    return this;
                }

                if (Method == null)
                    msig = new MemberSignature(rc.CurrentNamespace, _id.Name, tp.ToArray(), loc);

            }
            else
            {
                if (!ResolveDelegate(rc))
                    Method = rc.Resolver.TryResolveMethod(_id.Name);
                if (Method == null)
                    msig = new MemberSignature(rc.CurrentNamespace, _id.Name, tp.ToArray(), loc);
            }
           if ((rc.CurrentScope & ResolveScopes.AccessOperation) == ResolveScopes.AccessOperation && rc.CurrentExtensionLookup != null)
            {
                if (Method == null)
                    ResolveContext.Report.Error(46, Location, "Unresolved extension method");
                else if (rc.ExtensionVar != null &&  Parameters.Count < Method.Parameters.Count)
                    Parameters.Add(rc.ExtensionVar);
                else if (!rc.StaticExtensionLookup)
                    ResolveContext.Report.Error(46, Location, "Extension methods must be called with less parameters by 1, the first is reserved for the extended type");
            }
            if(Method == null)
                ResolveContext.Report.Error(46, Location, "Unknown method " + msig.NormalSignature + " ");
            else if(Method.Parameters.Count != Parameters.Count)
                ResolveContext.Report.Error(46, Location, "the method "+Method.Name + " has different parameters");
            else if (!MatchParameterTypes(rc))
                ResolveContext.Report.Error(46, Location, "the method " + Method.Name + " has different parameters types. try casting");
       
            if (Method != null)
            Type = Method.MemberType;
            return this;
        }
        bool MatchParameterTypes(ResolveContext rc)
        {
            for (int i = 0; i < Method.Parameters.Count; i++)
                if (!TypeChecker.CompatibleTypes(Parameters[i].Type, Method.Parameters[i].MemberType))
                    return false;
                else if( Method.Parameters[i].IsReference)
                {
                 LoadEffectiveAddressOp lea =   new LoadEffectiveAddressOp();
                 lea.loc = Parameters[i].loc;
                 Parameters[i] = new UnaryOperation(Parameters[i], lea);
                 Parameters[i].loc = lea.loc;
                 Parameters[i] = (Expr)Parameters[i].DoResolve(rc);

                }

            return true;
        }
        bool MatchParameterTypes(DelegateTypeSpec t)
        {
            for (int i = 0; i < t.Parameters.Count; i++)
                if (!TypeChecker.CompatibleTypes(Parameters[i].Type, t.Parameters[i]))
                    return false;

            return true;
        }
        CallingConventionsHandler ccvh;
        public override bool Emit(EmitContext ec)
        {
            if (isdelegate && DelegateVar != null)
            {
                DelegateVar.EmitToStack(ec);
                ec.EmitPop(RegistersEnum.BX);
                ccvh.EmitCall(ec, Parameters, DelegateVar, RegistersEnum.BX, (DelegateVar.MemberType as DelegateTypeSpec).CCV);
            }
            else
            ccvh.EmitCall(ec, Parameters, Method);
                           
       
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            Emit(ec);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            throw new NotImplementedException();
       //     return base.EmitFromStack(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            Emit(ec);
            ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            return true;
        }
        public override string CommentString()
        {
            return _id.Name + ((_param== null)?"()":"(" + ")");
        }
    }

    /// <summary>
    /// Register Expr
    /// </summary>
    public class RegisterExpression : Identifier
    {
        public RegistersEnum Register { get; set; }

        RegisterIdentifier rid;
        [Rule(@"<Value>       ::= <REGISTER>")]
        public RegisterExpression(RegisterIdentifier id)
            : base(id.Name)
        {
            rid = id;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
           
            rid = (RegisterIdentifier)rid.DoResolve(rc);
            Register = rid.Register;
            Type = rid.Is16Bits ? RegisterTypeSpec.RegisterWord : RegisterTypeSpec.RegisterByte;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
          
            return rid.Resolve(rc);
        }
       
        public override bool Emit(EmitContext ec)
        {
           
            ec.EmitPush(Register);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitPop(Register);
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitPush(Register);
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            if(rg != Register)
                ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceReg = Register , Size = 80 });

            return true;
        }
        public override string CommentString()
        {
            return Name;
        }

  
        public static void EmitAssign(EmitContext ec,  RegistersEnum dst,ushort val)
        {
     
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceValue = val, Size = 80 });

        }
        public static void EmitAssign(EmitContext ec, RegistersEnum src, RegistersEnum dst)
        {
            if (src != dst)
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = src, Size = 80 });

        }
        public static void EmitOperation(EmitContext ec, InstructionWithDestinationAndSourceAndSize ins, RegistersEnum src, RegistersEnum dst,bool tostack=true)
        {
            ins.SourceReg = src;
            ins.DestinationReg = dst;
            
            ins.Size = 80;
            ec.EmitInstruction(ins);
            if(tostack)
            ec.EmitPush(dst);
           
        }
        public static void EmitOperation(EmitContext ec, InstructionWithDestinationAndSourceAndSize ins, ushort src, RegistersEnum dst)
        {
            ins.SourceValue = src;
            ins.DestinationReg = dst;

            ins.Size = 80;
            ec.EmitInstruction(ins);
            ec.EmitPush(dst);

        }
        public static void EmitUnaryOperation(EmitContext ec, InstructionWithDestinationAndSize ins, RegistersEnum dst, bool tostack = true)
        {
 
            ins.DestinationReg = dst;

            ins.Size = 80;
            ec.EmitInstruction(ins);
            if (tostack)
                ec.EmitPush(dst);

        }
    }

   
    public class AccessExpression : VariableExpression
    {
        public static RegistersEnum LastUsed;
        public static void SetNext()
        {
            if (LastUsed == RegistersEnum.SI)
                LastUsed = RegistersEnum.DI;
            else LastUsed = RegistersEnum.SI;
        }

      
        bool IsExpr { get; set; }
        bool IsByAdr { get; set; }
        int AccessIndex { get; set; }
        TypeSpec MemberT { get; set; }
        MemberSpec RootVar = null;
        AccessExpression Parent = null;

        public bool IsByIndex { get; set; }

        public AccessExpression(MemberSpec ms,AccessExpression parent )
            : base(ms)
        {

            if (parent != null && parent.IsByAdr)
                Parent = parent;
           

           

            IsByAdr = false;
            IsExpr = false;

            Type = ms.MemberType;
        }
        /// <summary>
        /// ByVal ccess operator
        /// </summary>
        /// <param name="ms"></param>
        public AccessExpression(MemberSpec ms,AccessExpression parent, bool adr = false, int index = 0, TypeSpec mem = null)
            : base(ms)
        {
            if (adr)
            {
                RootVar = ms;
                SetNext();
                variable = new RegisterSpec(mem, LastUsed,Location,index);
              
            }
            if(parent != null)
                Parent = parent;
            IsByAdr = adr;
           IsExpr = false;
           AccessIndex = index;
           MemberT = mem;
           Type = mem != null?mem:ms.MemberType;
       }

        public VariableExpression Left { get; set; }
        public Expr Right { get; set; }
        public AccessOp Operator { get; set; }
        public AccessExpression(VariableExpression left, Expr right, AccessOp op) : base(left.variable)
        {
            IsByIndex = true;
            IsExpr = true;
            Left = left;
            Right = right;
            Operator = op;
            Type = left.Type.BaseType;
        }

       
        public void EmitIndirections(EmitContext ec)
        {
            if (Parent == null && RootVar != null && variable is RegisterSpec)
            {
                RootVar.EmitToStack(ec);
                ec.EmitPop((variable as RegisterSpec).Register);
            }

            else if (Parent != null && !Parent.IsByIndex)
            {
                if (Parent.Parent != null)
                    Parent.EmitIndirections(ec);

                if (IsByAdr)
                {

                    Parent.variable.EmitToStack(ec);
                    ec.EmitPop((variable as RegisterSpec).Register);
                }
            }
            else if (Parent != null) // by index
            {
                Parent.EmitToStack(ec);
                ec.EmitPop((variable as RegisterSpec).Register);
            }
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("Indirections ");
            EmitIndirections(ec);

            if (!IsExpr && !IsByAdr)
                return base.Emit(ec);
            else if (IsByAdr)
            {
                if (variable is VarSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is FieldSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is ParameterSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is RegisterSpec)
                    variable.EmitToStack(ec);
                return true;
            }
            else
                return Operator.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitComment("Indirections ");
            EmitIndirections(ec);
            if (!IsExpr && !IsByAdr)
                return base.EmitFromStack(ec);
            else if (IsByAdr)
            {

                if (variable is VarSpec)
                    variable.ValueOfStackAccess(ec, AccessIndex, MemberT);
                else if (variable is FieldSpec)
                    variable.ValueOfStackAccess(ec, AccessIndex, MemberT);
                else if (variable is ParameterSpec)
                    variable.ValueOfStackAccess(ec, AccessIndex, MemberT);
                else if (variable is RegisterSpec)
                    variable.EmitFromStack(ec);
                return true;

            }
            else
                return Operator.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitComment("Indirections ");
            EmitIndirections(ec);


            if (!IsExpr && !IsByAdr)
                return base.EmitToStack(ec);
            else if (IsByAdr)
            {

                if (variable is VarSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is FieldSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is ParameterSpec)
                    variable.ValueOfAccess(ec,AccessIndex,MemberT);
                else if (variable is RegisterSpec)
                    variable.EmitToStack(ec);
                return true;
            }
            else
                return Operator.EmitToStack(ec);
        }
        public override string CommentString()
        {
            if (!IsExpr)
                return base.CommentString();
            else
                return Operator.CommentString();
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });

            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            return true;
        }
    }

    /// <summary>
    /// Variable Expr
    /// </summary>
    public class VariableExpression : Identifier
    {

       public MemberSpec variable;
       /// <summary>
       /// ByVal ccess operator
       /// </summary>
       /// <param name="ms"></param>
       public VariableExpression(MemberSpec ms)
           : base(ms.Name)
       {
           Type = ms.MemberType;
           variable = ms;
       }

        [Rule(@"<Var Expr>       ::= Id")]
        public VariableExpression(Identifier id)
            : base(id.Name)
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (variable == null)
            {

                variable = rc.Resolver.TryResolveVar(Name);
                if (variable == null)
                    variable = rc.Resolver.TryResolveEnumValue(Name);

                if (variable == null)
                    variable = rc.Resolver.ResolveParameter(Name);

                if (variable == null)
                    variable = rc.Resolver.TryResolveField(Name);

                if (variable != null)
                {
                    if (variable is VarSpec)
                        Type = ((VarSpec)variable).MemberType;
                    else if (variable is FieldSpec)
                        Type = ((FieldSpec)variable).MemberType;
                    else if (variable is EnumMemberSpec)
                        Type = ((EnumMemberSpec)variable).MemberType;
                    else if (variable is ParameterSpec)
                        Type = ((ParameterSpec)variable).MemberType;
                }

                if (variable == null && (rc.CurrentScope & ResolveScopes.AccessOperation) != ResolveScopes.AccessOperation)
                    ResolveContext.Report.Error(14, Location, "Unresolved variable '" + Name + "'");
            }
            base.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            variable = rc.Resolver.TryResolveVar(_idName);
            return base.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            //TODO:Non builtin
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (variable is VarSpec)
                variable.EmitFromStack(ec);
            else if (variable is FieldSpec)
                variable.EmitFromStack(ec);
            else if (variable is ParameterSpec)
                variable.EmitFromStack(ec);
            else if (variable is RegisterSpec)
                variable.EmitFromStack(ec);
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {


            if (variable is VarSpec)
                variable.EmitToStack(ec);
            else if (variable is FieldSpec)
                variable.EmitToStack(ec);
            else if (variable is EnumMemberSpec)
                variable.EmitToStack(ec);
            else if (variable is ParameterSpec)
                variable.EmitToStack(ec);
            else if (variable is RegisterSpec)
                variable.EmitToStack(ec);
            return true;
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });

            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            return true ;
        }
        public override string CommentString()
        {
            return variable.Name ;
        }
    }

    /// <summary>
    /// <Op Assign>
    /// </summary>
    public class AssignExpression : Expr
    {

        AssignOp _op;




        [Rule(@"<Op Assign>  ::= <Op If> '='   <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '<>'   <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '+='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '-='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '*='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '/='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '^='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '&='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '|='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '>>=' <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '<<=' <Op Assign>")]
        public AssignExpression(Expr src, AssignOp op, Expr target)
        {

            _op = op;
            _op.Left = src;
            _op.Right = target;
        }


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _op.Right = (Expr)_op.Right.DoResolve(rc);
            _op.Left = (Expr)_op.Left.DoResolve(rc);
           _op = (AssignOp)_op.DoResolve(rc);
           if (!(_op.Left is VariableExpression) && !(_op.Left is AccessOperation) && !(_op.Left is RegisterExpression) && !(_op.Left is UnaryOperation))
               ResolveContext.Report.Error(42, Location, "Target must be a variable");
           else if ((!(_op.Left is AccessOperation)) & !(_op.Left is RegisterExpression) && (!(_op.Left is UnaryOperation)) && (_op.Left as VariableExpression).variable.IsConstant)
               ResolveContext.Report.Error(43, Location, "Cannot assign a constant variable only in it's declaration");
           Type = _op.Left.Type;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _op.Left.Resolve(rc);
            ok &= _op.Right.Resolve(rc);

            return  ok;
        }
        public override bool Emit(EmitContext ec)
        {

            ec.EmitComment("Assign expression: " + _op.Left.CommentString() + _op.Name + (_op.Right).CommentString());
            return _op.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return _op.EmitFromStack(ec);
        }
#if IMPL
       [Rule(@"<Op Assign>  ::= <Op If>")]
       public AssignExpression(IfExpression expr)
       {
           _src = expr;
           _op = null;
           _target = null;
       }
#endif
    }

    // a.b | a->b | a[5]
    /// <summary>
    /// Access Op
    /// </summary>
    public class AccessOperation : Expr
    {

        public int Offset { get; set; }
        public MemberSpec Member { get; set; }
        private AccessOp _op;
     
  

        [Rule(@"<Op Pointer> ::= <Op Pointer> '.' <Value>")]
        [Rule(@"<Op Pointer> ::= <Op Pointer> '->' <Value>")]
        public AccessOperation(Expr left, AccessOp op, Expr target)
        {
            _op = op;
           
     
            _op.Left = left;
            _op.Right = target;


        }

        [Rule(@"<Op Pointer> ::= <Name> '::' <Value>")]
        public AccessOperation(NameIdentifier id, AccessOp op, Expr target)
        {
            _op = op;
            _op.Namespace = new Namespace(id.Name);
            _op.Right = target;


        }


        [Rule(@"<Op Pointer> ::= <Type> '::' <Value>")]
        public AccessOperation(TypeToken id, AccessOp op, Expr target)
        {
            _op = op;
            _op.LeftType = id;
            _op.Right = target;


        }

        [Rule(@"<Op Pointer> ::= <Op Pointer> ~'[' <Expression> ~']'")]
        public AccessOperation(Expr left, Expr target)
        {
            _op = new ByIndexOperator();
            _op.Left = left;
            _op.Right = target;


        }
#if IMPL
       [Rule(@"<Op Pointer> ::=  <Value>")]
       public AccessOperation(Expression target)
           
       {
           _left = null;
           _op = null;
           _right = target;
         

       }
#endif

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CurrentScope |= ResolveScopes.AccessOperation;

            if (_op._op != AccessOperator.ByName)
            {

                if (_op.Right is MethodExpression)
                {
                    _op.Left = (Expr)_op.Left.DoResolve(rc);

                    rc.CurrentExtensionLookup = _op.Left.Type;
                    rc.StaticExtensionLookup = false;
                    rc.ExtensionVar = _op.Left;
                    _op.Right = (Expr)_op.Right.DoResolve(rc);
                    rc.StaticExtensionLookup = false;
                    rc.ExtensionVar = null;


                    rc.CurrentScope &= ~ResolveScopes.AccessOperation;
                    return _op.Right;
                }
                else
                {
                    _op.Right = (Expr)_op.Right.DoResolve(rc);
                    _op.Left = (Expr)_op.Left.DoResolve(rc);

                    rc.CurrentScope &= ~ResolveScopes.AccessOperation;
                    return _op.DoResolve(rc);
                }
            }
            else
                return _op.DoResolve(rc);
            
          
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_op._op != AccessOperator.ByName)
            {
                bool ok = _op.Left.Resolve(rc);
                ok &= _op.Right.Resolve(rc);
                    return  ok;
            }
            else return _op.Right.Resolve(rc);
        
        }
        public override bool Emit(EmitContext ec)
        {
            return _op.Emit(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {


            return _op.EmitToStack(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {

            return _op.EmitFromStack(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return _op.EmitBranchable(ec, truecase,v);
        }
    }

    /// <summary>
    /// Handle all unary operators
    /// </summary>
    public class UnaryOperation : Expr
    {

        private  Operator _op;

        [Rule(@"<Op Unary>   ::= '!'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '~'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '-'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '*'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '&'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '--'   <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '++'   <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '??'   <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '¤'   <Op Unary>")]
        public UnaryOperation(Operator op, Expr target)
        {
            _op = op;
       
            if (op is BitwiseAndOperator)
                _op = new LoadEffectiveAddressOp();
            else if (op is MultiplyOperator)
                _op = new ValueOfOp();
            _op.Right = target;
        }
        [Rule(@"<Op Unary>   ::= OperatorLiteralUnary   <Op Unary>")]
        public UnaryOperation(OperatorLiteralUnary op, Expr target)
        {
            _op = new ExtendedUnaryOperator(op.Sym, op.Value.GetValue().ToString());
           
            _op.Right = target;
        }
        // Postfix
        [Rule(@"<Op Unary>   ::= <Op Pointer> '--'")]
        [Rule(@"<Op Unary>   ::= <Op Pointer> '++'")]
        public UnaryOperation(Expr target, UnaryOp op)
        {
      
            _op = op;
        
            _op.Right = target;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bool adrop = _op is LoadEffectiveAddressOp || _op is ValueOfOp;
            bool idecop = _op is IncrementOperator || _op is DecrementOperator;
            _op.Right = (Expr)_op.Right.DoResolve(rc);
            _op = (Operator)_op.DoResolve(rc);
           
            if ((idecop && !_op.Right.Type.IsPointer && !_op.Right.Type.IsNumeric) &&!adrop &&  !TypeChecker.ArtihmeticsAllowed(_op.Right.Type, _op.Right.Type))
                ResolveContext.Report.Error(46, Location, "Unary operations are not allowed for this type");
            Type = _op.CommonType;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _op.Right.Resolve(rc);

            return  ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return _op.Emit(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return _op.EmitToStack(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return _op.EmitFromStack(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return _op.EmitBranchable(ec, truecase, v);
        }

        public override string CommentString()
        {
            return _op.Right.CommentString() + _op.CommentString();
        }
#if IMPL
       [Rule(@"<Op Unary>   ::= <Op Pointer>")]
       public UnaryOperation(AccessOperation target)
       {
           _target = target;
           _op = null;

       }
#endif
    }

    /// <summary>
    /// Handle all binary operators
    /// </summary>
    public class BinaryOperation : Expr
    {

        public  BinaryOp _op;
        public bool IsConstant { get; set; }



        
        [Rule(@"<Op Or>      ::= <Op Or> '||' <Op And>")]
        [Rule(@"<Op And>     ::= <Op And> '&&' <Op BinOR>")]
        [Rule(@"<Op BinOR>   ::= <Op BinOR> '|' <Op BinXOR>")]
        [Rule(@"<Op BinXOR>  ::= <Op BinXOR> '^' <Op BinAND>")]
        [Rule(@"<Op BinAND>  ::= <Op BinAND> '&' <Op Equate>")]
        [Rule(@"<Op Equate>  ::= <Op Equate> '==' <Op NEqual>")]
        [Rule(@"<Op NEqual>  ::= <Op NEqual> '!=' <Op Compare>")]
        [Rule(@"<Op Compare> ::= <Op Compare> '<'  <Op Shift>")]
        [Rule(@"<Op Compare> ::= <Op Compare> '>'  <Op Shift>")]
        [Rule(@"<Op Compare> ::= <Op Compare> '<=' <Op Shift>")]
        [Rule(@"<Op Compare> ::= <Op Compare> '>=' <Op Shift>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '<<' <Op Add>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '>>' <Op Add>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '<~' <Op Add>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '~>' <Op Add>")]
        [Rule(@"<Op Add>     ::= <Op Add> '+' <Op Mult>")]
        [Rule(@"<Op Add>     ::= <Op Add> '-' <Op Mult>")]
        [Rule(@"<Op Mult>    ::= <Op Mult> '*' <Op Unary>")]
        [Rule(@"<Op Mult>    ::= <Op Mult> '/' <Op Unary>")]
        [Rule(@"<Op Mult>    ::= <Op Mult> '%' <Op Unary>")]
        public BinaryOperation(Expr left, BinaryOp op, Expr right)
        {
            _op = op;
            _op.Left = left;
            IsConstant = false;
            _op.Right = right;
        }

        [Rule(@"<Op BinaryOpDef>     ::= <Op BinaryOpDef> OperatorLiteralBinary <Op Or>")]
        public BinaryOperation(Expr left, OperatorLiteralBinary op, Expr right)
        {
            _op = new ExtendedBinaryOperator(op.Sym, op.Value.GetValue().ToString());
  
            _op.Left = left;
            IsConstant = false;
            _op.Right = right;
        }
        byte GetValueAsByte(Expr rexp, Expr lexp)
        {
            ByteConstant lce = ((ByteConstant)lexp);
            ByteConstant rce = ((ByteConstant)rexp);


                if (_op is AdditionOperator)
                    return (byte)(lce._value + rce._value);
                else if (_op is SubtractionOperator)
                    return (byte)(lce._value - rce._value);
                else if (_op is MultiplyOperator)
                    return (byte)(lce._value * rce._value);
                else if (_op is DivisionOperator)
                    return (byte)(lce._value / rce._value);
                else if (_op is ModulusOperator)
                    return (byte)(lce._value % rce._value);
                else if (_op is BitwiseAndOperator)
                    return (byte)(lce._value & rce._value);
                else if (_op is BitwiseOrOperator)
                    return (byte)(lce._value | rce._value);
                else if (_op is BitwiseXorOperator)
                    return (byte)(lce._value ^ rce._value);
                else if (_op is LeftShiftOperator)
                    return (byte)(lce._value << rce._value);
                else if (_op is RightShiftOperator)
                    return (byte)(lce._value >> rce._value);

                else if (_op is LeftRotateOperator)
                    return (byte)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
                else if (_op is RightShiftOperator)
                    return (byte)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

                throw new Exception("Failed");

        }
        sbyte GetValueAsSByte(Expr rexp, Expr lexp)
        {
            SByteConstant lce = ((SByteConstant)lexp);
            SByteConstant rce = ((SByteConstant)rexp);


            if (_op is AdditionOperator)
                return (sbyte)(lce._value + rce._value);
            else if (_op is SubtractionOperator)
                return (sbyte)(lce._value - rce._value);
            else if (_op is MultiplyOperator)
                return (sbyte)(lce._value * rce._value);
            else if (_op is DivisionOperator)
                return (sbyte)(lce._value / rce._value);
            else if (_op is ModulusOperator)
                return (sbyte)(lce._value % rce._value);
            else if (_op is BitwiseAndOperator)
                return (sbyte)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (sbyte)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (sbyte)(lce._value ^ rce._value);
            else if (_op is LeftShiftOperator)
                return (sbyte)(lce._value << rce._value);
            else if (_op is RightShiftOperator)
                return (sbyte)(lce._value >> rce._value);

            else if (_op is LeftRotateOperator)
                return (sbyte)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
            else if (_op is RightShiftOperator)
                return (sbyte)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

            throw new Exception("Failed");

        }
        short GetValueAsInt(Expr rexp, Expr lexp)
        {
            IntConstant lce = ((IntConstant)lexp);
            IntConstant rce = ((IntConstant)rexp);


            if (_op is AdditionOperator)
                return (short)(lce._value + rce._value);
            else if (_op is SubtractionOperator)
                return (short)(lce._value - rce._value);
            else if (_op is MultiplyOperator)
                return (short)(lce._value * rce._value);
            else if (_op is DivisionOperator)
                return (short)(lce._value / rce._value);
            else if (_op is ModulusOperator)
                return (short)(lce._value % rce._value);
            else if (_op is BitwiseAndOperator)
                return (short)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (short)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (short)(lce._value ^ rce._value);
            else if (_op is LeftShiftOperator)
                return (short)(lce._value << rce._value);
            else if (_op is RightShiftOperator)
                return (short)(lce._value >> rce._value);

            else if (_op is LeftRotateOperator)
                return (short)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
            else if (_op is RightShiftOperator)
                return (short)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

            throw new Exception("Failed");

        }
        ushort GetValueAsUInt(Expr rexp, Expr lexp)
        {
            UIntConstant lce = ((UIntConstant)lexp);
            UIntConstant rce = ((UIntConstant)rexp);


            if (_op is AdditionOperator)
                return (ushort)(lce._value + rce._value);
            else if (_op is SubtractionOperator)
                return (ushort)(lce._value - rce._value);
            else if (_op is MultiplyOperator)
                return (ushort)(lce._value * rce._value);
            else if (_op is DivisionOperator)
                return (ushort)(lce._value / rce._value);
            else if (_op is ModulusOperator)
                return (ushort)(lce._value % rce._value);
            else if (_op is BitwiseAndOperator)
                return (ushort)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (ushort)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (ushort)(lce._value ^ rce._value);
            else if (_op is LeftShiftOperator)
                return (ushort)(lce._value << rce._value);
            else if (_op is RightShiftOperator)
                return (ushort)(lce._value >> rce._value);

            else if (_op is LeftRotateOperator)
                return (ushort)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
            else if (_op is RightShiftOperator)
                return (ushort)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

            throw new Exception("Failed");

        }
        bool GetValueAsBool(Expr rexp, Expr lexp)
        {
            BoolConstant lce = ((BoolConstant)lexp);
            BoolConstant rce = ((BoolConstant)rexp);


            if (_op is LogicalAndOperator)
                return (bool)(lce._value && rce._value);
            else if (_op is LogicalOrOperator)
                return (bool)(lce._value || rce._value);

            else if (_op is BitwiseAndOperator)
                return (bool)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (bool)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (bool)(lce._value ^ rce._value);


            throw new Exception("Failed");

        }

        bool CompareExpr(Expr rexp, Expr lexp)
        {
            if (rexp is ByteConstant)
                return EvalueComparison((lexp as ByteConstant)._value, (rexp as ByteConstant)._value);
            else if (rexp is SByteConstant)
                return EvalueComparison((lexp as SByteConstant)._value, (rexp as SByteConstant)._value);
            else if (rexp is IntConstant)
                return EvalueComparison((lexp as IntConstant)._value, (rexp as IntConstant)._value);
            else if (rexp is UIntConstant)
                return EvalueComparison((lexp as UIntConstant)._value, (rexp as UIntConstant)._value);
            else if (rexp is UIntConstant)
                return EvalueComparison((lexp as UIntConstant)._value, (rexp as UIntConstant)._value);
            else if (rexp is BoolConstant)
                return EvalueComparison((lexp as BoolConstant)._value, (rexp as BoolConstant)._value);
            else throw new Exception("Failed");
        }

        bool EvalueComparison(byte lv,byte rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(sbyte lv, sbyte rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(int lv, int rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(uint lv, uint rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(bool lv, bool rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv == true && rv == false;
            else if (_op is LessThanOperator)
                return lv == false && rv==true;
            else if (_op is GreaterThanOrEqualOperator)
                return lv == true;
            else if (_op is LessThanOrEqualOperator)
                return lv ==false;
            else return false;
        }

        Expr TryEvaluate()
        {
            try{
            if (_op is EqualOperator || _op is NotEqualOperator || _op is GreaterThanOperator || _op is LessThanOperator || _op is GreaterThanOrEqualOperator || _op is LessThanOrEqualOperator)
                return new BoolConstant(CompareExpr(_op.Right, _op.Left), Location);
            else
            {
                if (_op.Left is ByteConstant)
                    return new ByteConstant(GetValueAsByte(_op.Right, _op.Left), Location);
                else if (_op.Left is SByteConstant)
                    return new SByteConstant(GetValueAsSByte(_op.Right, _op.Left), Location);
                else if (_op.Left is IntConstant)
                    return new IntConstant(GetValueAsInt(_op.Right, _op.Left), Location);
                else if (_op.Left is UIntConstant)
                    return new UIntConstant(GetValueAsUInt(_op.Right, _op.Left), Location);
                else if (_op.Left is BoolConstant)
                    return new BoolConstant(GetValueAsBool(_op.Right, _op.Left), Location);
                else return this;
            }
            }
            catch{
                return this;
            }
           
        }
        bool requireoverload = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Expr tmp;
            _op.Right = (Expr)_op.Right.DoResolve(rc);
            _op.Left = (Expr)_op.Left.DoResolve(rc);

            if (!TypeChecker.ArtihmeticsAllowed(_op.Right.Type, _op.Left.Type))
                requireoverload = true;
              
            // check for const
            if (_op.Right is ConstantExpression && _op.Left is ConstantExpression)
            {
                IsConstant = true;
                // try calculate
                tmp = TryEvaluate();
                if (tmp != this)
                    return tmp;
            }
          
            // end check const
            _op = (BinaryOp)_op.DoResolve(rc);
            if (requireoverload && _op.OvlrdOp == null)
               ResolveContext.Report.Error(46, Location, "Binary operations are not allowed for this type");
            Type = _op.CommonType;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _op.Left.Resolve(rc);
            ok &= _op.Right.Resolve(rc);
        
            return   ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return _op.Emit(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            Emit(ec);
            return true;
        }

        public override string CommentString()
        {
            return _op.Left.CommentString() +_op.CommentString() + _op.Right.CommentString();
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return _op.EmitBranchable(ec, truecase, v);
        }
#if IMPL
       [Rule(@"<Op Mult>    ::= <Op Unary>")]
       public BinaryOperation(UnaryOperation left)
       {
           _left = left;
           _op = null;
           _right = null;
       }
#endif
    }

    /// <summary>
    /// <Arg>
    /// </summary>
    public class ArgumentExpression : Expr
    {
       public Expr argexpr;
        [Rule(@"<Arg>       ::= <Expression> ")]
        public ArgumentExpression(Expr expr)
        {
            argexpr = expr;
        }

        [Rule(@"<Arg>       ::= ")]
        public ArgumentExpression()
        {

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (argexpr != null)
            {
                argexpr = (Expr) argexpr.DoResolve(rc);
                Type = argexpr.Type;
                return argexpr;
            }

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (argexpr != null)
                return  argexpr.Resolve(rc);
            else return false;
        }
        public override bool Emit(EmitContext ec)
        {
            if (argexpr != null)
                return argexpr.Emit(ec);
            return true;
        }

    }

    /// <summary>
    /// <Op If>
    /// </summary>
    public class IfExpression : Expr
    {
        private Expr _cond;
        private Expr _true;
        private Expr _false;

        [Rule(@"<Op If>      ::= <Op BinaryOpDef> ~'?' <Op If> ~':' <Op If>")]
        public IfExpression(Expr cnd, Expr tr, Expr fl)
        {
            _cond = cnd;
            _true = tr;
            _false = fl;
        }
#if IMPL
       [Rule(@"<Op If>      ::= <Op Or>")]
       public IfExpression(BinaryOperation cnd)
       {
           _cond = cnd;
           _true = null;
           _false = null;
       }
#endif
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _cond = (Expr)_cond.DoResolve(rc);
            _true = (Expr)_true.DoResolve(rc);
            _false = (Expr)_false.DoResolve(rc);
            Type = _true.Type;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _cond.Resolve(rc);
            ok &= _true.Resolve(rc);
            ok &= _false.Resolve(rc);
            return  ok;
        }

        public override bool Emit(EmitContext ec)
        {
         Label elselb =   ec.DefineLabel(LabelType.IF_EXPR);
         Label iflb = ec.DefineLabel(elselb.Name + "_EXIT");
            _cond.Emit(ec);
            
            _true.Emit(ec);
            ec.MarkLabel(elselb);
            _false.Emit(ec);
            return true;
        }
    }
}
