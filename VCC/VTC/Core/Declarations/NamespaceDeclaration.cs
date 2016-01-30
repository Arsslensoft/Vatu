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
            Namespace = new Namespace(id.Name, id.loc);

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Namespace.Default == Namespace)
                ResolveContext.Report.Error(0, Location, "Global namespace cannot be overriden");
            rc.Resolver.KnowNamespace(Namespace);
            return this;
        }
    }
}
