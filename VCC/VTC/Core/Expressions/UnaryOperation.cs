﻿using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VTC.Core
{
    /// <summary>
    /// Handle all unary operators
    /// </summary>
    public class UnaryOperation : Expr
    {

        private Operator _op;

        [Rule(@"<Op Unary>   ::= '!'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '~'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '-'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '*'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '&'    <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '--'   <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '++'   <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '??'   <Op Unary>")]
        [Rule(@"<Op Unary>   ::= '¤'   <Op Unary>")]
        public UnaryOperation(Operator op, Expr target)
        {
            _op = op;

            if (op is BitwiseAndOperator)
                _op = new LoadEffectiveAddressOp();
            else if (op is MultiplyOperator)
                _op = new ValueOfOp();
            _op.Right = target;
        }
        [Rule(@"<Op Unary>   ::= OperatorLiteralUnary   <Op Unary>")]
        public UnaryOperation(OperatorLiteralUnary op, Expr target)
        {
            _op = new ExtendedUnaryOperator(op.Sym, op.Value.GetValue().ToString());

            _op.Right = target;
        }
        // Postfix
        [Rule(@"<Op Unary>   ::= <Op Pointer> '--'")]
        [Rule(@"<Op Unary>   ::= <Op Pointer> '++'")]
        public UnaryOperation(Expr target, UnaryOp op)
        {

            _op = op;

            _op.Right = target;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            bool adrop = _op is LoadEffectiveAddressOp || _op is ValueOfOp;
            bool idecop = _op is IncrementOperator || _op is DecrementOperator;
            _op.Right = (Expr)_op.Right.DoResolve(rc);
            _op = (Operator)_op.DoResolve(rc);
           AcceptStatement =  (_op is IncrementOperator || _op is DecrementOperator) ;
            if ((idecop && !_op.Right.Type.IsPointer && !_op.Right.Type.IsNumeric) && !adrop && !TypeChecker.ArtihmeticsAllowed(_op.Right.Type, _op.Right.Type))
                ResolveContext.Report.Error(46, Location, "Unary operations are not allowed for this type");
            Type = _op.CommonType;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = _op.Right.Resolve(rc);

            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return _op.Emit(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return _op.EmitToStack(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return _op.EmitFromStack(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return _op.EmitBranchable(ec, truecase, v);
        }

        public override string CommentString()
        {
            return _op.Right.CommentString() + _op.CommentString();
        }

    }
}