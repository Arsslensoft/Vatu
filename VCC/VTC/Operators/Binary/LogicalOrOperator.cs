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
    
	
}