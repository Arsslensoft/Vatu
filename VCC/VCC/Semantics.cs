using bsn.GoldParser.Semantic;
using System;
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
    [Terminal("(")]
    [Terminal(")")]
    [Terminal("=")]
    public class SimpleToken : SemanticToken
    {
    }

    [Terminal("auto")]
    [Terminal("break")]
    [Terminal("case")]
    [Terminal("const")]
    [Terminal("continue")]
    [Terminal("default")]
    [Terminal("do")]
    [Terminal("else")]
    [Terminal("enum")]
    [Terminal("extern")]
    [Terminal("for")]
    [Terminal("goto")]
    [Terminal("if")]
    [Terminal("register")]
    [Terminal("return")]
    [Terminal("signed")]
    [Terminal("sizeof")]
    [Terminal("static")]
    [Terminal("struct")]
    [Terminal("switch")]
    [Terminal("typedef")]
    [Terminal("union")]
    [Terminal("unsigned")]
    [Terminal("while")]
    [Terminal("volatile")]
    // types
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
    public class KeywordToken : SimpleToken
    {
    }


    public class ResolveContext : IDisposable
    {
       

        #region IDisposable Members

        public void Dispose()
        {
         
        }

        #endregion

    }


    public abstract class Statement : SimpleToken, IEmit
    {

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


        public abstract bool Resolve(ResolveContext rc);
       
        public abstract bool Emit(EmitContext ec);

        public virtual Reachability MarkReachable(Reachability rc)
        {
            if (!rc.IsUnreachable)
                reachable = true;

            return rc;
        }
    }
    public abstract class Expression : SimpleToken,IEmitExpr,IEmit
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

        public abstract bool Resolve(ResolveContext rc);

        public abstract bool Emit(EmitContext ec);
        public abstract bool EmitFromStack(EmitContext ec);
        public abstract bool EmitToStack(EmitContext ec);
    }


    public enum Operator : byte
    {
        // Unary Operators

        // Binary Operators
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
}
