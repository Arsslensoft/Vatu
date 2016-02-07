using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC.Core
{
   public class InstanceExpression : Expr
    {
       Expr _sizeexpr;
       TypeToken _type;
       MethodSpec method;
       [Rule("<Value>      ::= ~new <Type> ~'(' <Expression> ~')'")]
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
           ec.EmitPop(EmitContext.A);
           ec.EmitMovToRegister(EmitContext.C, (ushort)_type.Type.GetSize(_type.Type));
           ec.EmitInstruction(new Multiply() { DestinationReg = EmitContext.C});
           ec.EmitPush(EmitContext.A);
           ec.EmitInstruction(new Call() { DestinationLabel = method.Signature.ToString() });
           ec.EmitPush(EmitContext.A);
           return true;
       }
       public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
       {
           if (method != null)
               fc.MarkAsUsed(method);
           return _sizeexpr.DoFlowAnalysis(fc);
       }
       public override bool Emit(EmitContext ec)
       {
           return EmitToStack(ec);
       }
    }
}
