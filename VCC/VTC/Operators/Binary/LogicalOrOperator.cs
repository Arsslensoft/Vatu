using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
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
            ec.EmitPop(LeftRegister.Value);
            Label avoid = ec.DefineLabel(LabelType.BOOL_EXPR, "LOR");
            ec.EmitInstruction(new Compare() { SourceValue = EmitContext.TRUE, DestinationReg = LeftRegister.Value });
            ec.EmitPush(LeftRegister.Value);
            ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.Equal, DestinationLabel = avoid.Name });
     


            Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + " || " + Right.CommentString());
            ec.EmitPop(RightRegister.Value);
            ec.EmitPop(LeftRegister.Value);

            ec.EmitInstruction(new Or() { DestinationReg = LeftRegister.Value, SourceReg =RightRegister.Value, Size = 80 });
            ec.EmitPush(LeftRegister.Value);
            ec.MarkLabel(avoid); // keeps left in the stack with false value

       
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


            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);

            return true;
        }
    }
    
	
}