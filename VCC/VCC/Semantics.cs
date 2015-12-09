using bsn.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using VJay;

namespace VCC
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
            if (this is ScalarTypeIdentifier)
            {
                ((ScalarTypeIdentifier)this).Resolve(rc);
                //Type = rc.ResolveType(((ScalarTypeIdentifier)this).t);

                //Console.WriteLine(Type.ToString());
            }
            return true;
        }
    }

    public class TypePointer : SimpleToken
    {
        public Location loc;
        public Location Location { get { return loc; } }

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


    }
    public class Definition : SimpleToken, IEmitExpr, IEmit, IResolve
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
        public virtual bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {
            return true;
        }
    }
    public abstract class Operator : SimpleToken, IEmitExpr, IEmit, IResolve
    {

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
        public virtual bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {
            return true;
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
    }
    public class Expression : SimpleToken, IEmitExpr, IEmit, IResolve
    {
        protected Location loc;
        protected TypeSpec type;

        public TypeSpec Type
        {
            get { return type; }
            set { type = value; }
        }
        public Location Location { get { return loc; } }

        public Expression(TypeSpec tp, Location lc)
        {
            type = tp;
            loc = lc;
        }
        public Expression(Location lc)
        {
            type = null;
            loc = lc;
        }
        public Expression()
            : this(Location.Null)
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
        public virtual bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {
            return true;
        }
    }
    public class Expr : SimpleToken, IEmitExpr, IEmit, IResolve
    {
        Expr next;
        Expr current;
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
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {
            return true;
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
        public virtual bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {
            return true;
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
    }


    public class Declaration : DeclarationToken
    {
        protected Identifier _name;
        protected Declaration _dcl;

     
        public Identifier Identifier
        {
            get { return _name; }
        }

        public Declaration()
        {

        }

        [Rule(@"<Decl>  ::= <Func Decl>")]
        [Rule(@"<Decl>  ::= <Func Proto>")]
        [Rule(@"<Decl>  ::= <Struct Decl>")]
        [Rule(@"<Decl>  ::= <Union Decl>")]
        [Rule(@"<Decl>  ::= <Enum Decl>")]
        [Rule(@"<Decl>  ::= <Var Decl>")]
        [Rule(@"<Decl>  ::= <Typedef Decl>")]
        public Declaration(Declaration decl)
        {
            _dcl = decl;
        }

        public virtual bool Resolve(ResolveContext rc)
        {
            if (_dcl != null)
                return _dcl.Resolve(rc);
            else return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {
            return true;
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
