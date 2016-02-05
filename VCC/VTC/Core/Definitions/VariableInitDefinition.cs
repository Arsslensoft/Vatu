using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	public class VariableInitDefinition : Expr
    {

        Expr _expr = null;

         [Rule(@"<Var Init Def> ::= <Op If>")]
         [Rule(@"<Var Init Def> ::= <Initializer>")]
         [Rule(@"<Var Init Def> ::= <NDim Initializer>")]
        public VariableInitDefinition(Expr exp)
        {
            _expr = exp;
        }
      
       public override bool Resolve(ResolveContext rc)
        {
            if (_expr != null)
                _expr.Resolve(rc);


      return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_expr != null)
            {
                _expr = (Expr)_expr.DoResolve(rc);
                return _expr;
            }


            return this;
        }

     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _expr.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }

  
    }
   
}