using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Core;

namespace VTC.Operators.Assign
{
    [Terminal("|=")]
    public class OrAssignOperator : AssignOp
    {


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new BitwiseOrOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment(CommentString());
            Right.Emit(ec);
            Left.EmitFromStack(ec);
            return true;
        }

        public override string CommentString()
        {
            return Left.CommentString() + " = " + Left.CommentString() + "|" + ((BinaryOperation)Right)._op.Right.CommentString();
        }

    }
}
