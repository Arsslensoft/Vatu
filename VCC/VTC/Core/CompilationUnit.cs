using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Core;

namespace VTC
{
   public class CompilationUnit : SimpleToken
    {
       public GlobalSequence<Global> Globals { get; set; }
       public IncludeSequence<IncludeDeclaration> Includes { get; set; }

       [Rule("<COMPILATION UNIT> ::= <Includes Decl> <GLOBALS>")]
       public CompilationUnit(IncludeSequence<IncludeDeclaration> incl, GlobalSequence<Global> gbls)
       {
           Globals = gbls;
           Includes = incl;
       }
       [Rule("<COMPILATION UNIT> ::= <Includes Decl>")]
       public CompilationUnit(IncludeSequence<IncludeDeclaration> incl)
       {
           Globals = null;
           Includes = incl;
       }
       public override SimpleToken DoResolve(ResolveContext rc)
       {
           return base.DoResolve(rc);
       }
      
    }
}
