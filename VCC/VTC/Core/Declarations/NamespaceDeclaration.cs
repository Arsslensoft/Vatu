using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class NamespaceDeclaration : SimpleToken
    {
        public Namespace Namespace { get; set; }
        [Rule("<Namespace> ::= ~namespace <Name> ")]
        public NamespaceDeclaration(NameIdentifier id)
        {
            string parent ="global";
            if (id.Name.Contains("::"))
            {
                parent = id.Name.Substring(0, id.Name.LastIndexOf("::"));
                Namespace = new Namespace(id.Name, id.Location, new  Namespace(parent) );
            }
            else
                   Namespace = new Namespace(id.Name, id.Location);
           
        }
        Namespace GetChilds(Namespace parent, ResolveContext rc)
        {
            for (int i = 0; i < rc.Resolver.KnownNamespaces.Count;i++ )
            {
                Namespace ns = rc.Resolver.KnownNamespaces[i];
                if (ns.Name != null && ns.Name.Contains("::") && ns.Name.Substring(0, ns.Name.LastIndexOf("::")) == parent.Name)
                {
                  
                    ns.ParentNameSpace = parent;
                    rc.Resolver.KnowNamespace(ns);
                    parent.ChildNamespaces.Add(ns);
                }
                
            }
          
            return parent;
        }
  
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Namespace.Default == Namespace)
                ResolveContext.Report.Error(0, Location, "Global namespace cannot be overriden");

            Namespace ns = GetChilds(Namespace, rc);
            rc.Resolver.ForceKnowNamespace(ns);
            return this;
        }
    }
}
