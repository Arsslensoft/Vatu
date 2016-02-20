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
        public Expr LeftVar { get; set; }

        RegistersEnum RR;
        public bool IsVarAssign = false;
        [Rule(@"<Register Expression> ::= <REGISTER> ~':=' <REGISTER Target Expression> ")]
        public RegisterAssignOperation(RegisterExpression right, RegisterTargetExpression left)
        {
            LeftExpr = left;
            Right = right;
        }

        [Rule(@"<Register Expression> ::= <Expression> ~':=' <REGISTER>")]
        public RegisterAssignOperation(Expr left, RegisterExpression right)
        {
            LeftVar = left;
            Right = right;
            IsVarAssign = true;
        }
          public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (LeftExpr != null)
               LeftExpr.DoFlowAnalysis(fc);
  
            if (LeftVar != null && LeftVar is VariableExpression)
           {
               fc.MarkAsAssigned((LeftVar as VariableExpression).variable);
               LeftVar.DoFlowAnalysis(fc);
           }
           return Right.DoFlowAnalysis(fc);
       }
        byte size = 16;
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Right = (RegisterExpression)Right.DoResolve(rc);

            if (LeftVar != null)
                LeftVar = (Expr)LeftVar.DoResolve(rc);
            else LeftExpr = (RegisterTargetExpression)LeftExpr.DoResolve(rc);

             RR = Right.Register;
            if (Registers.Is8Bit(RR) == true)
                size = 8;

            if (LeftVar != null && LeftVar.Type.SizeInBits != size)
                ResolveContext.Report.Error(0, Location, "Incompatible variable type, Registers cannot accept this type");

           


      

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
                if (Registers.IsSegment(RR))
                {
                    LeftExpr.Emit(ec);
                    ec.EmitPop(RR);
                }else LeftExpr.EmitMoveToRegister(ec, RR, size);

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
        public Expr LeftExpr { get; set; }

        [Rule(@"<REGISTER Target Expression> ::= <Expression>")]
        public RegisterTargetExpression(Expr left)
        {
            LeftExpr = left;


        }
      

        [Rule(@"<REGISTER Target Expression> ::= <REGISTER>")]
        public RegisterTargetExpression(RegisterExpression right)
        {

            Register = right;

        }

         public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
          
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
           
            else
            {
                LeftExpr = (Expr)LeftExpr.DoResolve(rc);
                if (LeftExpr != null && LeftExpr.Type.Size > 2)
                    ResolveContext.Report.Error(0, Location, "Registers can't hold this type");

                
            }


            return this;
        }
        public string CommentString()
        {
            if (Register != null)
                return Register.Register.ToString();
       
            else return LeftExpr.CommentString();
        }
        public void EmitMoveToRegister(EmitContext ec, RegistersEnum dst, byte size)
        {
            if (Register != null)
                ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = Register.Register, Size = size });
          
            else
            {
                if ((LeftExpr is VariableExpression && !(LeftExpr is AccessExpression) ) )
                {
                    VariableExpression v = (VariableExpression)LeftExpr;
                    if (v.variable is FieldSpec)
                        ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceRef = ElementReference.New(v.variable.Signature.ToString()), SourceIsIndirect = true, Size = size });
                    else if (v.variable is VarSpec)
                        ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = EmitContext.BP, SourceDisplacement = (v.variable as VarSpec).VariableStackIndex, SourceIsIndirect = true, Size = size });
                    else if (v.variable is ParameterSpec)
                        ec.EmitInstruction(new Mov() { DestinationReg = dst, SourceReg = EmitContext.BP, SourceDisplacement = (v.variable as ParameterSpec).StackIdx, SourceIsIndirect = true, Size = size });
                    else if (v.variable is EnumMemberSpec)
                    {
                        (v.variable as EnumMemberSpec).EmitToStack(ec);//AM sign
                        ec.EmitPop(dst);
                    }
                }
                else
                {
                    LeftExpr.EmitToStack(ec);
                    ec.EmitPop(dst);
                }
            }
        }
       
        public override bool Emit(EmitContext ec)
        {
            if (Register != null)
                ec.EmitPush(Register.Register);
         
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
            Label tr = ec.DefineLabel(LabelType.BOOL_EXPR);
            Label t = new Label(tr.Name +"_TRUE");
            ec.EmitInstruction(new Compare() { DestinationReg = Source.TargetRegister, SourceValue = 1 });
            ec.EmitBooleanBranch(true, t, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            FalseVal.Emit(ec);
            ec.EmitInstruction(new Jump() { DestinationLabel = tr.Name });
            ec.MarkLabel(t);
            TrueVal.Emit(ec);
            ec.MarkLabel(tr);
            Target.EmitFromStack(ec);
            return true;
        }
    }

    public class RegisterOperation : RegisterExpr
    {
        public RegistersEnum TargetRegister { get; set; }
    }
}
