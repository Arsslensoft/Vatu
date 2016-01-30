using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	 public class NameIdentifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }
        [Rule(@"<Name> ::= Id")]
        public NameIdentifier(Identifier idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName.Name;
        }
        [Rule(@"<Name> ::= ~global")]
        public NameIdentifier()
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = "global";
        }
        [Rule(@"<Name> ::= <QualifiedName>")]
        public NameIdentifier(QualifiedNameIdentifier idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName.Name;
        }

    }
    
	
	
}