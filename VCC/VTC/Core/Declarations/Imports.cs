using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class Imports : SimpleToken
    {
        public List<Namespace> Used { get; set; }

        ImportDeclaration imd;
        Imports _next;
        [Rule("<Imports>  ::= <Import> <Imports>")]
        public Imports(ImportDeclaration im, Imports imp)
        {
           
            _next = imp;
            imd = im;

        }
        [Rule("<Imports>  ::= <Import>")]
        public Imports(ImportDeclaration im)
        {

            imd = im;



        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Used = new List<Namespace>();
            if (_next != null)
            {
                _next = (Imports)_next.DoResolve(rc);
                Used.AddRange(_next.Used);
            }

            imd = (ImportDeclaration)imd.DoResolve(rc);

 
            Used.Add(imd.Import);
       
       


            return this;
        }
    }
}
