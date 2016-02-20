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
       public List<TypeSpec> InheritedClass { get; private set; }
       public ClassTypeSpec ParentClass { get; private set; }
       TypeIdentifierListDefinition _tidl;
       [Rule("<Inheritance> ::= ~extends <Types>")]
       public InheritanceDefinition(TypeIdentifierListDefinition tidl)
       {
           Inherited = new List<StructTypeSpec>();
           InheritedClass = new List<TypeSpec>();
           _tidl = tidl;
       }
       [Rule("<Inheritance> ::= ")]
       public InheritanceDefinition()
       {
           Inherited = new List<StructTypeSpec>();
           InheritedClass = new List<TypeSpec>();
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
                   if (rc.IsInStruct && ct._id.Type is StructTypeSpec)
                   {
                       if ((ct._id.Type.Modifiers & Modifiers.Sealed) == Modifiers.Sealed)
                           ResolveContext.Report.Error(0, Location, "Can't inherit a sealed struct");
                       Inherited.Add(ct._id.Type as StructTypeSpec);
                   }
               
                   else if (rc.IsInClass && ct._id.Type is ClassTypeSpec)
                   {
                       if (ParentClass == null)
                       {
                           ParentClass = ct._id.Type as ClassTypeSpec;

                           if ((ParentClass.Modifiers & Modifiers.Sealed) == Modifiers.Sealed)
                               ResolveContext.Report.Error(0, Location, "Can't inherit a sealed class");


                           if (InheritedClass.Count > 0)
                               InheritedClass.Insert(0, ct._id.Type as ClassTypeSpec);
                           else InheritedClass.Add(ct._id.Type as ClassTypeSpec);
                       }

                       else ResolveContext.Report.Error(0, Location, "Multiple class inheritance is not supported");

                   }
                   else if (rc.IsInClass && ct._id.Type is StructTypeSpec)
                       InheritedClass.Add(ct._id.Type as StructTypeSpec);
                   else ResolveContext.Report.Error(0, Location, "Can't inherited non struct or class based types");

                   ct = ct._nextid;
               }
           }
         
           return this;
       }
    }
}
