using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Base.GoldParser.Parser;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core
{
   public class SuperExpression : VariableExpression
    {
       [Rule(@"<Var Expr>     ::= super")]
       public SuperExpression(SimpleToken t)
           : base("this")
       {
           position = t.position;
       
       }
       public SuperExpression(LineInfo t)
           : base("this")
       {
           position = t;
       }

       bool GetInheritedClassIndex(ClassTypeSpec st, TypeSpec cmp, ref int off)
       {

           foreach (TypeSpec s in st.Inherited)
           {
               if (s.Equals(cmp))
                   return true;
               else
               {
                   // check child inheritance
                   int rel = 0; // relative offset for child struct

                   if ( s is ClassTypeSpec && GetInheritedClassIndex(s as ClassTypeSpec, cmp, ref rel))
                   {
                       off += rel;
                       return true;
                   }
                   else off += s.GetAllocSize(s) ;

               }
           }
           return false;
       }
       public override SimpleToken DoResolve(ResolveContext rc)
       {
       
           if (rc.CurrentType == null || !rc.CurrentType.IsClass)
               ResolveContext.Report.Error(0, Location, "Super must be used with classes");

           if (rc.CurrentExtensionLookup == null)
               ResolveContext.Report.Error(0, Location, "Super must have a parent type Type::super()");
           else
           {
              
               if (GetInheritedClassIndex(rc.CurrentType as ClassTypeSpec, rc.CurrentExtensionLookup, ref InheritanceIdx))
               {
                   if(InheritanceIdx == 0)
                   return base.DoResolve(rc);
                   else
                       base.DoResolve(rc);

               }
               else ResolveContext.Report.Error(0, Location, "The class "+rc.CurrentExtensionLookup.Name+" is not inherited by "+rc.CurrentType.Name);
           }

           return this;
       }
       int InheritanceIdx = 0;
       public override bool Resolve(ResolveContext rc)
       {
           variable = rc.Resolver.TryResolveVar("super");
           return base.Resolve(rc);
       }
       public override bool Emit(EmitContext ec)
       {
           
           base.EmitToStack(ec);
           if (InheritanceIdx > 0)
           {
               ec.EmitPop(Vasm.x86.RegistersEnum.AX);
               ec.EmitInstruction(new Add() { SourceValue = (ushort)InheritanceIdx, DestinationReg = RegistersEnum.AX });
               ec.EmitPush(RegistersEnum.AX);
           }
           return true;
       }
       public override bool EmitToStack(EmitContext ec)
       {
           return Emit(ec);
       }
       public override bool EmitBranchable(EmitContext ec, Vasm.Label truecase, bool v)
       {
           return base.EmitBranchable(ec, truecase, v);
       }

    }
}
