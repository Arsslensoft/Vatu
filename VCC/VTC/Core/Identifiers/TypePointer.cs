using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
    public class TypePointer : SimpleToken
    {



        internal bool isref = false;
        TypePointer _next;

        [Rule(@"<Pointers> ::= ~'*' <Pointers>")]
        public TypePointer(TypePointer ptr)
        {
            //  Location = CompilerContext.TranslateLocation(position);
            _next = ptr;
        }
        [Rule(@"<Pointers> ::= '&' <Pointers>")]
        public TypePointer(SimpleToken t, TypePointer ptr)
        {
            isref = true;
            //  Location = CompilerContext.TranslateLocation(position);
            _next = ptr;
        }
        [Rule(@"<Pointers> ::= ~'*' ")]
        public TypePointer()
        {
            //  Location = CompilerContext.TranslateLocation(position);
            _next = null;
        }
        [Rule(@"<Pointers> ::= '&' ")]
        public TypePointer(SimpleToken t)
        {
            isref = true;
            //  Location = CompilerContext.TranslateLocation(position);
            _next = null;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_next == null)
                return this;

            _next = (TypePointer)_next.DoResolve(rc);
            return this;
        }
        public TypeSpec CreateType(TypeSpec tp, TypePointer ptr)
        {
            if (ptr._next == null)
            {
                if (isref)
                {
                    
                    return tp.MakeReference();
                }
                else return tp.MakePointer();
            }
            else
            {

                if (isref)
                
                    return CreateType(tp.MakeReference(),ptr._next);
                
                else return CreateType(tp.MakePointer(), ptr._next);
            }
        }


    }
   
	
	
	
}