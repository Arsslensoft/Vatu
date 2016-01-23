using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{

    [Terminal("<>")]
    public class ExchangeOperator : AssignOp
    {

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type != Left.Type)
                ResolveContext.Report.Error(35, Location, "Source and target must have same types");


            return this;
        }
        public override bool Emit(EmitContext ec)
        {


            if (Right is RegisterExpression && Left is RegisterExpression)
            {
                RegisterExpression.EmitOperation(ec, new Xchg(), ((RegisterExpression)Right).Register, ((RegisterExpression)Left).Register, false);

                return true;
            }

            Right.EmitToStack(ec);
            Left.EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            ec.EmitPop(EmitContext.B);
            ec.EmitComment(Left.CommentString() + "<>" + Right.CommentString());
            ec.EmitInstruction(new Xchg() { SourceReg = EmitContext.A, DestinationReg = EmitContext.B, Size = 16 });
            ec.EmitPush(EmitContext.A);
            ec.EmitPush(EmitContext.B);
            Right.EmitFromStack(ec);
            Left.EmitFromStack(ec);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return Left.EmitFromStack(ec);
        }
    }
}
