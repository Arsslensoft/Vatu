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
    // TODO:ADD OPTIMIZE CONSTANT EVALUATION
    #region Binary Operators
    public class ExtendedBinaryOperator : BinaryOp
    {
        public string SymbolName { get; set; }
 
        public ExtendedBinaryOperator(bsn.GoldParser.Grammar.Symbol l,string name)
        {
            SymbolName = name;
            symbol = l;
            Operator = BinaryOperator.UserDefine;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
          

            CommonType = Left.Type;
            if (Right is RegisterExpression && Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Registers cannot be used with this kind of operators");
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");

            OperatorSpec oper = rc.Resolver.TryResolveOperator(SymbolName);
            if (oper == null)
                ResolveContext.Report.Error(0, Location, "Unknown operator");
            else
            {
                if (oper.IsLogic)
                    CommonType = BuiltinTypeSpec.Bool;

                OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + oper.Name, new TypeSpec[2] { Left.Type, Right.Type });
                if (rc.CurrentMethod == OvlrdOp)
                    OvlrdOp = null;
                else if(OvlrdOp == null)
                    ResolveContext.Report.Error(0, Location, "No operator overloading for this operator");
            }
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            else return false;
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
                return base.EmitOverrideOperatorBranchable(ec, truecase,v, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            else return false;
        }

    }



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
            if (CommonType.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceValue = EmitContext.TRUE, Size = 80 });
            else
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
            RightRegister = RegistersEnum.BX;
            LeftRegister = RegistersEnum.AX;
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

            if (CommonType.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceValue = EmitContext.TRUE, Size = 80 });
            else
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

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

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

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
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
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

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

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] {Left.Type, Right.Type});
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

          
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

            ec.EmitBoolean(ec.GetLow(LeftRegister.Value) ,ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);


            ec.EmitPush(LeftRegister.Value);


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase,bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec,truecase,v, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " == " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);

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
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }

        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " != " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            if (Left.Type.Size == 1)
               ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg =ec.GetLow( RightRegister.Value), Size = 80 });
            else
               ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.NotEqual, ConditionalTestEnum.Equal);


            ec.EmitPush(LeftRegister.Value);


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.NotEqual, ConditionalTestEnum.Equal);

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " != " + Right.CommentString());
            ec.EmitPop(LeftRegister.Value);
            ec.EmitPop(RightRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

            // jumps

            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.NotEqual, ConditionalTestEnum.Equal);
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

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;

            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " < " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });

       
            if(unsigned)
                ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.Below, ConditionalTestEnum.NotBelow);
            else
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.LessThan, ConditionalTestEnum.NotLessThan);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.LessThan, ConditionalTestEnum.NotLessThan);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " < " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);


            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps

            if (unsigned)
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Below, ConditionalTestEnum.NotBelow);
            else
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.LessThan, ConditionalTestEnum.NotLessThan);
         

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
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }

        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " > " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            if (unsigned)
                ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.Above, ConditionalTestEnum.NotAbove);
            else
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.GreaterThan, ConditionalTestEnum.NotGreaterThan);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.GreaterThan, ConditionalTestEnum.NotGreaterThan);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " > " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps
            if (unsigned)
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Above, ConditionalTestEnum.NotAbove);
            else
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.GreaterThan, ConditionalTestEnum.NotGreaterThan);
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
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);

            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");
            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }

        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            if (unsigned)
                ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.BelowOrEqual, ConditionalTestEnum.NotBelowOrEqual);
            else
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.LessThanOrEqualTo, ConditionalTestEnum.NotGreaterThanOrEqualTo);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.LessThanOrEqualTo, ConditionalTestEnum.NotLessThanOrEqualTo);

            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " <= " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });


            // jumps
            if (unsigned)
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.BelowOrEqual, ConditionalTestEnum.NotBelowOrEqual);
            else
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.LessThanOrEqualTo,ConditionalTestEnum.NotLessThanOrEqualTo);

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
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(24, Location, "Comparison operation must have the same type");

            CommonType = BuiltinTypeSpec.Bool;
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }

        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " >= " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 80 });
            if (unsigned)
                ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.AboveOrEqual, ConditionalTestEnum.NotAboveOrEqual);
            else
            ec.EmitBoolean(ec.GetLow(LeftRegister.Value), ConditionalTestEnum.GreaterThanOrEqualTo, ConditionalTestEnum.NotGreaterThanOrEqualTo);


            ec.EmitPush(ec.GetLow(LeftRegister.Value));


            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.GreaterThanOrEqualTo, ConditionalTestEnum.NotGreaterThanOrEqualTo);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " >= " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            if (Left.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceReg = ec.GetLow(RightRegister.Value), Size = 8 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceReg = RightRegister.Value, Size = 16 });


            // jumps
            if (unsigned)
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.AboveOrEqual, ConditionalTestEnum.NotAboveOrEqual);
            else
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.GreaterThanOrEqualTo, ConditionalTestEnum.NotGreaterThanOrEqualTo);

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
        public ushort ShiftValue { get; set; }
        bool noshift = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                ShiftValue = ushort.Parse((Right as ConstantExpression).GetValue().ToString());


            if (ShiftValue > 15)
                noshift = true;

            if (Left is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

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
        public ushort ShiftValue { get; set; }
        bool noshift = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;
            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                ShiftValue = ushort.Parse((Right as ConstantExpression).GetValue().ToString());


            if (ShiftValue > 15)
                noshift = true;

            if (Left is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

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
        public ushort RotValue { get; set; }
        bool norot = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                RotValue = ushort.Parse((Right as ConstantExpression).GetValue().ToString());

            RotValue = (ushort)(RotValue % 16);

      
                norot = (RotValue == 0);

            if (Left is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
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
        public ushort RotValue { get; set; }
        bool norot = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Shift operations must have the same types (use cast)");
            CommonType = Left.Type;

            if (!(Right is ConstantExpression))
                ResolveContext.Report.Error(31, Location, "Right side must be constant value in shift operations");
            else
                RotValue = ushort.Parse((Right as ConstantExpression).GetValue().ToString());

            RotValue = (ushort)(RotValue % 16);


            norot = (RotValue == 0);

            if (Left is RegisterExpression)
                RegisterOperation = true;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
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

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

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

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

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

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

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
   

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bytemul = !(Right.Type.SizeInBits > 8 || Left.Type.SizeInBits > 8);
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");

            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            CommonType = Left.Type;

            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " / " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new Xor() { DestinationReg = EmitContext.D, SourceReg = EmitContext.D, Size = 80 });
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


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bytemul = !(Right.Type.SizeInBits > 8 || Left.Type.SizeInBits > 8);
            unsigned = (Right.Type.IsUnsigned || Left.Type.IsUnsigned);
            if (!FixConstant(rc))
                ResolveContext.Report.Error(23, Location, "Arithmetic operations must have the same types (use cast)");
            if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(29, Location, "Registers are not allowed for this operation");
            CommonType = Left.Type;
            OvlrdOp = rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), new TypeSpec[2] { Left.Type, Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " % " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);
            ec.EmitInstruction(new Xor() { DestinationReg = EmitContext.D,SourceReg = EmitContext.D, Size = 80 });
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
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec) ;
        }
    }
    #endregion
}
