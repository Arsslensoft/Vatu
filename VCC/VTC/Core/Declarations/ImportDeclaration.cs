using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class ImportDeclaration : SimpleToken
    {
        public Namespace Import { get; set; }
    

        [Rule("<Import>   ::= ~use <Name> ~';'")]
        public ImportDeclaration(NameIdentifier id)
        {
            Import = new Namespace(id.Name);
        }
       
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {

            return base.DoFlowAnalysis(fc);
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Import.Name == "global")
                ResolveContext.Report.Error(0, Location, "Global namespace cannot be used for access");
                Namespace ns = rc.Resolver.ResolveNS(Import.Name);

                if (Namespace.Default == ns)
                {

                    ns = rc.Resolver.ResolveNS(rc.CurrentNamespace.Name + "::" + Import.Name);
                     Import = ns;
                }
                

              if (Namespace.Default == Import)
                    ResolveContext.Report.Error(0, Location, "Unknown namespace");
            
            return this;
        }
    }
}
