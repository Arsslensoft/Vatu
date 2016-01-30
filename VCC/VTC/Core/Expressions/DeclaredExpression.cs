using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class DeclaredExpression : Expr
    {
       public Expr Expression { get; set; }

        [Rule("<Declared Expression> ::=<Method Expr>  ")]
        [Rule("<Declared Expression> ::=<Var Expr>")]
       public DeclaredExpression(Expr expr)
       {
           Expression = expr;
       }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return Expression.DoFlowAnalysis(fc);
        }
    
       public override bool EmitToStack(EmitContext ec)
       {
           return Expression.EmitToStack(ec);
       }
       public override bool EmitToRegister(EmitContext ec, Vasm.x86.RegistersEnum rg)
       {
           return Expression.EmitToRegister(ec, rg);
       }
       public override bool EmitFromStack(EmitContext ec)
       {
           return Expression.EmitFromStack(ec);
       }
       public override bool EmitBranchable(EmitContext ec, Vasm.Label truecase, bool v)
       {
           return Expression.EmitBranchable(ec, truecase, v);
       }
       public override bool Emit(EmitContext ec)
       {
           return Expression.Emit(ec);
       }
      public override bool Resolve(ResolveContext rc)
       {
           return Expression.Resolve(rc);
       }
 public override SimpleToken DoResolve(ResolveContext rc)
       {
           return Expression.DoResolve(rc);
       }
       public override string CommentString()
       {
           return Expression.CommentString();
       }
    }
}
