using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	[Terminal("Id")]
    public class Identifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }

        public Identifier(string idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName;
        }


    }
    
	
	
}