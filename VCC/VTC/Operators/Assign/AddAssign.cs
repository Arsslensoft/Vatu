using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{


    [Terminal("+=")]
    public class AddAssignOperator : AssignOp
    {
        bool IsDelegateMethodAssign = false;
        MethodSpec DelegateMethod;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            IsDelegateMethodAssign = (Left.Type.IsDelegate && Right is DeclaredExpression && ((Right as DeclaredExpression).Expression is VariableExpression));
            if (!IsDelegateMethodAssign)
            {
                Right = (Expr)Right.DoResolve(rc);
                _op = new AdditionOperator();
                Right = new BinaryOperation(Left, _op, Right);
                Right = (Expr)Right.DoResolve(rc);
            }
            else
            {
                Left = (Expr)Left.DoResolve(rc);
                rc.Resolver.TryResolveMethod(((Right as DeclaredExpression).Expression as VariableExpression).Name, ref DelegateMethod, (Left.Type as DelegateTypeSpec).Parameters.ToArray());
                if (DelegateMethod == null)
                    ResolveContext.Report.Error(0, Location, "Unresolved delegate method");
            }
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (IsDelegateMethodAssign)
                fc.MarkAsUsed(DelegateMethod);
            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            if (IsDelegateMethodAssign)
            {
                ec.EmitInstruction(new Push() { DestinationRef = Vasm.ElementReference.New(DelegateMethod.Signature.ToString()), Size = 16 });
                return Left.EmitFromStack(ec);
               
            }
            ec.EmitComment(CommentString());
            Right.Emit(ec);
            Left.EmitFromStack(ec);
            return true;
        }

        public override string CommentString()
        {
            return Left.CommentString() + " = " + Left.CommentString() + "+" + ((BinaryOperation)Right)._op.Right.CommentString();
        }
    }
}
