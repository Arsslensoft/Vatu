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

       TypeToken _type;
       MethodSpec method;
       MethodSpec Ctor;
       CallingConventionsHandler ccvh;
       public List<Expr> Parameters { get; set; }
       protected ParameterSequence<Expr> _param;
       [Rule("<Value>      ::= ~new <Type> ~'(' <PARAM EXPR> ~')'")]
       public InstanceExpression(TypeToken type, ParameterSequence<Expr> p)
       {
           _type = type;
           _param = p;
       }

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           ccvh = new CallingConventionsHandler();
           _type = (TypeToken)_type.DoResolve(rc);
           Type = _type.Type.MakePointer();

           if (_type.Type.IsClass && !_type.Type.IsPointer)
               Type = _type.Type;
           else Type = _type.Type.MakePointer();

           List<TypeSpec> types = new List<TypeSpec>();
           Parameters = new List<Expr>();
           if (_param != null)
           {
               types.Add(Type);
               foreach (Expr p in _param)
               {
                   Expr e = (Expr)p.DoResolve(rc);
                   Parameters.Add(e);
                   types.Add(e.Type);
               }

           }
           // resolve ctor
           if (Type is ClassTypeSpec)
           {
             
               rc.Resolver.CurrentClassLookup = Type;
               rc.Resolver.TryResolveMethod(Type.Signature.NoNamespaceTypeSignature.Split('<')[0] + "_$ctor", ref Ctor, types.ToArray());
               rc.Resolver.CurrentClassLookup = null;
           }

           rc.Resolver.TryResolveMethod("op_alloc_new", ref method, new TypeSpec[1] { BuiltinTypeSpec.UInt });

           if (method == null)
               ResolveContext.Report.Error(0, Location, "No operator overload for new");
           if (Ctor == null && Type.IsClass && !Type.IsPointer)
               ResolveContext.Report.Error(0, Location, "No constructor found");
           return this;
       }

       public override bool EmitToStack(EmitContext ec)
       {

           ec.EmitPush((ushort)_type.Type.GetAllocSize(_type.Type));
           ec.EmitCall(method);
           ec.EmitPush(EmitContext.A);

           if (Ctor != null)
           {
               ec.EmitComment("Calling constructor");
               ec.EmitPush(EmitContext.A);
               ccvh.EmitCall(ec, Parameters, Ctor, false);
           }
           return true;
       }
       public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
       {
           if (method != null)
               fc.MarkAsUsed(method);
           return base.DoFlowAnalysis(fc);
       }
       public override bool Emit(EmitContext ec)
       {
           return EmitToStack(ec);
       }
    }
}
