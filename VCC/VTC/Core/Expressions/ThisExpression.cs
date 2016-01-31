using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core
{
   public class ThisExpression : VariableExpression
    {
       [Rule(@"<Var Expr>     ::= this")]
       public ThisExpression(SimpleToken t)
           : base("this")
       {
           position = t.position;
       
       }

       public override SimpleToken DoResolve(ResolveContext rc)
       {

           return base.DoResolve(rc);
       }
       public override bool Resolve(ResolveContext rc)
       {
           variable = rc.Resolver.TryResolveVar("this");
           return base.Resolve(rc);
       }
    }
}
