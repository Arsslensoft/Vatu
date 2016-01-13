using bsn.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;


namespace VTC.Core
{
    [Terminal("[]")]
    [Terminal("nameof")]
    [Terminal("typeof")]
    [Terminal("extends")]
    [Terminal("delegate")]
    [Terminal("public")]
    [Terminal("vfastcall")]
    [Terminal("ref")]
    [Terminal("isolated")]
    [Terminal("interrupt")]
     [Terminal("union")]
    [Terminal("pascal")]
    [Terminal("operator")]
    [Terminal("override")]
        [Terminal("loop")]
    [Terminal("asm")]
    [Terminal("break")]
    [Terminal("next")]
    [Terminal("case")]
    [Terminal("continue")]
    [Terminal("default")]
    [Terminal("do")]
    [Terminal("else")]
    [Terminal("for")]
    [Terminal("goto")]
    [Terminal("if")]
    [Terminal("return")]
    [Terminal("sizeof")]
    [Terminal("switch")]
    [Terminal("typedef")]
    [Terminal("while")]
    [Terminal("use")]
    [Terminal("namespace")]
    [Terminal("(EOF)")]
    [Terminal("(Error)")]
    [Terminal("(Whitespace)")]
    [Terminal("(Comment)")]
    [Terminal("(NewLine)")]
    [Terminal("(*/)")]
    [Terminal("(//)")]
    [Terminal("(/*)")]
    [Terminal("(")]
    [Terminal(")")]
    [Terminal("{")]
    [Terminal("}")]
    [Terminal("[")]
    [Terminal("]")]
    [Terminal(":")]
    [Terminal(";")]
    [Terminal("?")]
    [Terminal(",")]

    [Terminal("@")]
    [Terminal("enum")]
    [Terminal("struct")]
    [Terminal("extern")]
    [Terminal("static")]
    [Terminal("const")]
    [Terminal("entry")]
    [Terminal("stdcall")]
    [Terminal("fastcall")]
    [Terminal("cdecl")]
    [Terminal("private")]
    public class SimpleToken : SemanticToken, IResolve
    {
        public Location loc;
        public Location Location { get { return CompilerContext.TranslateLocation(position); } }

        public virtual string Name { get { return symbol.Name; } }

        public virtual bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    }
 

    public class Definition : SimpleToken, IEmit, IResolve
    {
      
        public Definition()
        {
            loc = CompilerContext.TranslateLocation(position);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    }
    public abstract class Operator : SimpleToken, IEmit,IEmitExpr, IResolve
    {
        public Namespace Namespace { get; set; }
        public Expr Left { get; set; }
        public Expr Right { get; set; }

        public bool FixConstant(ResolveContext rc)
        {
            bool conv = false;
            if (Left is ConstantExpression && Right is ConstantExpression)
            {
                // greater conversion
                if (Left.Type.Size > Right.Type.Size)
                {
                    Right = (Right as ConstantExpression).ConvertImplicitly(rc, Left.Type, ref conv);
                    return conv;
                }
                else if (Left.Type.Size < Right.Type.Size)
                {
                    Left = (Left as ConstantExpression).ConvertImplicitly(rc, Right.Type, ref conv);
                    return conv;
                }
                else return (Left.Type.Equals( Right.Type));
            }
            else if (Left is ConstantExpression)
            {
               Left = (Left as ConstantExpression).ConvertImplicitly(rc,Right.Type,ref conv);
               return conv;

            }
            else if (Right is ConstantExpression)
            {
                Right = (Right as ConstantExpression).ConvertImplicitly(rc, Left.Type, ref conv);
                return conv;
            }else
                return (Left.Type.Equals(Right.Type));
        }
        public TypeSpec CommonType { get; set; }

        public Operator()
        {
            loc = CompilerContext.TranslateLocation(position);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {


            return true;

        }
        public virtual bool EmitFromStack(EmitContext ec)
        {


            return true;
        }
        public virtual bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            return true;
        }
        public virtual bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return true;
        }
        public virtual string CommentString()
        {
            if (symbol != null)
                return symbol.Name;
            else return "";
        }
    }
    public abstract class Statement : SimpleToken, IEmit, IResolve
    {
  

        public Statement()
        {
            loc = CompilerContext.TranslateLocation(position);
        }

        protected bool reachable;

        public bool IsUnreachable
        {
            get
            {
                return !reachable;
            }
        }


        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }

        public virtual Reachability MarkReachable(Reachability rc)
        {
            if (!rc.IsUnreachable)
                reachable = true;

            return rc;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    } 
    public  class Expr : SimpleToken, IEmit,  IEmitExpr
    {
      
       public Expr current;
        [Rule("<Expression> ::= <Op Assign>")]
        public Expr(Expr expr)
        {
            loc = expr.Location;
            current = expr;

        }

        
    
        protected TypeSpec type;

        public TypeSpec Type
        {
            get
            {
                if (type != null && type.IsTypeDef)
                    return type.GetTypeDefBase(type);
                else return type;
            }
            set
            {
                type = value;
            }
        }
      

        public Expr(TypeSpec tp, Location lc)
        {
            type = tp;
            loc = lc;
        }
        public Expr(Location lc)
        {
            type = null;
            loc = lc;
        }
        public Expr()
            : this(Location.Null)
        {
           
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;
            if (current != null)
               ok &=  current.Resolve(rc);
         

            return ok;
        }
        public virtual bool Emit(EmitContext ec)
        {
            if (current != null)
              current.Emit(ec);
      
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (current != null)
            {
                current = (Expr)current.DoResolve(rc);
                Type = current.Type;
              
          
                    return current;
            }
         
            return this;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {

       
            return current.EmitToStack(ec);

        }
        public virtual bool EmitFromStack(EmitContext ec)
        {
       

            return current.EmitFromStack(ec);
        }
        public virtual bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            return current.EmitToRegister(ec,rg);
        }
        public virtual bool EmitBranchable(EmitContext ec, Label truecase,bool v )
        {
            return current.EmitBranchable(ec,truecase,v);
        }
     
        public virtual string CommentString()
        {
            return "";
        }
    }
     public abstract class DeclarationToken : SimpleToken, IEmit, IResolve
    {



        public DeclarationToken(Location lc)
        {
            loc = lc;
        }
        public DeclarationToken()
           :this(Location.Null)
        {
            loc = CompilerContext.TranslateLocation(position);
        }

     
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }

      
     
    }
    

   
    public class AccessOp : Operator
    {
        public MethodSpec OvlrdOp { get; set; }
        public virtual int  Offset { get { return 0; } }
        public virtual MemberSpec Member { get { return null; } }
        public AccessOperator _op;
        public RegistersEnum? Register { get; set; }
        public virtual bool EmitOverrideOperatorAddress(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + _op.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorValue(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + _op.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitInstruction(new Mov() { SourceReg = EmitContext.A, DestinationReg = EmitContext.SI});
            ec.EmitPush(EmitContext.SI, 80, true);
            return true;
        }
    }
    public class BinaryOp : Operator
    {
        public MethodSpec OvlrdOp { get; set; }
        public RegistersEnum? RightRegister { get; set; }
        public RegistersEnum? LeftRegister { get; set; }
        protected bool ConstantOperation = false;
        protected bool RegisterOperation = false;
        public  BinaryOperator Operator {get;set;}
       protected bool unsigned = true;
     
        public virtual bool EmitOverrideOperator(EmitContext ec)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + Left.CommentString() + " " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {
            Left.EmitToStack(ec);
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + Left.CommentString() + " "+Operator.ToString()+" " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
    
            ec.EmitPush(EmitContext.A);
            ec.EmitPop(LeftRegister.Value);

            if (CommonType.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(LeftRegister.Value), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = LeftRegister.Value, SourceValue = EmitContext.TRUE, Size = 80 });
         
            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = cond, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = acond, DestinationLabel = truecase.Name });
       
            return true;
        }
    }
    public class UnaryOp : Operator
    {
        public MethodSpec OvlrdOp { get; set; }
        protected bool RegisterOperation = false;
        public RegistersEnum? Register { get; set; }
        public UnaryOperator Operator { get; set; }

        public virtual bool EmitOverrideOperator(EmitContext ec)
        {

            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " + Operator.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {
  
            Right.EmitToStack(ec);
            ec.EmitComment("Override Operator : " +  Operator.ToString() + " " + Right.CommentString());
            ec.EmitCall(OvlrdOp);

            ec.EmitPush(EmitContext.A);
            ec.EmitPop(Register.Value);

            if (CommonType.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(Register.Value), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = EmitContext.TRUE, Size = 80 });

            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = cond, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = acond, DestinationLabel = truecase.Name });

            return true;
        }
    }
    public class AssignOp : Operator
    {
        public BinaryOp _op;

     
    }

    [Flags]
    public enum BinaryOperator
    {
        Multiply = 0 | ArithmeticMask,
        Division = 1 | ArithmeticMask,
        Modulus = 2 | ArithmeticMask,
        Addition = 3 | ArithmeticMask | AdditionMask,
        Subtraction = 4 | ArithmeticMask | SubtractionMask,

        LeftShift = 5 | ShiftMask,
        RightShift = 6 | ShiftMask,

        LessThan = 7 | ComparisonMask | RelationalMask,
        GreaterThan = 8 | ComparisonMask | RelationalMask,
        LessThanOrEqual = 9 | ComparisonMask | RelationalMask,
        GreaterThanOrEqual = 10 | ComparisonMask | RelationalMask,
        Equality = 11 | ComparisonMask | EqualityMask,
        Inequality = 12 | ComparisonMask | EqualityMask,
       
     


        BitwiseAnd = 13 | BitwiseMask,
        ExclusiveOr = 14 | BitwiseMask,
        BitwiseOr = 15 | BitwiseMask,

        LogicalAnd = 16 | LogicalMask,
        LogicalOr = 17 | LogicalMask,
      
        
 
        LeftRotate = 18 | ShiftMask,
        RightRotate = 19 | ShiftMask,
        //
        // Operator masks
        //
        ValuesOnlyMask = ArithmeticMask - 1,
        ArithmeticMask = 1 << 5,
        ShiftMask = 1 << 6,
        ComparisonMask = 1 << 7,
        EqualityMask = 1 << 8,
        BitwiseMask = 1 << 9,
        LogicalMask = 1 << 10,
        AdditionMask = 1 << 11,
        SubtractionMask = 1 << 12,
        RelationalMask = 1 << 13,

        DecomposedMask = 1 << 19,
        NullableMask = 1 << 20
    }
    public enum UnaryOperator : byte
    {
        UnaryPlus, UnaryNegation, LogicalNot, OnesComplement,
        AddressOf, ValueOf, PostfixIncrement, PostfixDecrement, ZeroTest ,      ParityTest 
    }
    public enum AccessOperator : byte
    {
       ByValue,
       ByAddress,
       ByIndex,
        ByName
    }
    public enum AssignOperator : byte
    {
        Equal,
        AddAssign,
        SubAssign,
        MulAssign,
        DivAssign,
        XorAssign,
        AndAssign,
        OrAssign,
        RightShiftAssign,
        LeftShiftAssign,
        Exchange

    }
  



    public class DeclarationSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly DeclarationSequence<T> next;




        [Rule("<Decls> ::= <Decl>", typeof(Declaration))]
        public DeclarationSequence(T item)
            : this(item, null)
        {
        }


        [Rule("<Decls> ::= <Decl> <Decls>", typeof(Declaration))]
        public DeclarationSequence(T item, DeclarationSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (DeclarationSequence<T> sequence = this; sequence != null; sequence = sequence.next)
            {
                if (sequence.item != null)
                {
                    yield return sequence.item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
    public class GlobalSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly GlobalSequence<T> next;




        [Rule("<GLOBALS> ::= ", typeof(Global))]
        public GlobalSequence()
            : this(null, null)
        {
        }


        [Rule("<GLOBALS> ::= <GLOBAL> <GLOBALS>", typeof(Global))]
        public GlobalSequence(T item, GlobalSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (GlobalSequence<T> sequence = this; sequence != null; sequence = sequence.next)
            {
                if (sequence.item != null)
                {
                    yield return sequence.item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
    public class ParameterSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly ParameterSequence<T> next;




        [Rule("<PARAM EXPR>  ::= ", typeof(Expr))]
        public ParameterSequence()
            : this(null, null)
        {
        }
        [Rule("<PARAM EXPR>  ::= <Expression>", typeof(Expr))]
        public ParameterSequence(T item)
            : this(item, null)
        {
        }

        [Rule("<PARAM EXPR>  ::= <Expression> ~',' <PARAM EXPR>", typeof(Expr))]
        public ParameterSequence(T item, ParameterSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (ParameterSequence<T> sequence = this; sequence != null; sequence = sequence.next)
            {
                if (sequence.item != null)
                {
                    yield return sequence.item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
