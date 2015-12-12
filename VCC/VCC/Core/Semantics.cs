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
        public string Name { get { return symbol.Name; } }
    }

    [Terminal("break")]
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
    // types
  /*  [Terminal("void")]
    [Terminal("char")]
    [Terminal("schar")]
    [Terminal("short")]
    [Terminal("ushort")]
    [Terminal("int")]
    [Terminal("uint")]
    [Terminal("long")]
    [Terminal("ulong")]
    [Terminal("double")]
    [Terminal("float")]
    [Terminal("extended")]
    [Terminal("bool")]*/
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
        public Location loc;
        public Location Location { get { return loc; } }
        public TypeSpec Type { get; set; }
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
            Type = rc.ResolveType(this.symbol.Name);
            return this ;
        }
    }

    public class Definition : SimpleToken, IEmit, IResolve
    {
        public Location loc;
        public Location Location { get { return loc; } }

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
    public abstract class Operator : SimpleToken, IEmit, IResolve
    {
        public Expr Left { get; set; }
        public Expr Right { get; set; }
          public Location loc;
        public Location Location { get { return loc; } }

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

    }
    public abstract class Statement : SimpleToken, IEmit, IResolve
    {
  

        public Statement()
        {
            loc = CompilerContext.TranslateLocation(position);
        }
        public Location loc;

        public Location Location { get { return loc; } }
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
            current = expr;

        }

          [Rule("<Expression> ::= <Expression> ~',' <Op Assign>")]
        public Expr(Expr expr, Expr n)
        {
            current = expr;
            next = n;
        }
           protected Location loc;
        protected TypeSpec type;

        public TypeSpec Type
        {
            get { return type; }
            set { type = value; }
        }
        public Location Location { get { return loc; } }

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
            loc = CompilerContext.TranslateLocation(position);
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
            current.Emit(ec);
            if (next != null)
                next.Emit(ec);
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            current = (Expr)current.DoResolve(rc);
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
       

            return current.EmitToStack(ec);
        }
        public virtual bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            return current.EmitToRegister(ec,rg);
        }

    }
      public abstract class DeclarationToken : SimpleToken, IEmit, IResolve
    {

        public Location loc;

        public Location Location { get { return loc; } }

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
        public Location loc;
        public Location Location { get { return loc; } }
        public TypeSpec Type { get; set; }
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
        public Location loc;
        public Location Location { get { return loc; } }
       
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
        private readonly AccessOperator _op;

   
    }
    public class BinaryOp : Operator
    {
        private readonly BinaryOperator _op;

     
    }
    public class UnaryOp : Operator
    {
        private readonly UnaryOperator _op;

    
    }
    public class AssignOp : Operator
    {
        private readonly BinaryOperator _op;

     
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
        AddressOf, ValueOf, PostfixIncrement, PostfixDecrement, PrefixIncrement, PrefixDecrement
    }
    public enum AccessOperator : byte
    {
       ByValue,
       ByAddress,
       ByIndex
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
        LeftShiftAssign

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


   

        [Rule("<Decls> ::= ", typeof(Declaration))]
        public DeclarationSequence()
            : this(null, null)
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
}
