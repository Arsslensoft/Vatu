using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class CastKind : SimpleToken
    {
       public bool IsImplicit { get; set; }
        [Rule(@"<Cast Kind> ::= implicit")]
        [Rule(@"<Cast Kind> ::= explicit")]
       public CastKind(SimpleToken tok)
       {
           IsImplicit = (tok.Name == "implicit");
       }
    }
}
