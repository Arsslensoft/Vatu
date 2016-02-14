using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	 public class DeleteStatement : NormalStatment
    {
        public Expr SizeExpr { get; set; }
        MethodSpec Dtor;
        CallingConventionsHandler ccvh;
        public List<Expr> Parameters { get; set; }
        protected ParameterSequence<Expr> _param;
        public MethodSpec DeleteOperator;
        [Rule("<Normal Stm> ::= ~delete <Var Expr> ~';'")]
        public DeleteStatement(Expr id)
        {
         
            SizeExpr = id;

        }

        [Rule("<Normal Stm> ::= ~delete <Var Expr>  ~'(' <PARAM EXPR> ~')' ~';'")]
        public DeleteStatement(Expr id, ParameterSequence<Expr> p)
        {

            SizeExpr = id;
            _param = p;
        }
       public override bool Resolve(ResolveContext rc)
        {
            SizeExpr.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
       {
           ccvh = new CallingConventionsHandler();
            List<TypeSpec> types = new List<TypeSpec>();
            rc.Resolver.TryResolveMethod("op_alloc_delete",ref DeleteOperator, new TypeSpec[2] { BuiltinTypeSpec.Pointer ,BuiltinTypeSpec.UInt});
            //if (rc.CurrentMethod == DeleteOperator)
            //    DeleteOperator = null;
            SizeExpr = (Expr)SizeExpr.DoResolve(rc);
            if (SizeExpr.Type.IsClass && !SizeExpr.Type.IsPointer)
            {
                if (_param == null)
                    ResolveContext.Report.Error(0, Location, "Delete must reference the classes destructor use 'delete <var>()' instead");
                else
                {
                    Parameters = new List<Expr>();
                 
                        types.Add(SizeExpr.Type);
                        foreach (Expr p in _param)
                        {
                            Expr e = (Expr)p.DoResolve(rc);
                            Parameters.Add(e);
                            types.Add(e.Type);
                        }

                    
                }
            }
            else if (!SizeExpr.Type.IsPointer)
                ResolveContext.Report.Error(0, Location, "Delete operator accepts only pointer or class type as parameter");


            // resolve ctor
            if (SizeExpr.Type is ClassTypeSpec)
            {

                rc.Resolver.CurrentClassLookup = SizeExpr.Type;
                rc.Resolver.TryResolveMethod(SizeExpr.Type.Signature.NoNamespaceTypeSignature.Split('<')[0] + "_$dtor", ref Dtor, types.ToArray());
                rc.Resolver.CurrentClassLookup = null;
            }
            if (DeleteOperator == null)
                ResolveContext.Report.Error(0, Location, "Unresolved delete operator overload");

            if (Dtor == null && SizeExpr.Type.IsClass && !SizeExpr.Type.IsPointer)
                ResolveContext.Report.Error(0, Location, "No destructor found");
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            fc.MarkAsUsed(DeleteOperator);
            return SizeExpr.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            SizeExpr.EmitToStack(ec);
            if (Dtor != null)
            {
                ec.EmitComment("Calling destructor");
                SizeExpr.EmitToStack(ec);           
                ccvh.EmitCall(ec, Parameters, Dtor);
            }

            if (SizeExpr.Type.IsClass && !SizeExpr.Type.IsPointer)
                ec.EmitPush((ushort)SizeExpr.Type.GetAllocSize(SizeExpr.Type));
            else
                ec.EmitPush((ushort)SizeExpr.Type.BaseType.GetAllocSize(SizeExpr.Type.BaseType));

            ec.EmitComment("Override Operator : Delete " + SizeExpr.CommentString());
            ec.EmitCall(DeleteOperator);

            // make class null
            if (Dtor != null)
            {
                ec.EmitPush((ushort)0);
                SizeExpr.EmitFromStack(ec);
            }


            return true;
        }
    }
    
}