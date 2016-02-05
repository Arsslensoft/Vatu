using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class Sequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly Sequence<T> next;




   
        public Sequence(T item)
            : this(item, null)
        {
        }



        public Sequence(T item, Sequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (Sequence<T> sequence = this; sequence != null; sequence = sequence.next)
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




        [Rule("<GLOBALS> ::= <GLOBAL> ", typeof(Global))]
        public GlobalSequence(T item)
            : this(item, null)
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
    public class IncludeSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly IncludeSequence<T> next;




     
       [Rule("<Includes Decl> ::=", typeof(IncludeDeclaration))]
        public IncludeSequence()
            : this(null, null)
        {
        }

        [Rule("<Includes Decl> ::= <Include Decl> <Includes Decl>", typeof(IncludeDeclaration))]
        public IncludeSequence(T item, IncludeSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (IncludeSequence<T> sequence = this; sequence != null; sequence = sequence.next)
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
    public class IntegralConstSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly IntegralConstSequence<T> next;





        [Rule("<Integral Const List> ::= <Integral Const>", typeof(IntegralConst))]
        public IntegralConstSequence(T item)
            : this(item, null)
        {
        }

        [Rule("<Integral Const List> ::= <Integral Const> ~',' <Integral Const List> ", typeof(IntegralConst))]
        public IntegralConstSequence(T item, IntegralConstSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (IntegralConstSequence<T> sequence = this; sequence != null; sequence = sequence.next)
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
    public class CaseSequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken
    {
        private readonly T item;
        private readonly CaseSequence<T> next;




        [Rule("<Case Stms> ::= <Case Stm>", typeof(Case))]
        public CaseSequence(T item)
            : this(item, null)
        {
        }


        [Rule("<Case Stms> ::= <Case Stm> <Case Stms>", typeof(Case))]
        public CaseSequence(T item, CaseSequence<T> next)
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (CaseSequence<T> sequence = this; sequence != null; sequence = sequence.next)
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

    public class MultiDimInitializerSequence : Sequence<Expr>
    {
        public bool HasMultiDim { get; set; }
        public List<Expr> Expressions { get; set; }
          [Rule("<NDim Initializers>   ::= <NDim Initializer>")]
        [Rule("<NDim Initializers>   ::= <Initializer>")]
        public MultiDimInitializerSequence(Expr item)
            : base(item, null)
        {
        }

        [Rule("<NDim Initializers>  ::= <NDim Initializer> ~',' <NDim Initializers>    ")]
        [Rule("<NDim Initializers>  ::= <Initializer> ~',' <NDim Initializers>    ")]
        public MultiDimInitializerSequence(Expr item, MultiDimInitializerSequence next)
            : base(item, next)
        {

        }


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Expressions = new List<Expr>();
            TypeSpec first = null;
            bool ismultidim = false;

            foreach (Expr l in this)
            {
                Expr tmp = (Expr)l.DoResolve(rc);
                if (tmp is InitializerConstant)
                {
         
                    if (first == null)
                    {
                        first = tmp.Type;
                        HasMultiDim = false;
                        ismultidim = false;
                        Expressions.Add(tmp);
                    }
                    else if (ismultidim )
                        ResolveContext.Report.Error(0, Location, "Initializers dimensions has been violated");
                    else if (!first.Equals(tmp.Type))
                        ResolveContext.Report.Error(0, Location, "Initializers must have same type as the first element");
                    else Expressions.Add(tmp);
                }
                else if (tmp is MultiDimInitializerConstant)
                {
                    if (first == null)
                    {
                        first = tmp.Type;
                        ismultidim = true;
                        HasMultiDim = true;
                        Expressions.Add(tmp);
                    }
                    else if (!ismultidim)
                        ResolveContext.Report.Error(0, Location, "Initializers dimensions has been violated");
                    else if (!first.Equals(tmp.Type))
                        ResolveContext.Report.Error(0, Location, "Initializers must have same type as the first element");
                    else Expressions.Add(tmp);
                }
                else ResolveContext.Report.Error(0, Location, "Initializers must be constant expressions");

            }
            return this;
        }

    }
    public class InitializerSequence : Sequence<Literal>
    {

        public List<Expr> Expressions { get; set; }
        [Rule("<Initializers>  ::= <CONSTANT>")]
        public InitializerSequence(Literal item)
            : base(item, null)
        {
        }

        [Rule("<Initializers> ::= <CONSTANT> ~',' <Initializers>    ")]
        public InitializerSequence(Literal item, InitializerSequence next)
            : base(item, next)
        {

        }

      
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Expressions = new List<Expr>();
            TypeSpec first = null;
            foreach (Expr l in this)
            {
                Expr tmp = (Expr)l.DoResolve(rc);
                if (tmp is ConstantExpression)
                {
                    if (first == null)
                    {
                        first = tmp.Type;
                        Expressions.Add(tmp);
                    }
                    else if (!first.Equals(tmp.Type))
                        ResolveContext.Report.Error(0, Location, "Initializers must have same type as the first element");
                    else Expressions.Add(tmp);

                }
                else ResolveContext.Report.Error(0, Location, "Initializers must be constant expressions");

            }
            return this;
        }

    }
    public class FunctionSpecifierSequence: Sequence<FunctionSpecifier>
    {

        [Rule("<Func Specs> ::= <Func Spec>")]
        public FunctionSpecifierSequence(FunctionSpecifier item)
            : base(item, null)
        {
        }

        [Rule("<Func Specs> ::= <Func Spec>  <Func Specs> ")]
        public FunctionSpecifierSequence(FunctionSpecifier item, FunctionSpecifierSequence next)
            : base(item, next)
        {

        }

        public Specifiers FunctionSpecs { get; set; }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            FunctionSpecifier fs = null;
            FunctionSpecs = Specifiers.NoSpec;
            foreach (FunctionSpecifier s in this)
            {
                fs = (FunctionSpecifier)s.DoResolve(rc);
                FunctionSpecs |= fs.Specs;
            }
            return this;
        }
     
    }
}
