using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    #region Unary Operators
    public class ExtendedUnaryOperator : UnaryOp
    {
        public string SymbolName { get; set; }
        public ExtendedUnaryOperator(bsn.GoldParser.Grammar.Symbol l,string name)
        {
            SymbolName = name;
            symbol = l;
            
            Operator = UnaryOperator.UserDefined;
            Register = RegistersEnum.AX;
     
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {


            CommonType = Right.Type;
            if (Right is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Registers cannot be used with this kind of operators");

            OperatorSpec oper = rc.Resolver.TryResolveOperator(SymbolName);
            if (oper == null)
                ResolveContext.Report.Error(0, Location, "Unknown operator");
            else
            {
                if (oper.IsLogic)
                    CommonType = BuiltinTypeSpec.Bool;

                OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + oper.Name, new TypeSpec[1] { Right.Type });
                if (rc.CurrentMethod == OvlrdOp)
                    OvlrdOp = null;
                else if (OvlrdOp == null)
                    ResolveContext.Report.Error(0, Location, "No operator overloading for this operator");
            }
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            else return false;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            else return false;
        }

    }
    public class ValueOfOp : UnaryOp
    {
        MemberSpec ms;
        public ValueOfOp()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ValueOf;
        }
        TypeSpec MemberType;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!Right.Type.IsPointer)
                ResolveContext.Report.Error(53, Location, "Value of operator cannot be used with non pointer types");
            // VOF
            if (Right is AccessExpression)
            {
                //ResolveContext.Report.Error(53, Location, "Value of operator cannot be used with non variable expressions");
                ms = null;
            }
            else if (Right is VariableExpression)
                ms = (Right as VariableExpression).variable;
            MemberType = Right.Type;
            Right.Type = Right.Type.BaseType;
            if (Right.Type != null)
                CommonType = Right.Type;
            
            
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return Right.Resolve(rc) ;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ms != null)
            {
                if (ms is VarSpec)
                    ms.ValueOf(ec);
                else if (ms is FieldSpec)
                    ms.ValueOf(ec);
                else if (ms is ParameterSpec)
                    ms.ValueOf(ec);
            }
            else
            {
             
              
                ec.EmitComment("ValueOf @Var");
                Right.EmitToStack(ec);
                ec.EmitPop(EmitContext.SI);
                if (MemberType.BaseType.Size <= 2)
                    ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
                else
                {

                    ec.EmitComment("Push ValueOf Var [TypeOf " + MemberType.Name + "] @Var");

                    PushAllFromRegister(ec, EmitContext.SI, MemberType.BaseType.Size, 0);
                }
            }
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
           
            return Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (ms != null)
            {
                if (ms is VarSpec)
                    ms.ValueOfStack(ec);
                else if (ms is FieldSpec)
                    ms.ValueOfStack(ec);
                else if (ms is ParameterSpec)
                    ms.ValueOfStack(ec);
            }
            else
            {
                Right.Emit(ec); 
                ec.EmitPop(EmitContext.SI); // pop @var 
                ec.EmitComment("ValueOf Stack @Var");

                if (MemberType.BaseType.Size <= 2)
                    ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
                else
                    PopAllToRegister(ec, EmitContext.SI, MemberType.BaseType.Size, 0);
         

            }
            return true;
        }
        public override string CommentString()
        {
            return "*" ;
        }

        public bool PushAllFromRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
        {
            int s = size / 2;

            if (size % 2 != 0)
            {
                ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.DL, SourceReg = rg, SourceDisplacement = offset - 1 + size, SourceIsIndirect = true, Size = 8 });
                ec.EmitPush(RegistersEnum.DX);
            }
            for (int i = s - 1; i >= 0; i--)
                ec.EmitInstruction(new Push() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });

            return true;
        }
        public bool PopAllToRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
        {

            int s = size / 2;


            for (int i = 0; i < s; i++)
                ec.EmitInstruction(new Pop() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });
            if (size % 2 != 0)
            {
                ec.EmitPop(RegistersEnum.DX);
                ec.EmitInstruction(new Mov() { DestinationReg = rg, DestinationDisplacement = offset - 1 + size, DestinationIsIndirect = true, Size = 8, SourceReg = RegistersEnum.DL });

            }
            return true;
        }
    }
    public class LoadEffectiveAddressOp : UnaryOp
    {
        MemberSpec ms;
        public LoadEffectiveAddressOp()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.AddressOf;
        }
        TypeSpec MemberType;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
       
                // LEA
            if (Right is AccessExpression)
            {
              ResolveContext.Report.Error(53, Location, "Value of operator cannot be used with non variable expressions");
                ms = null;
            }
            else if (Right is VariableExpression)
                ms = (Right as VariableExpression).variable;
            else
                ResolveContext.Report.Error(54, Location, "Address Of Operator does not support non variable expressions");
                CommonType = Right.Type.MakePointer();
                return this;
         
        }
        public override bool Resolve(ResolveContext rc)
        {
           return Right.Resolve(rc) ;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ms != null)
            {
                if (ms is VarSpec)
                    ms.LoadEffectiveAddress(ec);
                else if (ms is FieldSpec)
                    ms.LoadEffectiveAddress(ec);
                else if (ms is ParameterSpec)
                    ms.LoadEffectiveAddress(ec);
            }
     

            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (ms != null)
            {
                if (ms is VarSpec)
                    ms.LoadEffectiveAddress(ec);
                else if (ms is FieldSpec)
                    ms.LoadEffectiveAddress(ec);
                else if (ms is ParameterSpec)
                    ms.LoadEffectiveAddress(ec);
            }
            else return Emit(ec);
            return true;
        }
        public override string CommentString()
        {
            return  "&";
        }
    }
    [Terminal("!")]
    public class LogicalNotOperator : UnaryOp
    {
        public LogicalNotOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.LogicalNot;
        }
        AssignExpression ae;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(26, Location, "Logical not must be used with boolean type, use ~ instead");

            CommonType = BuiltinTypeSpec.Bool;

            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment("!" + Right.CommentString());
                RegisterExpression.EmitUnaryOperation(ec, new Not(), ((RegisterExpression)Right).Register);
                return true;
            }

         
            Right.EmitToStack(ec);
            ec.EmitComment("!" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Not() { DestinationReg =Register.Value, Size = 80 });
            ec.EmitInstruction(new And() { DestinationReg = Register.Value, SourceValue = 1, Size = 80 });
            ec.EmitPush(Register.Value);
  


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
    }
    [Terminal("~")]
    public class OnesComplementOperator : UnaryOp
    {
        public OnesComplementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.OnesComplement;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "OnesComplement must be used with non boolean, pointer types, use ! instead");
            CommonType = Right.Type;

            if (Right is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            if (RegisterOperation)
            {
                ec.EmitComment("~" + Right.CommentString());
                RegisterExpression.EmitUnaryOperation(ec, new Not(), ((RegisterExpression)Right).Register);
                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("~" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Not() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
         


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }

    }

    [Terminal("??")]
    public class ZeroTestOperator : UnaryOp
    {
        public ZeroTestOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ZeroTest;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(32, Location, "Zero Operators must be used with non boolean, pointer types");
         
            CommonType = BuiltinTypeSpec.Bool;


            if (Right is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            if (RegisterOperation)
            {
                ec.EmitComment("??" + Right.CommentString());
              
                ec.EmitInstruction(new Compare() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 0, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
                ec.EmitPush(((RegisterExpression)Right).Register);
             
                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("??" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = 0, Size = 80 });
            ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
            ec.EmitPush(Register.Value);
       


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);

            if (RegisterOperation)
            {
                ec.EmitComment("??" + Right.CommentString());

                ec.EmitInstruction(new Compare() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 0, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
                ec.EmitPush(((RegisterExpression)Right).Register);

                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("??" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = 0, Size = 80 });
            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
            return true;
        }

    }
    [Terminal("¤")]
    public class ParityTestOperator : UnaryOp
    {
        public ParityTestOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ParityTest;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(33, Location, "Parity Operators must be used with non boolean, pointer types");
         
            CommonType = BuiltinTypeSpec.Bool;

            if (Right is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            if (RegisterOperation)
            {
                ec.EmitComment("¤" + Right.CommentString());

                ec.EmitInstruction(new Test() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 1, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
                ec.EmitPush(((RegisterExpression)Right).Register);

                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("¤" + Right.CommentString());
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new Test() { DestinationReg = Register.Value, SourceValue = 1, Size = 80 });
            ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
            ec.EmitPush(Register.Value);
     


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec,truecase,v, ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
            if (RegisterOperation)
            {
                ec.EmitComment("¤" + Right.CommentString());

                ec.EmitInstruction(new Test() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 1, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
                ec.EmitPush(((RegisterExpression)Right).Register);

                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("¤" + Right.CommentString());
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new Test() { DestinationReg = Register.Value, SourceValue = 1, Size = 80 });
            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
            return true;
        }
    }

    [Terminal("--")]
    public class DecrementOperator : UnaryOp
    {
        public DecrementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.PostfixDecrement;
        }
        AssignExpression ae;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "Unary operator must be used with non boolean, pointer types, use ! instead");

            ae = new AssignExpression(Right, new SimpleAssignOperator(), Right);
            ae = (AssignExpression)ae.DoResolve(rc);
            CommonType = Right.Type;
            if (Right is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                base.EmitOverrideOperator(ec);
                
                return Right.EmitFromStack(ec);
            }
            if (RegisterOperation)
            {
                ec.EmitComment( Right.CommentString()+"--");
                RegisterExpression.EmitUnaryOperation(ec, new Dec(), ((RegisterExpression)Right).Register,false);
                return true;
            }
      
            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "-- ");
            ec.EmitPop(Register.Value);
            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Sub() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
                ec.EmitInstruction(new Dec() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);


            ae.EmitFromStack(ec);
         //   ec.EmitPush(ec.FirstRegister());
        


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                base.EmitOverrideOperator(ec);
                ec.EmitPush(EmitContext.A);
                return Right.EmitFromStack(ec);
            }
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "--");
                RegisterExpression.EmitUnaryOperation(ec, new Dec(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "-- ");
            ec.EmitPop(Register.Value);
            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Sub() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
            ec.EmitInstruction(new Dec() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
            ec.EmitPush(Register.Value);
            ae.EmitFromStack(ec);
          
        
            return true;
        }
    }
    [Terminal("++")]
    public class IncrementOperator : UnaryOp
    {
        public IncrementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.PostfixIncrement;
        }
        AssignExpression ae;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "Unary operator must be used with non boolean, pointer types, use ! instead");


            ae = new AssignExpression(Right, new SimpleAssignOperator(), Right);
            ae = (AssignExpression)ae.DoResolve(rc);
            CommonType = Right.Type;
            if (Right is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                base.EmitOverrideOperator(ec);
                return Right.EmitFromStack(ec);
            }
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "++");
                RegisterExpression.EmitUnaryOperation(ec, new INC(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "++ ");
            ec.EmitPop(Register.Value);
            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Add() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
            ec.EmitInstruction(new INC() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);


            ae.EmitFromStack(ec);
            //   ec.EmitPush(ec.FirstRegister());



            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                base.EmitOverrideOperator(ec);
                ec.EmitPush(EmitContext.A);
                return Right.EmitFromStack(ec);
            }
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "++");
                RegisterExpression.EmitUnaryOperation(ec, new INC(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "++ ");
            ec.EmitPop(Register.Value);
            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Add() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
            ec.EmitInstruction(new INC() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
            ec.EmitPush(Register.Value);
            ae.EmitFromStack(ec);


            return true;
        }
    }

    // TODO FIX SIZEOF/CAST

    public class CastOperator : Expr
    {
        bool nofix = true;
        bool to16 = false;
        bool tosign = false;
        protected MethodSpec OvlrdOp { get; set; }
        protected TypeIdentifier _type;
        protected Expr _target;
        public virtual bool EmitOverrideOperatorFromStack(EmitContext ec)
        {
 
            ec.EmitComment("Override Cast Operator : " + " (" + _type.Type.Name + ")" + _target.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperator(EmitContext ec)
        {
            _target.EmitToStack(ec);
            ec.EmitComment("Override Cast Operator : " + " (" + _type.Type.Name + ")" + _target.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {
            _target.EmitToStack(ec);
            ec.EmitComment("Override Cast Operator : " + " (" + _type.Type.Name + ")" + _target.CommentString());
            ec.EmitCall(OvlrdOp);



            if (_type.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(EmitContext.A), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = EmitContext.TRUE, Size = 80 });

            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = cond, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = acond, DestinationLabel = truecase.Name });

            return true;
        }
      
        public Expr Target
        {
            get { return _target; }
        }
        [Rule(@"<Op Unary> ::= ~'(' <Type> ~')' <Op Unary>")]
        public CastOperator(TypeIdentifier id, Expr target)
        {
            _target = target;
            _type = id;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _type = (TypeIdentifier)_type.DoResolve(rc);
            Type = _type.Type;
            _target = (Expr)_target.DoResolve(rc);
            if(_type.Type.IsForeignType)
              OvlrdOp = rc.Resolver.TryResolveMethod(_type.Type.NormalizedName + "P_OpCast_" + _target.Type.NormalizedName, new TypeSpec[1] { _target.Type });
            else OvlrdOp = rc.Resolver.TryResolveMethod(_type.Type.NormalizedName + "_OpCast_" + _target.Type.NormalizedName, new TypeSpec[1] { _target.Type });

            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            if (OvlrdOp == null)
            {
                // simple fix typedef typedef => type
                if (Type.IsTypeDef && Type.GetTypeDefBase(Type) == _target.Type)
                    Type = _target.Type;
                else if (_target.Type.IsTypeDef && _target.Type.GetTypeDefBase(_target.Type) == Type) // simple fix (type => typedef)
                    _target.Type = Type;
                else if (Type.IsPointer && _target.Type == BuiltinTypeSpec.UInt)
                {
                    _target.Type = Type;
                    nofix = true;
                }
                else if (_target.Type.IsPointer && Type == BuiltinTypeSpec.UInt)
                {
                    Type = _target.Type;
                    nofix = true;
                }
                else if (_target is ConstantExpression) // convert const
                {
                    bool c = false;
                    try
                    {
                        _target = ((ConstantExpression)_target).ConvertExplicitly(rc, Type, ref c);
                    }
                    catch
                    {
                        c = false;
                    }
                    if (!c)
                        ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
                    else return _target;
                }
                else if (_target is CastOperator) // cast under cast
                    _target = ((CastOperator)_target).Target;
                else if (_target.Type.BuiltinType == BuiltinTypes.Byte || _target.Type.BuiltinType == BuiltinTypes.SByte) // byte or sbyte => int or uint
                {
                    if (Type.BuiltinType == BuiltinTypes.UInt)
                    {
                        nofix = false;
                        to16 = true;
                        tosign = false;
                    }
                    else if (Type.BuiltinType == BuiltinTypes.Int)
                    {
                        nofix = false;
                        to16 = true;
                        tosign = true;
                    }
                    else ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
                }
                else if (_target.Type.BuiltinType == BuiltinTypes.Int || _target.Type.BuiltinType == BuiltinTypes.UInt) // int or uint => byte or sbyte
                {
                    if (Type.BuiltinType == BuiltinTypes.Byte)
                    {
                        nofix = false;
                        to16 = false;
                        tosign = false;
                    }
                    else if (Type.BuiltinType == BuiltinTypes.SByte)
                    {
                        nofix = false;
                        to16 = false;
                        tosign = true;
                    }
                    else if (Type.BuiltinType == BuiltinTypes.Int || Type.BuiltinType == BuiltinTypes.UInt)
                        nofix = true;


                    else ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
                }
                else
                    ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            return _type.Resolve(rc) && _target.Resolve(rc);
        }
        public override string CommentString()
        {
            return "Cast " + _target.CommentString() + " to " + Type.ToString();
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return EmitOverrideOperator(ec);

            if (nofix)
            {
                ec.EmitComment(CommentString());
                return _target.Emit(ec);
            }
            else
            {
                _target.Emit(ec);
                // cast
                RegistersEnum src = RegistersEnum.AX;

                ec.EmitPop(src); // take target
                ec.EmitComment(CommentString());
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, src);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, src);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, src);
                ec.EmitPush(src);

             
                return true;

            }
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (OvlrdOp != null)
                return EmitOverrideOperator(ec);

            if (nofix)
            {
                ec.EmitComment(CommentString());
                return _target.EmitToStack(ec);
            }
            else
            {
                _target.EmitToStack(ec);
                // cast

                RegistersEnum dst = RegistersEnum.AX;
                ec.EmitPop(dst); // take target
                ec.EmitComment(CommentString());
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, dst);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, dst);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, dst);
                ec.EmitPush(dst);

         
                return true;

            }
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            if (OvlrdOp != null)
            {
                EmitOverrideOperator(ec);
                ec.EmitPop(rg);
                return true;
            }
            if (nofix)
                return _target.EmitToRegister(ec, rg);
            else
            {
                _target.EmitToRegister(ec, rg);
                // cast
                RegistersEnum src = rg;

                ec.EmitPop(src); // take target
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, src);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, src);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, src);



                return true;
            }
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                EmitOverrideOperatorFromStack(ec);
                return _target.EmitFromStack(ec);
            }
            if (nofix)
                return _target.EmitFromStack(ec);
            else
            {

                // cast
                RegistersEnum src = RegistersEnum.AX;

                ec.EmitPop(src); // take target
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, src);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, src);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, src);

                ec.EmitPush(src);



                return _target.EmitFromStack(ec);
            }
        }
    }
    public class SizeOfOperator : Expr
    {
        public ushort Size
        {
            get;
            set;
        }

        private TypeIdentifier _type;
        private Identifier _id;
        private TypePointer _ptr;


        [Rule(@"<Op Unary> ::= ~sizeof ~'(' <Type> ~')'")]
        public SizeOfOperator(TypeIdentifier type)
        {
            _type = type;
        }
        [Rule(@"<Op Unary> ::= ~sizeof ~'(' Id <Pointers> ~')'")]
        public SizeOfOperator(Identifier type, TypePointer tp)
        {
            _ptr = tp;
            _id = type;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_type != null)
            {
                _type = (TypeIdentifier)_type.DoResolve(rc);
                Type = _type.Type;
                Size = (ushort)Type.Size;
            }

            if (_id != null && _ptr != null)
            {
                Type = rc.Resolver.TryResolveVar(_id.Name).MemberType;
                _ptr = (TypePointer)_ptr.DoResolve(rc);
                Size = (ushort)Type.Size;
                if (_ptr.PointerCount > 0)
                    Size = 2;
                Type = PointerTypeSpec.MakePointer(Type, _ptr.PointerCount);

            }

            return new UIntConstant(Size,Location) ;
        }
        public override bool Resolve(ResolveContext rc)
        {
            Size = 0;
            return true;
        }
     /*   public override bool Emit(EmitContext ec)
        {
            RegistersEnum acc = ec.GetNextRegister();
            ec.EmitInstruction(new Mov() { DestinationReg = acc, SourceValue = (ushort)this.Size, Size = 16 });
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (ushort)this.Size, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)this.Size, Size = 16 });
            return true;
        }

        public override string CommentString()
        {
            return "sizeOf(" + Type.Name + ")";
        }*/
    }
    public class NameOfOperator : Expr
    {
  

      
        private Identifier _id;
    


        [Rule(@"<Op Unary> ::= ~nameof ~'(' Id ~')'")]
        public NameOfOperator(Identifier type)
        {
            _id = type;
        }
      

        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Type = BuiltinTypeSpec.String;
            if (_id != null )
            {
                MemberSpec ms = rc.Resolver.TryResolveName(_id.Name);
                if (ms != null)
                    return new StringConstant(ms.Name, Location);
                else
                    ResolveContext.Report.Error(0,Location,"Unresolved name " + _id.Name);
            }

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
         
            return true;
        }
        /*   public override bool Emit(EmitContext ec)
           {
               RegistersEnum acc = ec.GetNextRegister();
               ec.EmitInstruction(new Mov() { DestinationReg = acc, SourceValue = (ushort)this.Size, Size = 16 });
               return true;
           }
           public override bool EmitToStack(EmitContext ec)
           {
               ec.EmitInstruction(new Push() { DestinationValue = (ushort)this.Size, Size = 16 });
               return true;
           }
           public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
           {
               ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)this.Size, Size = 16 });
               return true;
           }

           public override string CommentString()
           {
               return "sizeOf(" + Type.Name + ")";
           }*/
    }
    public class AddressOfOperator : Expr
    {



        private Identifier _id;



        [Rule(@"<Op Unary> ::= ~addressof ~'(' Id ~')'")]
        public AddressOfOperator(Identifier type)
        {
            _id = type;
        }

        public string Label=null;
        MemberSpec ms= null;
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Type = BuiltinTypeSpec.UInt;
            if (_id != null)
            {
                if (_id.Name == "begin")
                    Label = "PROGRAM_ORG";
                else if (_id.Name == "end")
                    Label = "PROGRAM_END";
                else
                {
                    ms = rc.Resolver.TryResolveName(_id.Name);
                    if (ms == null)
                    {
                        ms = rc.Resolver.TryResolveMethod(_id.Name);
                        if (ms == null)
                            Label = _id.Name;
                           
                    }
                      
                }
            }
            if(Label == null && ms == null)
                ResolveContext.Report.Error(0, Location, "Unresolved name " + _id.Name);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            return true;
        }
         public override bool Emit(EmitContext ec)
           {
               if (ms == null)
               {


                   ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceRef = ElementReference.New(Label), Size = 16 });
                   ec.EmitPush(EmitContext.A);
               }
               else ms.LoadEffectiveAddress(ec);
               return true;
           }
           public override bool EmitToStack(EmitContext ec)
         {
             if (ms == null)
             {

                 ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceRef = ElementReference.New(Label), Size = 16 });
                 ec.EmitPush(EmitContext.A);
             }
             else ms.LoadEffectiveAddress(ec);
               return true;
           }
       

           public override string CommentString()
           {
               return "AddressOf(" + Label + ")";
           }
    }
    #endregion
}
