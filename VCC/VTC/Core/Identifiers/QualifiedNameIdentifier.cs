using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	public class QualifiedNameIdentifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }
        [Rule(@"<QualifiedName> ::= <Name> ~'::' Id")]
        public QualifiedNameIdentifier(NameIdentifier nid,Identifier idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = nid.Name + "::"+idName.Name ;
        }
      
    }

	
	
}