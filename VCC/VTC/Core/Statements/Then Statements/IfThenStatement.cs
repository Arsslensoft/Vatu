using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class IfThenStatement : ThenStatement
      {
          IfElseStatement IfElse { get; set; }

     

          [Rule("<Then Stm>   ::= ~if ~'(' <Expression> ~')' <Then Stm> ~else <Then Stm> ")]
          public IfThenStatement(Expr ifexp, ThenStatement ifstmt, ThenStatement elsestmt)
          {
              IfElse = new IfElseStatement(ifexp, ifstmt, (Statement)elsestmt);

          }

          public override SimpleToken DoResolve(ResolveContext rc)
          {
 
              return IfElse.DoResolve(rc);
          }
          public override bool Emit(EmitContext ec)
          {
         
              return IfElse.Emit(ec);
          }
          public override Reachability MarkReachable(Reachability rc)
          {
              base.MarkReachable(rc);
              return IfElse.MarkReachable(rc);
          }
          public override bool DoFlowAnalysis(FlowAnalysisContext fc)
          {
              return IfElse.DoFlowAnalysis(fc);
          }
      }
   
	
}