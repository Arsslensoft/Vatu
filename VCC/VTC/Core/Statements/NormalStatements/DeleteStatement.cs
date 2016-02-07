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
        public MethodSpec DeleteOperator;
        [Rule("<Normal Stm> ::= ~delete <Value> ~';'")]
        public DeleteStatement(Expr id)
        {
         
            SizeExpr = id;

        }

       
       public override bool Resolve(ResolveContext rc)
        {
            SizeExpr.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.Resolver.TryResolveMethod("op_alloc_delete",ref DeleteOperator, new TypeSpec[2] { BuiltinTypeSpec.Pointer ,BuiltinTypeSpec.UInt});
            //if (rc.CurrentMethod == DeleteOperator)
            //    DeleteOperator = null;
            SizeExpr = (Expr)SizeExpr.DoResolve(rc);
            if(!SizeExpr.Type.IsPointer)
                ResolveContext.Report.Error(0, Location, "Delete operator accepts only pointer type as parameter");
            
            if (DeleteOperator == null)
                ResolveContext.Report.Error(0, Location, "Unresolved delete operator overload");
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
            ec.EmitPush((ushort)SizeExpr.Type.BaseType.Size);
            ec.EmitComment("Override Operator : Delete " + SizeExpr.CommentString());
            ec.EmitCall(DeleteOperator);
        
            return true;
        }
    }
    
}