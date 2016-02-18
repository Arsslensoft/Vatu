using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class RegisterExpr : SimpleToken, IEmit
    {
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    public class RegisterUnaryOperation : RegisterOperation
    {

        public RegisterExpression Right { get; set; }


        RegistersEnum RR;
        public UnaryOp Operator { get; set; }
        [Rule(@"<Register Operation> ::= <Unary Operator> <REGISTER>")]
        public RegisterUnaryOperation(UnaryOperatorDefinition binop, RegisterExpression right)
        {

            Right = right;
            Operator = binop.Operator;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return Right.DoFlowAnalysis(fc);
        }
        byte size = 16;
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Right = (RegisterExpression)Right.DoResolve(rc);

            RR = Right.Register;

            TargetRegister = RR;

            if (Registers.Is8Bit(RR) == true)
                size = 8;




            return this;
        }
        void MoveRegister(EmitContext ec, RegistersEnum src, RegistersEnum dst)
        {
            if (src != dst)
                ec.EmitInstruction(new Mov() { SourceReg = src, DestinationReg = dst, Size = 16 });
        }
        public override bool Emit(EmitContext ec)
        {

            ec.EmitComment(RR.ToString() + "=" + Operator.Name + RR.ToString());
            if (Operator.Operator == UnaryOperator.PostfixIncrement)
                ec.EmitInstruction(new INC() { DestinationReg = RR });
            else if (Operator.Operator == UnaryOperator.PostfixDecrement)
                ec.EmitInstruction(new Dec() { DestinationReg = RR });
            else if (Operator.Operator == UnaryOperator.OnesComplement)
                ec.EmitInstruction(new Not() { DestinationReg = RR });
            else if (Operator.Operator == UnaryOperator.ZeroTest)
            {
                ec.EmitInstruction(new Test() { DestinationReg = RR });
                ec.EmitInstruction(new ConditionalSet() { DestinationReg = RR, Condition = ConditionalTestEnum.Zero });

            }
            else if (Operator.Operator == UnaryOperator.ParityTest)
            {
                ec.EmitInstruction(new Test() { DestinationReg = RR });
                ec.EmitInstruction(new ConditionalSet() { DestinationReg = RR, Condition = ConditionalTestEnum.ParityEven });

            }
            return true;
        }
    }
    public class RegisterStackOperation : RegisterExpr
    {

        public RegisterExpression Right { get; set; }


        RegistersEnum RR;
        bool ispush = false;

        [Rule(@"<Register Expression> ::= '-' <REGISTER>")]
        [Rule(@"<Register Expression> ::= '+' <REGISTER>")]
        public RegisterStackOperation(BinaryOp binop, RegisterExpression right)
        {

            Right = right;
            ispush = ((binop.Operator & BinaryOperator.Addition) == BinaryOperator.Addition);
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return Right.DoFlowAnalysis(fc);
        }

        byte size = 16;
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Right = (RegisterExpression)Right.DoResolve(rc);

            RR = Right.Register;



            if (Registers.Is8Bit(RR) == true)
                size = 8;




            return this;
        }
        void MoveRegister(EmitContext ec, RegistersEnum src, RegistersEnum dst)
        {
            if (src != dst)
                ec.EmitInstruction(new Mov() { SourceReg = src, DestinationReg = dst, Size = 16 });
        }
        public override bool Emit(EmitContext ec)
        {
            if (ispush)
            {
                ec.EmitComment("Push " + RR.ToString());
                ec.EmitPush(RR);
            }
            else
            {
                ec.EmitComment("Pop " + RR.ToString());
                ec.EmitPop(RR);
            }

            return true;
        }
    }
    public class RegisterAssignOperation : RegisterExpr
    {

        public RegisterExpression Right { get; set; }
        public RegisterTargetExpression LeftExpr { get; set; }
        public VariableExpression LeftVar { get; set; }

        RegistersEnum RR;
        public bool IsVarAssign = false;
        [Rule(@"<Register Expression> ::= <REGISTER> ~':=' <REGISTER Target Expression> ")]
        public RegisterAssignOperation(RegisterExpression right, RegisterTargetExpression left)
        {
            LeftExpr = left;
            Right = right;
        }

        [Rule(@"<Register Expression> ::= <Var Expr> ~':=' <REGISTER>")]
        public RegisterAssignOperation(VariableExpression left, RegisterExpression right)
        {
            LeftVar = left;
            Right = right;
            IsVarAssign = true;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (LeftExpr != null)
                LeftExpr.DoFlowAnalysis(fc);

            if (LeftVar != null)
            {
                fc.MarkAsAssigned(LeftVar.variable);
                LeftVar.DoFlowAnalysis(fc);
            }
            return Right.DoFlowAnalysis(fc);
        }
        byte size = 16;
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Right = (RegisterExpression)Right.DoResolve(rc);

            if (LeftVar != null)
                LeftVar = (VariableExpression)LeftVar.DoResolve(rc);
            else LeftExpr = (RegisterTargetExpression)LeftExpr.DoResolve(rc);

            if (LeftVar != null && LeftVar.Type.IsForeignType)
                ResolveContext.Report.Error(0, Location, "Incompatible variable type, Registers cannot accept this type");

            RR = Right.Register;
            if (Registers.Is8Bit(RR) == true)
                size = 8;




            return this;
        }
        void MoveRegister(EmitContext ec, RegistersEnum src, RegistersEnum dst)
        {
            if (src != dst)
                ec.EmitInstruction(new Mov() { SourceReg = src, DestinationReg = dst, Size = 16 });
        }
        public override bool Emit(EmitContext ec)
        {
            if (!IsVarAssign)
            {
                ec.EmitComment(LeftExpr.CommentString() + " := " + RR.ToString());
                LeftExpr.EmitMoveToRegister(ec, RR, size);

            }
            else
            {
                ec.EmitComment(LeftVar.CommentString() + " := " + RR.ToString());
                ec.EmitPush(RR);
                LeftVar.EmitFromStack(ec);
            }
            return true;
        }
    }

    public class RegisterTargetExpression : RegisterExpr
    {

        public RegisterExpression Register { get; set; }
        public VariableExpression LeftExpr { get; set; }
        public SimpleToken LeftLit { get; set; }


        [Rule(@"<REGISTER Target Expression> ::= <Var Expr>")]
        public RegisterTargetExpression(VariableExpression left)
        {
            LeftExpr = left;


        }
        [Rule(@"<REGISTER Target Expression> ::= <CONSTANT>")]
        public RegisterTargetExpression(Literal left)
        {
            LeftLit = left;

        }

        [Rule(@"<REGISTER Target Expression> ::= <REGISTER>")]
        public RegisterTargetExpression(RegisterExpression right)
        {

            Register = right;

        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (LeftLit != null)
                LeftLit.DoFlowAnalysis(fc);

            if (LeftExpr != null)
          
                LeftExpr.DoFlowAnalysis(fc);
         
            if (Register != null)
                Register.DoFlowAnalysis(fc);

            return FlowState.Valid;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Register != null)
                Register = (RegisterExpression)Register.DoResolve(rc);
            else if (LeftLit != null)
                LeftLit = (ConstantExpression)LeftLit.DoResolve(rc);
            else
            {
                LeftExpr = (VariableExpression)LeftExpr.DoResolve(rc);
                if (LeftExpr != null && LeftExpr.Type.IsForeignType)
                    ResolveContext.Report.Error(0, Location, "Registers can't hold this type");
            }


            return this;
        }
        public string CommentString()
        {
            if (Register != null)
                return Register.Register.ToString();
            else if (LeftLit != null)
                return (LeftLit as ConstantExpression).CommentString();
            else return LeftExpr.CommentString();
        }
        public void EmitMoveToRegister(EmitContext ec, RegistersEnum dst, byte size)
        {
            if (Register != null)
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = Register.Register, Size = size });
            else if (LeftLit != null)
            {
                if (Registers.IsSegment(dst))
                {
                    (LeftLit as ConstantExpression).EmitToStack(ec);
                    ec.EmitPop(dst);
                }
                else
                {
                    if ((size == 8) && (LeftLit is ByteConstant))
                        ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceValue = (ushort)(LeftLit as ByteConstant)._value }); // ushort.Parse((LeftLit as ConstantExpression).GetValue().ToString()) });
                    else if ((size == 8) && (LeftLit is BoolConstant))
                        ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceValue = (LeftLit as BoolConstant)._value ? (ushort)1 : (ushort)0 });
                    else (LeftLit as ConstantExpression).EmitToRegister(ec, dst);
                }
            }
            else
            {
             
                    LeftExpr.EmitToStack(ec);
                    ec.EmitPop(dst);
                    return;
               

            }
        }
        public void EmitMoveToRegisterCond(EmitContext ec, RegistersEnum dst, ConditionalTestEnum cnd)
        {
            if (Register != null)
                ec.EmitInstruction(new ConditionalMove() { DestinationReg = dst, SourceReg = Register.Register, Condition = cnd, Size = 80 });
            else if (LeftLit != null)
            {
                ec.ag.SetAsUsed(dst);
                RegistersEnum r = ec.ag.GetNextRegister();
                (LeftLit as ConstantExpression).EmitToRegister(ec, r);
                ec.ag.FreeRegister();
                ec.ag.FreeRegister();
                ec.EmitInstruction(new ConditionalMove() { DestinationReg = dst, SourceReg = r, Condition = cnd, Size = 80 });
            }
            else
            {
                if (LeftExpr.variable is FieldSpec)
                    ec.EmitInstruction(new ConditionalMove() { DestinationReg = dst, SourceRef = ElementReference.New(LeftExpr.variable.Signature.ToString()), SourceIsIndirect = true, Condition = cnd, Size = 80 });
                else if (LeftExpr.variable is VarSpec)
                    ec.EmitInstruction(new ConditionalMove() { DestinationReg = dst, SourceReg = EmitContext.BP, SourceDisplacement = (LeftExpr.variable as VarSpec).VariableStackIndex, SourceIsIndirect = true, Condition = cnd, Size = 80 });
                else if (LeftExpr.variable is ParameterSpec)
                    ec.EmitInstruction(new ConditionalMove() { DestinationReg = dst, SourceReg = EmitContext.BP, SourceDisplacement = (LeftExpr.variable as ParameterSpec).StackIdx, SourceIsIndirect = true, Condition = cnd, Size = 80 });

            }
        }
        public override bool Emit(EmitContext ec)
        {
            if (Register != null)
                ec.EmitPush(Register.Register);
            else if (LeftLit != null)
                (LeftLit as ConstantExpression).EmitToStack(ec);
            else LeftExpr.EmitToStack(ec);
            return true;
        }
    }

    public class RegisterIfExpression : RegisterExpr
    {
        public RegisterExpression Target { get; set; }
        public RegisterOperation Source { get; set; }

        public RegisterTargetExpression TrueVal { get; set; }
        public RegisterTargetExpression FalseVal { get; set; }
        [Rule(@"<Register Expression> ::= <REGISTER> ~':=' <Register Operation> ~'?' <REGISTER Target Expression>  ~':' <REGISTER Target Expression> ")]
        public RegisterIfExpression(RegisterExpression t, RegisterOperation s, RegisterTargetExpression tr, RegisterTargetExpression fl)
        {
            Source = s;
            Target = t;
            TrueVal = tr;
            FalseVal = fl;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            Source.DoFlowAnalysis(fc);
            Target.DoFlowAnalysis(fc);
            TrueVal.DoFlowAnalysis(fc);
            FalseVal.DoFlowAnalysis(fc);

            return FlowState.Valid;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Target = (RegisterExpression)Target.DoResolve(rc);
            Source = (RegisterOperation)Source.DoResolve(rc);
            TrueVal = (RegisterTargetExpression)TrueVal.DoResolve(rc);
            FalseVal = (RegisterTargetExpression)FalseVal.DoResolve(rc);


            return this;
        }

        public override bool Emit(EmitContext ec)
        {
            Source.Emit(ec);
            ec.EmitInstruction(new Compare() { DestinationReg = Source.TargetRegister, SourceValue = 1 });
            TrueVal.EmitMoveToRegisterCond(ec, Target.Register, ConditionalTestEnum.Equal);
            FalseVal.EmitMoveToRegisterCond(ec, Target.Register, ConditionalTestEnum.NotEqual);

            Target.EmitFromStack(ec);
            return true;
        }
    }

    public class RegisterOperation : RegisterExpr
    {
        public RegistersEnum TargetRegister { get; set; }
    }
}
