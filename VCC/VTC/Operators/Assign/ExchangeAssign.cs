using VTC.Base.GoldParser.Semantic;
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


            Right.EmitToStack(ec);
            Left.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + "<>" + Right.CommentString());
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
