using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class IncludeDeclaration : SimpleToken
    {
       public string IncludeFile { get; set; }
      [Rule("<Include Decl> ::= ~include IncludeLiteral")]
       public IncludeDeclaration(IncludeLiteral il)
       {
           IncludeFile = il.IncludeFile;
       }
      
      public override bool Resolve(ResolveContext rc)
       {
           return base.Resolve(rc);
       }
 public override SimpleToken DoResolve(ResolveContext rc)
       {
           return base.DoResolve(rc);
       }
       public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
       {

           return base.DoFlowAnalysis(fc);
       }
    }
}
