using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VTC.Core
{
    /// <summary>
    /// Handle all binary operators
    /// </summary>
    public class BinaryOperation : Expr
    {

        public BinaryOp _op;
        public bool IsConstant { get; set; }




        [Rule(@"<Op Or>      ::= <Op Or> '||' <Op And>")]
        [Rule(@"<Op And>     ::= <Op And> '&&' <Op BinOR>")]
        [Rule(@"<Op BinOR>   ::= <Op BinOR> '|' <Op BinXOR>")]
        [Rule(@"<Op BinXOR>  ::= <Op BinXOR> '^' <Op BinAND>")]
        [Rule(@"<Op BinAND>  ::= <Op BinAND> '&' <Op Equate>")]
        [Rule(@"<Op Equate>  ::= <Op Equate> '==' <Op Compare>")]
        [Rule(@"<Op Equate>  ::= <Op Equate> '!=' <Op Compare>")]

        [Rule(@"<Op Compare> ::= <Op Compare> '<'  <Op Shift>")]
        [Rule(@"<Op Compare> ::= <Op Compare> '>'  <Op Shift>")]
        [Rule(@"<Op Compare> ::= <Op Compare> '<=' <Op Shift>")]
        [Rule(@"<Op Compare> ::= <Op Compare> '>=' <Op Shift>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '<<' <Op Add>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '>>' <Op Add>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '<~' <Op Add>")]
        [Rule(@"<Op Shift>   ::= <Op Shift> '~>' <Op Add>")]
        [Rule(@"<Op Add>     ::= <Op Add> '+' <Op Mult>")]
        [Rule(@"<Op Add>     ::= <Op Add> '-' <Op Mult>")]
        [Rule(@"<Op Mult>    ::= <Op Mult> '*' <Op Unary>")]
        [Rule(@"<Op Mult>    ::= <Op Mult> '/' <Op Unary>")]
        [Rule(@"<Op Mult>    ::= <Op Mult> '%' <Op Unary>")]
        public BinaryOperation(Expr left, BinaryOp op, Expr right)
        {
            _op = op;
            _op.Left = left;
            IsConstant = false;
            _op.Right = right;
        }
        [Rule(@"<Op Equate>  ::= <Op Equate> is <Type>")]
        public BinaryOperation(Expr left, BinaryOp op, TypeToken right)
        {
            _op = op;
            _op.Left = left;
            _op.RightType = right;
            IsConstant = false;
   
        }
        [Rule(@"<Op BinaryOpDef>     ::= <Op BinaryOpDef> OperatorLiteralBinary <Op Or>")]
        public BinaryOperation(Expr left, OperatorLiteralBinary op, Expr right)
        {
            _op = new ExtendedBinaryOperator(op.Sym, op.Value.GetValue().ToString());

            _op.Left = left;
            IsConstant = false;
            _op.Right = right;
        }
        byte GetValueAsByte(Expr rexp, Expr lexp)
        {
            ByteConstant lce = ((ByteConstant)lexp);
            ByteConstant rce = ((ByteConstant)rexp);


            if (_op is AdditionOperator)
                return (byte)(lce._value + rce._value);
            else if (_op is SubtractionOperator)
                return (byte)(lce._value - rce._value);
            else if (_op is MultiplyOperator)
                return (byte)(lce._value * rce._value);
            else if (_op is DivisionOperator)
                return (byte)(lce._value / rce._value);
            else if (_op is ModulusOperator)
                return (byte)(lce._value % rce._value);
            else if (_op is BitwiseAndOperator)
                return (byte)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (byte)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (byte)(lce._value ^ rce._value);
            else if (_op is LeftShiftOperator)
                return (byte)(lce._value << rce._value);
            else if (_op is RightShiftOperator)
                return (byte)(lce._value >> rce._value);

            else if (_op is LeftRotateOperator)
                return (byte)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
            else if (_op is RightShiftOperator)
                return (byte)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

            throw new Exception("Failed");

        }
        sbyte GetValueAsSByte(Expr rexp, Expr lexp)
        {
            SByteConstant lce = ((SByteConstant)lexp);
            SByteConstant rce = ((SByteConstant)rexp);


            if (_op is AdditionOperator)
                return (sbyte)(lce._value + rce._value);
            else if (_op is SubtractionOperator)
                return (sbyte)(lce._value - rce._value);
            else if (_op is MultiplyOperator)
                return (sbyte)(lce._value * rce._value);
            else if (_op is DivisionOperator)
                return (sbyte)(lce._value / rce._value);
            else if (_op is ModulusOperator)
                return (sbyte)(lce._value % rce._value);
            else if (_op is BitwiseAndOperator)
                return (sbyte)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (sbyte)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (sbyte)(lce._value ^ rce._value);
            else if (_op is LeftShiftOperator)
                return (sbyte)(lce._value << rce._value);
            else if (_op is RightShiftOperator)
                return (sbyte)(lce._value >> rce._value);

            else if (_op is LeftRotateOperator)
                return (sbyte)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
            else if (_op is RightShiftOperator)
                return (sbyte)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

            throw new Exception("Failed");

        }
        short GetValueAsInt(Expr rexp, Expr lexp)
        {
            IntConstant lce = ((IntConstant)lexp);
            IntConstant rce = ((IntConstant)rexp);


            if (_op is AdditionOperator)
                return (short)(lce._value + rce._value);
            else if (_op is SubtractionOperator)
                return (short)(lce._value - rce._value);
            else if (_op is MultiplyOperator)
                return (short)(lce._value * rce._value);
            else if (_op is DivisionOperator)
                return (short)(lce._value / rce._value);
            else if (_op is ModulusOperator)
                return (short)(lce._value % rce._value);
            else if (_op is BitwiseAndOperator)
                return (short)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (short)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (short)(lce._value ^ rce._value);
            else if (_op is LeftShiftOperator)
                return (short)(lce._value << rce._value);
            else if (_op is RightShiftOperator)
                return (short)(lce._value >> rce._value);

            else if (_op is LeftRotateOperator)
                return (short)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
            else if (_op is RightShiftOperator)
                return (short)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

            throw new Exception("Failed");

        }
        ushort GetValueAsUInt(Expr rexp, Expr lexp)
        {
            UIntConstant lce = ((UIntConstant)lexp);
            UIntConstant rce = ((UIntConstant)rexp);


            if (_op is AdditionOperator)
                return (ushort)(lce._value + rce._value);
            else if (_op is SubtractionOperator)
                return (ushort)(lce._value - rce._value);
            else if (_op is MultiplyOperator)
                return (ushort)(lce._value * rce._value);
            else if (_op is DivisionOperator)
                return (ushort)(lce._value / rce._value);
            else if (_op is ModulusOperator)
                return (ushort)(lce._value % rce._value);
            else if (_op is BitwiseAndOperator)
                return (ushort)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (ushort)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (ushort)(lce._value ^ rce._value);
            else if (_op is LeftShiftOperator)
                return (ushort)(lce._value << rce._value);
            else if (_op is RightShiftOperator)
                return (ushort)(lce._value >> rce._value);

            else if (_op is LeftRotateOperator)
                return (ushort)((lce._value << rce._value) | (lce._value >> 8 - rce._value));
            else if (_op is RightShiftOperator)
                return (ushort)((lce._value >> rce._value) | (lce._value << 8 - rce._value));

            throw new Exception("Failed");

        }
        bool GetValueAsBool(Expr rexp, Expr lexp)
        {
            BoolConstant lce = ((BoolConstant)lexp);
            BoolConstant rce = ((BoolConstant)rexp);


            if (_op is LogicalAndOperator)
                return (bool)(lce._value && rce._value);
            else if (_op is LogicalOrOperator)
                return (bool)(lce._value || rce._value);

            else if (_op is BitwiseAndOperator)
                return (bool)(lce._value & rce._value);
            else if (_op is BitwiseOrOperator)
                return (bool)(lce._value | rce._value);
            else if (_op is BitwiseXorOperator)
                return (bool)(lce._value ^ rce._value);


            throw new Exception("Failed");

        }

        bool CompareExpr(Expr rexp, Expr lexp)
        {
            if (rexp is ByteConstant)
                return EvalueComparison((lexp as ByteConstant)._value, (rexp as ByteConstant)._value);
            else if (rexp is SByteConstant)
                return EvalueComparison((lexp as SByteConstant)._value, (rexp as SByteConstant)._value);
            else if (rexp is IntConstant)
                return EvalueComparison((lexp as IntConstant)._value, (rexp as IntConstant)._value);
            else if (rexp is UIntConstant)
                return EvalueComparison((lexp as UIntConstant)._value, (rexp as UIntConstant)._value);
            else if (rexp is UIntConstant)
                return EvalueComparison((lexp as UIntConstant)._value, (rexp as UIntConstant)._value);
            else if (rexp is BoolConstant)
                return EvalueComparison((lexp as BoolConstant)._value, (rexp as BoolConstant)._value);
            else throw new Exception("Failed");
        }

        bool EvalueComparison(byte lv, byte rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(sbyte lv, sbyte rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(int lv, int rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(uint lv, uint rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv > rv;
            else if (_op is LessThanOperator)
                return lv < rv;
            else if (_op is GreaterThanOrEqualOperator)
                return lv >= rv;
            else if (_op is LessThanOrEqualOperator)
                return lv <= rv;
            else return false;
        }
        bool EvalueComparison(bool lv, bool rv)
        {
            if (_op is EqualOperator)
                return lv == rv;
            else if (_op is NotEqualOperator)
                return lv != rv;
            else if (_op is GreaterThanOperator)
                return lv == true && rv == false;
            else if (_op is LessThanOperator)
                return lv == false && rv == true;
            else if (_op is GreaterThanOrEqualOperator)
                return lv == true;
            else if (_op is LessThanOrEqualOperator)
                return lv == false;
            else return false;
        }

        Expr TryEvaluate()
        {
            try
            {
                if (_op is EqualOperator || _op is NotEqualOperator || _op is GreaterThanOperator || _op is LessThanOperator || _op is GreaterThanOrEqualOperator || _op is LessThanOrEqualOperator)
                    return new BoolConstant(CompareExpr(_op.Right, _op.Left), Location);
                else
                {
                    if (_op.Left is ByteConstant)
                        return new ByteConstant(GetValueAsByte(_op.Right, _op.Left), Location);
                    else if (_op.Left is SByteConstant)
                        return new SByteConstant(GetValueAsSByte(_op.Right, _op.Left), Location);
                    else if (_op.Left is IntConstant)
                        return new IntConstant(GetValueAsInt(_op.Right, _op.Left), Location);
                    else if (_op.Left is UIntConstant)
                        return new UIntConstant(GetValueAsUInt(_op.Right, _op.Left), Location);
                    else if (_op.Left is BoolConstant)
                        return new BoolConstant(GetValueAsBool(_op.Right, _op.Left), Location);
                    else return this;
                }
            }
            catch
            {
                return this;
            }

        }
        bool requireoverload = false;
       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _op.Left.Resolve(rc);
            ok &= _op.Right.Resolve(rc);

            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            Expr tmp;
            if (_op.Right != null)
                _op.Right = (Expr)_op.Right.DoResolve(rc);
            else _op.RightType = (TypeToken)_op.RightType.DoResolve(rc);


            _op.Left = (Expr)_op.Left.DoResolve(rc);
            if (_op.Right != null)
            {
                if (!TypeChecker.ArtihmeticsAllowed(_op.Right.Type, _op.Left.Type))
                    requireoverload = true;

                // check for const
                if (_op.Right is ConstantExpression && _op.Left is ConstantExpression)
                {
                    IsConstant = true;
                    // try calculate
                    tmp = TryEvaluate();
                    if (tmp != this)
                        return tmp;
                }
            }

            // end check const
            _op = (BinaryOp)_op.DoResolve(rc);
            if (requireoverload && _op.OvlrdOp == null)
                ResolveContext.Report.Error(46, Location, "Binary operations are not allowed for this type");
            Type = _op.CommonType;
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            FlowState ok = _op.Left.DoFlowAnalysis(fc);
            if (_op.Right != null)
                ok &= _op.Right.DoFlowAnalysis(fc);

            return ok & base.DoFlowAnalysis(fc);
        }
       
        public override bool Emit(EmitContext ec)
        {
            return _op.Emit(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            Emit(ec);
            return true;
        }

        public override string CommentString()
        {
            if(_op.Right != null)
            return _op.Left.CommentString() + _op.CommentString() + _op.Right.CommentString();
            else return _op.Left.CommentString() + "is" + _op.RightType.Name ;
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return _op.EmitBranchable(ec, truecase, v);
        }

    }
}
