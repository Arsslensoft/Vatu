using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{

    /// <summary>
    /// <Op Assign>
    /// </summary>
    public class AssignExpression : Expr
    {

        AssignOp _op;

          


        [Rule(@"<Op Assign>  ::= <Op If> '='   <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '<>'   <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '+='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '-='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '*='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '/='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '^='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '&='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '|='  <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '>>=' <Op Assign>")]
        [Rule(@"<Op Assign>  ::= <Op If> '<<=' <Op Assign>")]
        public AssignExpression(Expr src, AssignOp op, Expr target)
        {

            _op = op;
            _op.Left = src;
            _op.Right = target;
        }


       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _op.Left.Resolve(rc);
            ok &= _op.Right.Resolve(rc);

            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if(!(_op is AddAssignOperator || _op is SubAssignOperator))
                    _op.Right = (Expr)_op.Right.DoResolve(rc);
            _op.Left = (Expr)_op.Left.DoResolve(rc);
            _op = (AssignOp)_op.DoResolve(rc);
            if (!(_op.Left is VariableExpression) && !(_op.Left is AccessOperation) && !(_op.Left is RegisterExpression) && !(_op.Left is UnaryOperation))
                ResolveContext.Report.Error(42, Location, "Target must be a variable");
            else if ((!(_op.Left is AccessOperation)) & !(_op.Left is RegisterExpression) && (!(_op.Left is UnaryOperation)) && (_op.Left as VariableExpression).variable.IsConstant)
                ResolveContext.Report.Error(43, Location, "Cannot assign a constant variable only in it's declaration");

            if ((_op.Left is VariableExpression) && (_op.Left as VariableExpression).variable is PropertySpec && ((_op.Left as VariableExpression).variable as PropertySpec).Setter == null)
                ResolveContext.Report.Error(0, Location, "Target property must have a setter declared");
            Type = _op.Left.Type;
            AcceptStatement = true;
            return this;
        }

     
        public override bool Emit(EmitContext ec)
        {

            ec.EmitComment("Assign expression: " + _op.Left.CommentString() + _op.Name + (_op.Right).CommentString());
            return _op.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return _op.EmitFromStack(ec);
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (_op.Left is VariableExpression && (_op.Left as VariableExpression).variable != null)
                fc.MarkAsAssigned((_op.Left as VariableExpression).variable);
            FlowState fs = _op.Left.DoFlowAnalysis(fc) & _op.Right.DoFlowAnalysis(fc);
            fs &= _op.DoFlowAnalysis(fc);
                
            return fs & base.DoFlowAnalysis(fc);
        }

    }
}
