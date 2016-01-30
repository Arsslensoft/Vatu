using VTC.Base.GoldParser.Parser;
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
	[Terminal("!")]
    public class LogicalNotOperator : UnaryOp
    {
        public LogicalNotOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.LogicalNot;
        }
        AssignExpression ae;
       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(26, Location, "Logical not must be used with boolean type, use ~ instead");

            CommonType = BuiltinTypeSpec.Bool;

            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
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
    
	
}