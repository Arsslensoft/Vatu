using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class RessourceDeclaration : IncludeDeclaration
    {
  
       public string Name { get; set; }
       public Namespace NS { get; set; }
      [Rule("<Include Decl> ::= ~ressource <Name> StringLiteral")]
       public RessourceDeclaration(NameIdentifier name,StringLiteral il) : base(il)
       {
           if (name.Name.Contains("::"))
           {
             
               string ns= name.Name.Substring(0, name.Name.LastIndexOf("::"));
               Name = name.Name.Remove(0, name.Name.LastIndexOf("::") + 2);
               NS = new Namespace(ns, Location);
           }
           else
           {
               Name = name.Name;
               NS = Namespace.Default;
           }
       
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
