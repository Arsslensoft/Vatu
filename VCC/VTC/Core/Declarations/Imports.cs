using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class Imports : SimpleToken
    {
        public List<Namespace> Used { get; set; }
        [Rule("<Imports>  ::= <Import> <Imports>")]
        public Imports(ImportDeclaration im, Imports imp)
        {
            Used = new List<Namespace>();
            Used.Add(im.Import);
            foreach (Namespace id in imp.Used)
                Used.Add(id);

        }
        [Rule("<Imports>  ::= <Import>")]
        public Imports(ImportDeclaration im)
        {
            Used = new List<Namespace>();
            Used.Add(im.Import);


        }
    }
}
