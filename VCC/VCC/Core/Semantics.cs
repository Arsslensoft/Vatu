using bsn.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;


namespace VCC.Core
{
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

    public class SimpleToken : SemanticToken
    {
        public Location loc;
        public Location Location { get { return CompilerContext.TranslateLocation(position); } }

        public string Name { get { return symbol.Name; } }
    }
 
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
 
    // types
    public class KeywordToken : SimpleToken
    {
    }


  
    [Terminal("void")]
    [Terminal("byte")]
    [Terminal("sbyte")]
    [Terminal("int")]
    [Terminal("uint")]
    [Terminal("string")]
    [Terminal("bool")]
    public class TypeToken : SimpleToken, IResolve
    {
        TypeSpec _ts;
        public TypeSpec Type
        {
            get
            {
                if (_ts != null && _ts.IsTypeDef)
                    return _ts.GetTypeDefBase(_ts);
                else return _ts;
            }
            set
            {
                _ts = value;
            }
        }
        public TypeToken()
        {
            loc = CompilerContext.TranslateLocation(position);

        }

        public virtual bool Resolve(ResolveContext rc)
        {
        
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            Type = rc.TryResolveType(this.symbol.Name);
            return this ;
        }
    }

  

    public class Definition : SimpleToken, IEmit, IResolve
    {
      
        public Definition()
        {
            loc = CompilerContext.TranslateLocation(position);
        }
        public virtual bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
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
                else return (Left.Type == Right.Type);
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
            return Left.Type == Right.Type;
        }
        public TypeSpec CommonType { get; set; }

        public Operator()
        {
            loc = CompilerContext.TranslateLocation(position);
        }
        public virtual bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
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


        public virtual bool Resolve(ResolveContext rc)
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
        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    } 
    public class Expr : SimpleToken, IEmit, IResolve, IEmitExpr
    {
       public Expr next;
       public Expr current;
        [Rule("<Expression> ::= <Op Assign>")]
        public Expr(Expr expr)
        {
            loc = expr.Location;
            current = expr;

        }

          [Rule("<Expression> ::= <Expression> ~',' <Op Assign>")]
        public Expr(Expr expr, Expr n)
        {
    
            current = expr;
            next = n;
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
        public virtual bool Resolve(ResolveContext rc)
        {
            bool ok = true;
            if (current != null)
               ok &=  current.Resolve(rc);
            if (next != null)
                ok &= next.Resolve(rc);

            return ok;
        }
        public virtual bool Emit(EmitContext ec)
        {
            if (current != null)
              current.Emit(ec);
            if (next != null)
                next.Emit(ec);
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            if (current != null)
            {
                current = (Expr)current.DoResolve(rc);
                Type = current.Type;
                if (next == null)
                    return current;
            }
            if (next != null)
                next = (Expr)next.DoResolve(rc);
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

        public virtual bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }

        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
     
    }
    public abstract class ModifierToken : SimpleToken, IResolve
    {
 
      
        public ModifierToken()
        {
            loc = CompilerContext.TranslateLocation(position);

        }

        public virtual bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    }


    [Terminal("Id")]
    public class Identifier : Expr
    {
        protected readonly string _idName;
        public string Name { get { return _idName; } }
       
        public Identifier(string idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName;
        }


    }
    public class MethodIdentifier : Identifier
    {
        public Identifier Id { get; set; }
        public TypeToken Type { get; set; }
    
        [Rule(@"<Func ID> ::= <Type> Id")]
        public MethodIdentifier(TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            Type = type;
        }

        [Rule(@"<Func ID> ::= Id")]
        public MethodIdentifier(Identifier id)
            : base(id.Name)
        {
            Id = id;
            Type = null;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
          Type = (TypeToken) Type.DoResolve(rc);
          base.Type = Type.Type;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            Type.Resolve(rc);

            return base.Resolve(rc);
        }
    }



    public class TypePointer : SimpleToken
    {
    
       
        public int PointerCount { get; set; }

        TypePointer _next;
        [Rule(@"<Pointers> ::= ~'*' <Pointers>")]
        public TypePointer(TypePointer ptr)
        {
            loc = CompilerContext.TranslateLocation(position);
            _next = ptr;
        }
        [Rule(@"<Pointers> ::=  ")]
        public TypePointer()
        {
            loc = CompilerContext.TranslateLocation(position);
            _next = null;
        }

        public TypePointer DoResolve(ResolveContext rc)
        {
            if (_next == null)
            {
                PointerCount = 0;
                return this;
            }
            PointerCount = 1 + (_next.DoResolve(rc)).PointerCount;
            return this;
        }
        

    }
    public class AccessOp : Operator
    {
        public virtual int  Offset { get { return 0; } }
        public virtual MemberSpec Member { get { return null; } }
        public AccessOperator _op;
        public RegistersEnum? Register { get; set; }
   
    }
    public class BinaryOp : Operator
    {
        public RegistersEnum? RightRegister { get; set; }
        public RegistersEnum? LeftRegister { get; set; }
        protected bool ConstantOperation = false;
        protected bool RegisterOperation = false;
        protected  BinaryOperator Operator {get;set;}

     
    }
    public class UnaryOp : Operator
    {
        protected bool RegisterOperation = false;
        public RegistersEnum? Register { get; set; }
        public UnaryOperator Operator { get; set; }
      
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
    public struct Reachability
    {
        readonly bool unreachable;

        Reachability(bool unreachable)
        {
            this.unreachable = unreachable;
        }

        public bool IsUnreachable
        {
            get
            {
                return unreachable;
            }
        }

        public static Reachability CreateUnreachable()
        {
            return new Reachability(true);
        }

        public static Reachability operator &(Reachability a, Reachability b)
        {
            return new Reachability(a.unreachable && b.unreachable);
        }

        public static Reachability operator |(Reachability a, Reachability b)
        {
            return new Reachability(a.unreachable | b.unreachable);
        }
    }

    public class StatementSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly StatementSequence<T> next;


   
    
        public StatementSequence()
            : this(null, null)
        {
        }


    //    [Rule("<Stm List>  ::=  <Statement> <Stm List> ", typeof(Statement))]
        public StatementSequence(T item, StatementSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (StatementSequence<T> sequence = this; sequence != null; sequence = sequence.next)
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
    public class ExpressionSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly ExpressionSequence<T> next;


        public ExpressionSequence()
            : this(null, null)
        {
        }

    //    [Rule("<Expression> ::= <Op Assign>", typeof(Expr))]
        public ExpressionSequence(T item)
            : this(item, null)
        {
        }


     //   [Rule("<Expression> ::= <Expression> ~',' <Op Assign>", typeof(Expr))]
        public ExpressionSequence(T item, ExpressionSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (ExpressionSequence<T> sequence = this; sequence != null; sequence = sequence.next)
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
}
