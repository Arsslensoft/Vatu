using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC.Core
{
    public class RegisterBinaryOperation : RegisterOperation
    {
        public RegisterExpression Left { get; set; }
        public RegisterExpression Right { get; set; }

        bool require_mov = false;
        bool shift_mov = false;
        RegistersEnum LR, RR;
        public BinaryOp Operator { get; set; } 
        [Rule(@"<Register Operation> ::= <REGISTER> <Binary Operator> <REGISTER>")]
        public RegisterBinaryOperation(RegisterExpression left, BinaryOperatorDefinition binop, RegisterExpression right)
        {
            Left = left;
            Right = right;
            Operator = binop.Operator;
        }

        byte size = 16;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Left = (RegisterExpression)Left.DoResolve(rc);
            Right = (RegisterExpression)Right.DoResolve(rc);
            LR = Left.Register;
            TargetRegister = LR;
            RR = Right.Register;
            if (Operator is MultiplyOperator || Operator is DivisionOperator || Operator is ModulusOperator)
                require_mov = true;
            if ((Operator.Operator & BinaryOperator.ShiftMask) != 0)
                shift_mov = true;

            if ((Operator.Operator & BinaryOperator.ShiftMask) != 0 && !Registers.Is8Bit(LR))
                ResolveContext.Report.Error(0, Location, "Register shift operation accepts only 8 bits left register");
          
        


            if((Registers.Is8Bit(LR)==true || Registers.Is8Bit(RR)==true))
                size = 8;
            else size = 16;
            
       

            return this;
        }
        void MoveRegister(EmitContext ec,RegistersEnum src, RegistersEnum dst)
        {
            if(src != dst)
                ec.EmitInstruction(new Mov() { SourceReg = src, DestinationReg = dst, Size = 16 });
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment(LR.ToString()+ "=" +LR.ToString() + Operator.Name + RR.ToString());
            if(RR != LR)
               ec.EmitPush(RR);
            if (require_mov)
            {
                ec.EmitPush(EmitContext.C);
                ec.EmitPush(EmitContext.A);
            
                MoveRegister(ec, LR, EmitContext.A);
                MoveRegister(ec, RR, EmitContext.C);
            }
            if (shift_mov)
                MoveRegister(ec, RR,ec.GetLow( EmitContext.C));


            if (Operator is MultiplyOperator)
            {
                ec.EmitInstruction(new Multiply() { DestinationReg = EmitContext.C });
                MoveRegister(ec, EmitContext.A, LR);
                ec.EmitPop(EmitContext.A);
                ec.EmitPop(EmitContext.C);
            }
            else if (Operator is DivisionOperator)
            {
                ec.EmitInstruction(new Divide() { DestinationReg = EmitContext.C });
                MoveRegister(ec, EmitContext.A, LR);
                ec.EmitPop(EmitContext.A);
                ec.EmitPop(EmitContext.C);
            }
            else if (Operator is ModulusOperator)
            {
                ec.EmitPush(EmitContext.D);
                ec.EmitInstruction(new Divide() { DestinationReg = EmitContext.C });
                MoveRegister(ec, EmitContext.D, LR);
                ec.EmitPop(EmitContext.D);
                ec.EmitPop(EmitContext.A);
                ec.EmitPop(EmitContext.C);
          
             
            }
            else if (Operator is AdditionOperator)
                ec.EmitInstruction(new Add() { SourceReg = RR, DestinationReg = LR, Size = size });
            else if (Operator is SubtractionOperator)
                ec.EmitInstruction(new Sub() { SourceReg = RR, DestinationReg = LR, Size = size });

            else if (Operator is BitwiseXorOperator)
                ec.EmitInstruction(new Xor() { SourceReg = RR, DestinationReg = LR, Size = size });
            else if (Operator is BitwiseAndOperator)
                ec.EmitInstruction(new And() { SourceReg = RR, DestinationReg = LR, Size = size });
            else if (Operator is BitwiseOrOperator)
                ec.EmitInstruction(new Or() { SourceReg = RR, DestinationReg = LR, Size = size });

            else if (Operator is LeftShiftOperator)
                ec.EmitInstruction(new ShiftLeft() { SourceReg = RegistersEnum.CL, DestinationReg = LR, Size = 80 });
            else if (Operator is RightShiftOperator)
                ec.EmitInstruction(new ShiftRight() { SourceReg = RegistersEnum.CL, DestinationReg = LR, Size = 80 });
            else if (Operator is LeftRotateOperator)
                ec.EmitInstruction(new RotateLeft() { SourceReg = RegistersEnum.CL, DestinationReg = LR, Size = 80 });
            else if (Operator is RightRotateOperator)
                ec.EmitInstruction(new RotateRight() { SourceReg = RegistersEnum.CL, DestinationReg = LR, Size = 80 });

            // compare operators
            else if ((Operator.Operator & BinaryOperator.ComparisonMask) != 0)
            {
                ec.EmitInstruction(new Compare() { SourceReg = RR, DestinationReg = LR });
                if (Operator is EqualOperator)
                    ec.EmitInstruction(new ConditionalSet() { DestinationReg = ec.GetLow(LR), Condition = ConditionalTestEnum.Equal });
                else if (Operator is NotEqualOperator)
                    ec.EmitInstruction(new ConditionalSet() { DestinationReg = ec.GetLow(LR), Condition = ConditionalTestEnum.NotEqual });
                else if (Operator is GreaterThanOperator)
                    ec.EmitInstruction(new ConditionalSet() { DestinationReg = ec.GetLow(LR), Condition = ConditionalTestEnum.GreaterThan });
                else if (Operator is GreaterThanOrEqualOperator)
                    ec.EmitInstruction(new ConditionalSet() { DestinationReg = ec.GetLow(LR), Condition = ConditionalTestEnum.GreaterThanOrEqualTo });
                else if (Operator is LessThanOperator)
                    ec.EmitInstruction(new ConditionalSet() { DestinationReg = ec.GetLow(LR), Condition = ConditionalTestEnum.LessThan });
                else if (Operator is LessThanOrEqualOperator)
                    ec.EmitInstruction(new ConditionalSet() { DestinationReg = ec.GetLow(LR), Condition = ConditionalTestEnum.LessThanOrEqualTo });





            }
            if (RR != LR)
            ec.EmitPop(RR);
            return true;
        }
    }
}
