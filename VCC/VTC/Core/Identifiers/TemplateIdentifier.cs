using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	[Terminal("TemplateId")]
    public class TemplateIdentifier : Expr
    {
        protected readonly string _idName;
        public override string Name { get { return _idName; } }

        public TemplateIdentifier(string idName)
        {
            loc = CompilerContext.TranslateLocation(position);
            _idName = idName.Remove(0,1);
        }


    }
    
	
	
}