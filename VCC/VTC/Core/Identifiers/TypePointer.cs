using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_next == null)
            {
                PointerCount = 0;
                return this;
            }
            PointerCount = 1 + ((TypePointer)(_next.DoResolve(rc))).PointerCount;
            return this;
        }


    }
   
	
	
	
}