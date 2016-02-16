using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class Global : SimpleToken
    {
        public DeclarationSequence<Declaration> Declarations { get; set; }
        public Namespace Namespace { get; set; }
        public List<Namespace> Used { get; set; }

        NamespaceDeclaration ncd;
        Imports im;
        [Rule("<GLOBAL> ::= <Namespace> ~'{' <Imports> <Decls> ~'}'")]
        public Global(NamespaceDeclaration ndcl, Imports imp, DeclarationSequence<Declaration> ds)
        {
            im = imp;
            ncd = ndcl;

            Declarations = ds;
            Namespace = ndcl.Namespace;
            Used = new List<Namespace>();

        }
        [Rule("<GLOBAL> ::=  <Decl> ")]
        public Global(Declaration ds)
        {
            Declarations = new DeclarationSequence<Declaration>(ds);
            Namespace = Namespace.Default;
            Used = new List<Namespace>();

        }
        [Rule("<GLOBAL> ::= <Namespace> ~'{' <Decls> ~'}'")]
        public Global(NamespaceDeclaration ndcl, DeclarationSequence<Declaration> ds)
        {

            ncd = ndcl;
            Declarations = ds;
            Namespace = ndcl.Namespace;
            Used = new List<Namespace>();

        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {

            return base.DoFlowAnalysis(fc);
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (ncd != null)
                ncd.DoResolve(rc);

            if (im != null)
            {
                im.DoResolve(rc);
                Used.AddRange(im.Used);
            }
            return this;
        }
    }
}
