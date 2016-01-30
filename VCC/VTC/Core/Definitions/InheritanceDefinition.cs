using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class InheritanceDefinition : Definition
    {
       public List<StructTypeSpec> Inherited { get; private set; }
       TypeIdentifierListDefinition _tidl;
       [Rule("<Inheritance> ::= ~extends <Types>")]
       public InheritanceDefinition(TypeIdentifierListDefinition tidl)
       {
           Inherited = new List<StructTypeSpec>();
           _tidl = tidl;
       }
       [Rule("<Inheritance> ::= ")]
       public InheritanceDefinition()
       {
           Inherited = new List<StructTypeSpec>();
           _tidl = null;
       }
       public override SimpleToken DoResolve(ResolveContext rc)
       {
           if (_tidl != null)
           {
               _tidl = (TypeIdentifierListDefinition)_tidl.DoResolve(rc);
               TypeIdentifierListDefinition ct = _tidl;
               while (ct != null)
               {
                   if (ct._id.Type is StructTypeSpec)
                       Inherited.Add(ct._id.Type as StructTypeSpec);
                   else ResolveContext.Report.Error(0, Location, "Can't inherited non struct based types");

                   ct = ct._nextid;
               }
           }
           return this;
       }
    }
}
