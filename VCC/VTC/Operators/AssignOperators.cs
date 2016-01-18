using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    #region Assign Operators


    [Terminal("=")]
    public class SimpleAssignOperator : AssignOp
    {
        bool IsDelegateMethodAssign = false;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
         /*  if (Left.Type.IsStruct)
                ResolveContext.Report.Error(44, Location, "Cannot assign struct values");*/
            IsDelegateMethodAssign = (Left.Type.IsDelegate && Right is MethodExpression);
          

            if(Left.Type.IsEnum)
                ResolveContext.Report.Error(44, Location, "Cannot assign enum values outside it's declaration");

            if (!TypeChecker.CompatibleTypes(Left.Type,Right.Type))
                ResolveContext.Report.Error(35, Location, "Source and target must have same types");



            return this;
        }
        public override bool Emit(EmitContext ec)
        {


            if (!IsDelegateMethodAssign)
                Right.EmitToStack(ec);
            else
                ec.EmitInstruction(new Push() { DestinationRef =  Vasm.ElementReference.New((Right as MethodExpression).Method.Signature.ToString()), Size = 16 });
      

            ec.EmitComment(Left.CommentString() + Name + Right.CommentString());
            Left.EmitFromStack(ec);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return Left.EmitFromStack(ec);
        }
    }

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
                RegisterExpression.EmitOperation(ec, new Xchg(), ((RegisterExpression)Right).Register, ((RegisterExpression)Left).Register,false);
             
                return true;
            }

            Right.EmitToStack(ec);
            Left.EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            ec.EmitPop(EmitContext.B);
            ec.EmitComment(Left.CommentString() + "<>" + Right.CommentString());
            ec.EmitInstruction(new Xchg() { SourceReg = EmitContext.A, DestinationReg = EmitContext.B, Size = 16});
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



    [Terminal("+=")]
    public class AddAssignOperator : AssignOp
    {
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new AdditionOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + "+" + ((BinaryOperation)Right)._op.Right.CommentString();
        }
    }
    [Terminal("-=")]
    public class SubAssignOperator : AssignOp
    {

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new SubtractionOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + "-" + ((BinaryOperation)Right)._op.Right.CommentString();
        }
    }
    [Terminal("*=")]
    public class MulAssignOperator : AssignOp
    {


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new MultiplyOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + "*" + ((BinaryOperation)Right)._op.Right.CommentString();
        }

    }
    [Terminal("/=")]
    public class DivAssignOperator : AssignOp
    {


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new DivisionOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + "/" + ((BinaryOperation)Right)._op.Right.CommentString();
        }

    }

    [Terminal("^=")]
    public class XorAssignOperator : AssignOp
    {

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new BitwiseXorOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + "^" + ((BinaryOperation)Right)._op.Right.CommentString();
        }
    }
    [Terminal("&=")]
    public class AndAssignOperator : AssignOp
    {

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new BitwiseAndOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + "&" + ((BinaryOperation)Right)._op.Right.CommentString();
        }

    }
    [Terminal("|=")]
    public class OrAssignOperator : AssignOp
    {


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new BitwiseOrOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
    [Terminal(">>=")]
    public class RightShiftAssignOperator : AssignOp
    {


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new RightShiftOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + ">>" + ((BinaryOperation)Right)._op.Right.CommentString();
        }

    }
    [Terminal("<<=")]
    public class LeftShiftAssignOperator : AssignOp
    {

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Right = (Expr)Right.DoResolve(rc);
            _op = new LeftShiftOperator();
            Right = new BinaryOperation(Left, _op, Right);
            Right = (Expr)Right.DoResolve(rc);
            return base.DoResolve(rc);
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
            return Left.CommentString() + " = " + Left.CommentString() + "<<" + ((BinaryOperation)Right)._op.Right.CommentString();
        }


    }
    #endregion
}
