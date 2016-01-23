using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC.Core.Expressions
{
   public class InstanceExpression : Expr
    {
       Expr _sizeexpr;
       TypeToken _type;
       MethodSpec method;
       [Rule("<Value>      ::= ~new <Type> ~'[' <Expression> ~']'")]
       public InstanceExpression(TypeToken type, Expr size)
       {
           _sizeexpr = size;
           _type = type;
       }

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           _sizeexpr = (Expr)_sizeexpr.DoResolve(rc);
           _type = (TypeToken)_type.DoResolve(rc);
           Type = _type.Type.MakePointer();


           rc.Resolver.TryResolveMethod("op_alloc_new", ref method, new TypeSpec[1] { BuiltinTypeSpec.UInt });

           if (method == null)
               ResolveContext.Report.Error(0, Location, "No operator overload for new");
           return this;
       }

       public override bool EmitToStack(EmitContext ec)
       {
           _sizeexpr.EmitToStack(ec);
           ec.EmitInstruction(new Call() { DestinationLabel = method.Signature.ToString() });
           ec.EmitPush(EmitContext.A);
           return true;
       }
       public override bool Emit(EmitContext ec)
       {
           return EmitToStack(ec);
       }
    }
}
