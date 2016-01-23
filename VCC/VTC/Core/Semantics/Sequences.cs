using bsn.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
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
}
