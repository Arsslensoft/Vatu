﻿using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    // TODO:ADD OPTIMIZE CONSTANT EVALUATION
    #region Binary Operators

    [Terminal("||")]
    public class LogicalOrOperator : BinaryOp
    {
        public LogicalOrOperator()
        {
            Operator = BinaryOperator.LogicalOr;
            RightRegister = RegistersEnum.BL;
            LeftRegister = RegistersEnum.AL;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type != BuiltinTypeSpec.Bool || Left.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(36, Location, "Right and left must be boolean");

            if (!FixConstant(rc))
                ResolveContext.Report.Error(22, Location, "Logical operation must have the same type");

         

            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");

            CommonType = Left.Type;

            return this;
        }

        public override bool Emit(EmitContext ec)
        {
            
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " || " + Right.CommentString());
            ec.EmitPop( LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);
            ec.EmitInstruction(new Or() { DestinationReg = LeftRegister.Value, SourceReg =RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);

       
            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " || " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);
        
            ec.EmitInstruction(new Or() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceValue = EmitContext.TRUE, Size = 80 });
      
            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.Equal, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotEqual, DestinationLabel = truecase.Name });


            return true;
        }
    }
    [Terminal("&&")]
    public class LogicalAndOperator : BinaryOp
    {
        public LogicalAndOperator()
        {
            Operator = BinaryOperator.LogicalAnd;
            RightRegister = RegistersEnum.BL;
            LeftRegister = RegistersEnum.AL;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            if (!FixConstant(rc))
                ResolveContext.Report.Error(22, Location, "Bitwise operation must have the same type");
            CommonType = Left.Type;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");

            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " && " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);
            ec.EmitInstruction(new And() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " && " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new And() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceValue = EmitContext.TRUE, Size = 80 });

            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.Equal, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotEqual, DestinationLabel = truecase.Name });


            return true;
        }
    }

    [Terminal("|")]
    public class BitwiseOrOperator : BinaryOp
    {
        public BitwiseOrOperator()
        {
            Operator = BinaryOperator.BitwiseOr;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            if (!FixConstant(rc))
                ResolveContext.Report.Error(22, Location, "Bitwise operation must have the same type");
            CommonType = Left.Type;

            if (Right is RegisterExpression && Left is RegisterExpression)
                RegisterOperation = true;
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " | " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new Or() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }
      
    }
    [Terminal("^")]
    public class BitwiseXorOperator : BinaryOp
    {
        public BitwiseXorOperator()
        {
            Operator = BinaryOperator.ExclusiveOr;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            if (!FixConstant(rc))
                ResolveContext.Report.Error(22, Location, "Bitwise operation must have the same type");
            CommonType = Left.Type;

            if (Right is RegisterExpression && Left is RegisterExpression)
                RegisterOperation = true;
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " ^ " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new Xor() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }
    }
    [Terminal("&")]
    public class BitwiseAndOperator : BinaryOp
    {
        public BitwiseAndOperator()
        {
            Operator = BinaryOperator.BitwiseAnd;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
        
            if (!FixConstant(rc))
                ResolveContext.Report.Error(22, Location, "Bitwise operation must have the same type");
            CommonType = Left.Type;

            if (Right is RegisterExpression && Left is RegisterExpression)
                RegisterOperation = true;
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " & " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new And() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }

    }

    [Terminal("==")]
    public class EqualOperator : BinaryOp
    {
        public EqualOperator()
        {
            Operator = BinaryOperator.Equality;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);
         
            ec.EmitInstruction(new Compare() { DestinationReg =LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value) ,ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase,bool v)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
          

            // jumps
            if(v)
            ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.Equal, DestinationLabel = truecase.Name });
            else
               ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotEqual, DestinationLabel = truecase.Name });
       

            return true;
        }
    }
    [Terminal("!=")]
    public class NotEqualOperator : BinaryOp
    {
        public NotEqualOperator()
        {
            Operator = BinaryOperator.Inequality;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            return this;
        }

        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " != " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.NotEqual, ConditionalTestEnum.Equal);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " != " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps
            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotEqual, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.Equal, DestinationLabel = truecase.Name });


            return true;
        }
    }
    [Terminal("<")]
    public class LessThanOperator : BinaryOp
    {
        public LessThanOperator()
        {
            Operator = BinaryOperator.LessThan;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " < " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.LessThan, ConditionalTestEnum.NotLessThan);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " < " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps
            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.LessThan, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotLessThan, DestinationLabel = truecase.Name });


            return true;
        }
    }
    [Terminal(">")]
    public class GreaterThanOperator : BinaryOp
    {
        public GreaterThanOperator()
        {
            Operator = BinaryOperator.GreaterThan;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            return this;
        }

        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " > " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.GreaterThan, ConditionalTestEnum.NotGreaterThan);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " > " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps
            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.GreaterThan, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotGreaterThan, DestinationLabel = truecase.Name });


            return true;
        }
    }
    [Terminal("<=")]
    public class LessThanOrEqualOperator : BinaryOp
    {
        public LessThanOrEqualOperator()
        {
            Operator = BinaryOperator.LessThanOrEqual;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            return this;
        }

        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.LessThanOrEqualTo, ConditionalTestEnum.NotGreaterThanOrEqualTo);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps
            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.LessThanOrEqualTo, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotLessThanOrEqualTo, DestinationLabel = truecase.Name });


            return true;
        }
    }
    [Terminal(">=")]
    public class GreaterThanOrEqualOperator : BinaryOp
    {
        public GreaterThanOrEqualOperator()
        {
            Operator = BinaryOperator.GreaterThanOrEqual;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");

            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            return this;
        }

        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " >= " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.GreaterThanOrEqualTo, ConditionalTestEnum.NotGreaterThanOrEqualTo);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " >= " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps
            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.GreaterThanOrEqualTo, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.NotGreaterThanOrEqualTo, DestinationLabel = truecase.Name });


            return true;
        }
    }
   


    [Terminal("<<")]
    public class LeftShiftOperator : BinaryOp
    {
        public LeftShiftOperator()
        {
            Operator = BinaryOperator.LeftShift;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        public uint ShiftValue { get; set; }
        bool noshift = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                ShiftValue = uint.Parse((Right as ConstantExpression).GetValue().ToString());


            if (ShiftValue > 15)
                noshift = true;

            if (Left is RegisterExpression)
                RegisterOperation = true;
          
            return this;
        }
        public override bool Emit(EmitContext ec)
        {


            if (RegisterOperation)
            {
                RegisterExpression.EmitOperation(ec, new ShiftLeft(), ShiftValue, ((RegisterExpression)Left).Register);
                return true;
            }
            if (noshift)
            {
                ec.EmitInstruction(new Mov() { SourceValue = 0, DestinationReg = LeftRegister.Value, Size = 16 });
                ec.EmitPush(LeftRegister.Value);
                return true;

            }
            Left.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " << " + ShiftValue);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new ShiftLeft() { DestinationReg = LeftRegister.Value, SourceValue = ShiftValue, Size = 80 });
            ec.EmitPush(LeftRegister.Value);
    
            return true;
        }

    }
    [Terminal(">>")]
    public class RightShiftOperator : BinaryOp
    {
        public RightShiftOperator()
        {
            Operator = BinaryOperator.RightShift;
            LeftRegister = RegistersEnum.AX;
    
        }
        public uint ShiftValue { get; set; }
        bool noshift = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;
            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                ShiftValue = uint.Parse((Right as ConstantExpression).GetValue().ToString());


            if (ShiftValue > 15)
                noshift = true;

            if (Left is RegisterExpression)
                RegisterOperation = true;
          
            return this;
        }
        public override bool Emit(EmitContext ec)
        {


            if (RegisterOperation)
            {
                RegisterExpression.EmitOperation(ec, new ShiftRight(), ShiftValue, ((RegisterExpression)Left).Register);
                return true;
            }
            if (noshift)
            {
                ec.EmitInstruction(new Mov() { SourceValue = 0, DestinationReg = LeftRegister.Value, Size = 16 });
                ec.EmitPush(LeftRegister.Value);
                return true;

            }
            Left.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " >> " + ShiftValue);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new ShiftRight() { DestinationReg = LeftRegister.Value, SourceValue = ShiftValue, Size = 80 });
            ec.EmitPush(LeftRegister.Value);

            return true;
        }
    

    }
    [Terminal("<~")]
    public class LeftRotateOperator : BinaryOp
    {
        public LeftRotateOperator()
        {
            Operator = BinaryOperator.LeftRotate;
            LeftRegister = RegistersEnum.AX;
       
        }
        public uint RotValue { get; set; }
        bool norot = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                RotValue = uint.Parse((Right as ConstantExpression).GetValue().ToString());

            RotValue = RotValue % 16;

      
                norot = (RotValue == 0);

            if (Left is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                RegisterExpression.EmitOperation(ec, new RotateLeft(), RotValue, ((RegisterExpression)Left).Register);
                return true;
            }
            if (norot)
            {
                Left.EmitToStack(ec);
                return true;
            }
            Left.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " <~ " + RotValue);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new RotateLeft() { DestinationReg = LeftRegister.Value, SourceValue = RotValue, Size = 80 });
            ec.EmitPush(LeftRegister.Value);
    
         
            return true;
        }

    }
    [Terminal("~>")]
    public class RightRotateOperator : BinaryOp
    {
        public RightRotateOperator()
        {
            Operator = BinaryOperator.RightRotate;
            LeftRegister = RegistersEnum.AX;

        }
        public uint RotValue { get; set; }
        bool norot = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                RotValue = uint.Parse((Right as ConstantExpression).GetValue().ToString());

            RotValue = RotValue % 16;


            norot = (RotValue == 0);

            if (Left is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                RegisterExpression.EmitOperation(ec, new RotateRight(), RotValue, ((RegisterExpression)Left).Register);
                return true;
            }
            if (norot)
            {
                Left.EmitToStack(ec);
                return true;
            }
            Left.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " ~> " + RotValue);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new RotateRight() { DestinationReg = LeftRegister.Value, SourceValue = RotValue, Size = 80 });
            ec.EmitPush(LeftRegister.Value);


            return true;
        }

    }


    [Terminal("+")]
    public class AdditionOperator : BinaryOp
    {
        public AdditionOperator()
        {
            Operator = BinaryOperator.Addition;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");
            
        

            CommonType = Left.Type;
            if (Right is RegisterExpression && Left is RegisterExpression)
                RegisterOperation = true;
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {


            if (RegisterOperation)
            {
                RegisterExpression.EmitOperation(ec, new Add(), ((RegisterExpression)Right).Register, ((RegisterExpression)Left).Register);
                return true;
            }

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " + " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new Add() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);
         
            return true;
        }


    }
    [Terminal("-")]
    public class SubtractionOperator : BinaryOp
    {
        public SubtractionOperator()
        {
            Operator = BinaryOperator.Subtraction;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");

            CommonType = Left.Type;
            if (Right is RegisterExpression && Left is RegisterExpression)
                RegisterOperation = true;
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {


            if (RegisterOperation)
            {
                RegisterExpression.EmitOperation(ec, new Add(), ((RegisterExpression)Right).Register, ((RegisterExpression)Left).Register);
                return true;
            }

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " - " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
      

            ec.EmitInstruction(new Sub() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);

            return true;
        }

    }

    [Terminal("*")]
    public class MultiplyOperator : BinaryOp
    {
        public MultiplyOperator()
        {
            Operator = BinaryOperator.Multiply;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        bool bytemul = false;
        bool unsigned = true;

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bytemul = !(Right.Type.SizeInBits > 8 || Left.Type.SizeInBits > 8);
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");

            CommonType = Left.Type;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " * " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            // TODO:CHECKED MUL
            if (unsigned)
            {
                if (bytemul)
                    ec.EmitInstruction(new Multiply() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });
                else ec.EmitInstruction(new Multiply() { DestinationReg = RightRegister.Value, Size = 80 });
            }
            else
            {
                if (bytemul)
                    ec.EmitInstruction(new SignedMultiply() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });
                else ec.EmitInstruction(new SignedMultiply() { DestinationReg =RightRegister.Value, Size = 80 });
            }
            if (bytemul)
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = LeftRegister.Value, SourceReg = RegistersEnum.AL, Size = 80 });

            ec.EmitPush(LeftRegister.Value);
      
            return true;
        }
    }
    [Terminal("/")]
    public class DivisionOperator : BinaryOp
    {
        public DivisionOperator()
        {
            Operator = BinaryOperator.Division;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        bool bytemul = false;
        bool unsigned = true;

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bytemul = !(Right.Type.SizeInBits > 8 || Left.Type.SizeInBits > 8);
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");

            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            CommonType = Left.Type;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " / " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            // TODO:CHECKED DIV
            if (unsigned)
            {
                if (bytemul)
                {

                    ec.EmitInstruction(new Divide() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });

                }
                else
                {
                    ec.EmitInstruction(new Divide() { DestinationReg = RightRegister.Value, Size = 80 });

                }
            }
            else
            {
                if (bytemul)
                {
                    ec.EmitInstruction(new SignedDivide() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });

                }
                else
                {
                    ec.EmitInstruction(new SignedDivide() { DestinationReg = RightRegister.Value, Size = 80 });

                }
            }
            if (bytemul)
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = LeftRegister.Value, SourceReg = RegistersEnum.AL, Size = 80 });

            ec.EmitPush(LeftRegister.Value);
          
            return true;
        }
    }
    [Terminal("%")]
    public class ModulusOperator : BinaryOp
    {
        public ModulusOperator()
        {
            Operator = BinaryOperator.Modulus;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        bool bytemul = false;
        bool unsigned = true;

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bytemul = !(Right.Type.SizeInBits > 8 || Left.Type.SizeInBits > 8);
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            CommonType = Left.Type;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " % " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            // TODO:CHECKED DIV
            if (unsigned)
            {
                if (bytemul)
                {
                    ec.EmitInstruction(new Divide() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });

                }
                else
                {
                    ec.EmitInstruction(new Divide() { DestinationReg = RightRegister.Value, Size = 80 });

                }


            }
            else
            {
                if (bytemul)
                {
                    ec.EmitInstruction(new SignedDivide() { DestinationReg = ec.GetLow(RightRegister.Value), Size = 80 });

                }
                else
                {
                    ec.EmitInstruction(new SignedDivide() { DestinationReg = RightRegister.Value, Size = 80 });

                }

            }
            if (bytemul)
                ec.EmitInstruction(new MoveZeroExtend() { DestinationReg = LeftRegister.Value, SourceReg = RegistersEnum.AH, Size = 80 });
            else
                ec.EmitInstruction(new Mov() { DestinationReg =  LeftRegister.Value, SourceReg = RegistersEnum.DX, Size = 80 });
            ec.EmitPush(LeftRegister.Value);
        
            return true;
        }
    }
    #endregion
}