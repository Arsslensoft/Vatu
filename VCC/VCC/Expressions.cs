using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
[assembly: RuleTrim("<Value> ::= '(' <Expression> ')'", "<Expression>", SemanticTokenType = typeof(VCC.Core.SimpleToken))]
namespace VCC.Core
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
                return this;
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

        protected Identifier _id;
        protected Expr _param;
        [Rule(@"<Value>       ::= Id ~'(' <Expression> ~')'")]
        public MethodExpression(Identifier id, Expr expr)
        {
            _id = id;
            _param = expr;
        }
        [Rule(@"<Value>       ::= Id ~'(' ~')'")]
        public MethodExpression(Identifier id)
        {
            _id = id;
            _param = null;
        }
        //TODO:CALL PARAMS
        public override bool Resolve(ResolveContext rc)
        {
            Method = rc.TryResolveMethod(_id.Name);
            bool ok = true;
            if (_param != null)
                ok &= _param.Resolve(rc);
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
          
            Parameters = new List<Expr>();
            if (_param != null)
            {
                _param = (Expr)_param.DoResolve(rc);
                Expr a = _param;
                while (a != null)
                {
                    if (a != null)
                        Parameters.Add(a);
                    else if (a.current != null)
                        Parameters.Add(a.current);
                    a = a.next;
                }
            }
            Method = rc.TryResolveMethod(_id.Name);
            if(Method.Parameters.Count != Parameters.Count)
                ResolveContext.Report.Error(46, Location, "the method "+Method.Name + " has different parameters");
            else if (!MatchParameterTypes())
                ResolveContext.Report.Error(46, Location, "the method " + Method.Name + " has different parameters types. try casting"); 
            Type = Method.MemberType;
            return this;
        }
        bool MatchParameterTypes()
        {
            for (int i = 0; i < Method.Parameters.Count; i++)
                if (!TypeChecker.CompatibleTypes(Parameters[i].Type, Method.Parameters[i].MemberType))
                    return false;

            return true;
        }
        public override bool Emit(EmitContext ec)
        {
          
                RegistersEnum acc = EmitContext.A;
                int size = 0;
                // parameters
                if (Parameters.Count > 0)
                {
                    foreach (Expr e in Parameters)
                    {
                    
                          e.EmitToStack(ec);
                          size += e.Type.Size;
                    }

                }

                if (Method.MemberType.IsBuiltinType)
                {
                    // call
                    ec.EmitCall(Method);
                    // clean params 
                    if (size > 0)
                        ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SP, SourceValue = (uint)size,Size = 80});

                  if(Method.MemberType != BuiltinTypeSpec.Void)
                    ec.EmitPush( EmitContext.A );

                }
                
            
       
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
            return base.EmitFromStack(ec);
        }

        public override string CommentString()
        {
            return _id.Name + ((_param!= null)?"()":"(" + _param.CommentString()+ ")");
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

  
        public static void EmitAssign(EmitContext ec,  RegistersEnum dst,uint val)
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
        public static void EmitOperation(EmitContext ec, InstructionWithDestinationAndSourceAndSize ins, uint src, RegistersEnum dst)
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
        bool IsExpr { get; set; }
        /// <summary>
        /// ByVal ccess operator
        /// </summary>
        /// <param name="ms"></param>
        public AccessExpression(MemberSpec ms)
            : base(ms)
       {
           IsExpr = false;
       }

        public VariableExpression Left { get; set; }
        public Expr Right { get; set; }
        public AccessOp Operator { get; set; }
        public AccessExpression(VariableExpression left, Expr right, AccessOp op) : base(left.variable)
        {
            IsExpr = true;
            Left = left;
            Right = right;
            Operator = op;
        }

        public override bool Emit(EmitContext ec)
        {
            if (!IsExpr)
                return base.Emit(ec);
            else
                return Operator.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (!IsExpr)
                return base.EmitFromStack(ec);
            else
                return Operator.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (!IsExpr)
                return base.EmitToStack(ec);
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

   

        [Rule(@"<Value>       ::= Id")]
        public VariableExpression(Identifier id)
            : base(id.Name)
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (variable == null)
            {

                variable = rc.TryResolveVar(Name);
                if (variable == null)
                    variable = rc.TryResolveEnumValue(Name);

                if (variable == null)
                    variable = rc.ResolveParameter(Name);

                if (variable == null)
                    variable = rc.TryResolveField(Name);

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
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            variable = rc.TryResolveVar(_idName);
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

            return true;
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
           if (!(_op.Left is VariableExpression) && ! (_op.Left is AccessOperation) && !(_op.Left is UnaryOperation))
               ResolveContext.Report.Error(42, Location, "Target must be a variable");
           else if ((!(_op.Left is AccessOperation)) && (!(_op.Left is UnaryOperation)) && (_op.Left as VariableExpression).variable.IsConstant)
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

        public int  Offset { get; set; }
        public MemberSpec Member { get; set; }
        private  AccessOp _op;
        [Rule(@"<Op Pointer> ::= <Op Pointer> '.' <Value>")]
        [Rule(@"<Op Pointer> ::= <Op Pointer> '->' <Value>")]
        public AccessOperation(Expr left, AccessOp op, Expr target)
        {
            _op = op;
     
            _op.Left = left;
            _op.Right = target;


        }

        [Rule(@"<Op Pointer> ::= Id '::' <Value>")]
        public AccessOperation(Identifier id, AccessOp op, Expr target)
        {
            _op = op;
            _op.Namespace = new Namespace(id.Name);
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
                _op.Right = (Expr)_op.Right.DoResolve(rc);
                _op.Left = (Expr)_op.Left.DoResolve(rc);
                 return _op.DoResolve(rc);

            }
            else
            {
                Namespace lastns = rc.CurrentNamespace;
                rc.CurrentNamespace = _op.Namespace;
                _op.Right = (Expr)_op.Right.DoResolve(rc);
        
                rc.CurrentNamespace = lastns;
                return _op.Right;
            }
            rc.CurrentScope &= ~ResolveScopes.AccessOperation;
           Type = _op.CommonType;
           Offset = _op.Offset;
           Member = _op.Member;
            return this;
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
        [Rule(@"<Op Unary>   ::= '£'   <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '$'   <Op Unary>")]
        public UnaryOperation(Operator op, Expr target)
        {
            _op = op;
       
            if (op is BitwiseAndOperator)
                _op = new LoadEffectiveAddressOp();
            else if (op is MultiplyOperator)
                _op = new ValueOfOp();
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
          
            _op.Right = (Expr)_op.Right.DoResolve(rc);
            _op = (Operator)_op.DoResolve(rc);
            if (!TypeChecker.ArtihmeticsAllowed(_op.Right.Type, _op.Right.Type))
                ResolveContext.Report.Error(46, Location, "Binary operations are not allowed for this type");
            Type = _op.Right.Type;
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Expr tmp;
            _op.Right = (Expr)_op.Right.DoResolve(rc);
            _op.Left = (Expr)_op.Left.DoResolve(rc);

            if (!TypeChecker.ArtihmeticsAllowed(_op.Right.Type, _op.Left.Type))
                ResolveContext.Report.Error(46, Location, "Binary operations are not allowed for this type");
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

        [Rule(@"<Op If>      ::= <Op Or> ~'?' <Op If> ~':' <Op If>")]
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
            _cond.Emit(ec);
            _true.Emit(ec);
            _false.Emit(ec);
            return true;
        }
    }
}
