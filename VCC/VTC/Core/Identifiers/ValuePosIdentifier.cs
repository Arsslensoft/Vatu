using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	public class ValuePosIdentifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }

    
        [Rule(@"<VALUE POS> ::= HIGH")]
         [Rule(@"<VALUE POS> ::= LOW")]
        public ValuePosIdentifier(SimpleToken vid)
        {
         //   Location = vid.Location;
            _idName = vid.Name;
        }

    }
    
	
	
}