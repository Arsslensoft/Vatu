﻿using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;


namespace VCC.Core
{
    #region Binary Operators

    [Terminal("||")]
    public class LogicalOrOperator : BinaryOp
    {


    }
    [Terminal("&&")]
    public class LogicalAndOperator : BinaryOp
    {

    }
    [Terminal("|")]
    public class BitwiseOrOperator : BinaryOp
    {

    
    }
    [Terminal("^")]
    public class BitwiseXorOperator : BinaryOp
    {


 
    }
    [Terminal("&")]
    public class BitwiseAndOperator : BinaryOp
    {


     
    }
    [Terminal("==")]
    public class EqualOperator : BinaryOp
    {


    
    }
    [Terminal("!=")]
    public class NotEqualOperator : BinaryOp
    {

      
    }
    [Terminal("<")]
    public class LessThanOperator : BinaryOp
    {

    }
    [Terminal(">")]
    public class GreaterThanOperator : BinaryOp
    {

      
    }
    [Terminal("<=")]
    public class LessThanOrEqualOperator : BinaryOp
    {

      
    }
    [Terminal(">=")]
    public class GreaterThanOrEqualOperator : BinaryOp
    {

      
    }
    [Terminal("<<")]
    public class LeftShiftOperator : BinaryOp
    {

      
    }
    [Terminal(">>")]
    public class RightShiftOperator : BinaryOp
    {

       
    }
    [Terminal("+")]
    public class AdditionOperator : BinaryOp
    {

        public override bool Emit(EmitContext ec)
        {
          
            Left.Emit(ec);
            Right.Emit(ec);
            ec.EmitComment(Left.CommentString()+" + " + Right.CommentString());
            CheckRegister(ec,Left);
            CheckRegister(ec,Right);
            
            ec.EmitInstruction(new Add() { DestinationReg = RegistersEnum.AX, SourceReg = RegistersEnum.CX, Size = 80 });
            ec.EmitPush(RegistersEnum.AX);
            ec.FreeRegister();
            ec.FreeRegister();
            return true;
        }
     
    }

    [Terminal("-")]
    public class SubtractionOperator : BinaryOp
    {
     
      
    }
    [Terminal("*")]
    public class MultiplyOperator : BinaryOp
    {

    }
    [Terminal("/")]
    public class DivisionOperator : BinaryOp
    {

    }
    [Terminal("%")]
    public class ModulusOperator : BinaryOp
    {

    }
    #endregion


    #region Unary Operators
    [Terminal("!")]
    public class LogicalNotOperator : UnaryOp
    {


    }
    [Terminal("~")]
    public class OnesComplementOperator : UnaryOp
    {

    }
    
    [Terminal("--")]
    public class DecrementOperator : UnaryOp
    {

    }
    [Terminal("++")]
    public class IncrementOperator : UnaryOp
    {

 
    }

    // TODO FIX SIZEOF/CAST

    public class CastOperator : Expr
    {
        protected TypeIdentifier _type;
        protected Expr _target;
        [Rule(@"<Op Unary> ::= ~'(' <Type> ~')' <Op Unary>")]
        public CastOperator(TypeIdentifier id,Expr target)
        {
            _target = target;
            _type = id;

        }
    }
    public class SizeOfOperator : Expr
    {
        private TypeIdentifier _type;
        private Identifier _id;
        private TypePointer _ptr;
        [Rule(@"<Op Unary> ::= ~sizeof ~'(' <Type> ~')'")]
        public SizeOfOperator(TypeIdentifier type)
        {
            _type = type;
        }
        [Rule(@"<Op Unary> ::= ~sizeof ~'(' Id <Pointers> ~')'")]
        public SizeOfOperator(Identifier type,TypePointer tp)
        {
            _ptr = tp;
            _id = type;
           
        }
    }
    #endregion

    #region Access Operators
    [Terminal(".")]
    public class ByValueOperator : AccessOp
    {


    }
    [Terminal("->")]
    public class ByAddressOperator : AccessOp
    {


    }
    

    public class ByIndexOperator : AccessOp
    {


    }

#endregion

    #region Assign Operators
    [Terminal("=")]
    public class SimpleAssignOperator : AssignOp
    {

        public override bool Emit(EmitContext ec)
        {
            Right.EmitToStack(ec);

           Left.EmitFromStack(ec);
            return true;
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
            Right.Emit(ec);
            Left.EmitFromStack(ec);
            return true;
        }
    }
    [Terminal("-=")]
    public class SubAssignOperator : AssignOp
    {


    }
    [Terminal("*=")]
    public class MulAssignOperator : AssignOp
    {



    }
    [Terminal("/=")]
    public class DivAssignOperator : AssignOp
    {



    }

    [Terminal("^=")]
    public class XorAssignOperator : AssignOp
    {

    }
    [Terminal("&=")]
    public class AndAssignOperator : AssignOp
    {


    }
    [Terminal("|=")]
    public class OrAssignOperator : AssignOp
    {



    }
    [Terminal(">>=")]
    public class RightShiftAssignOperator : AssignOp
    {



    }
    [Terminal("<<=")]
    public class LeftShiftAssignOperator : AssignOp
    {



    }
#endregion
}
