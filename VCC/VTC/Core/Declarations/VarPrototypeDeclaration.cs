using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Declarations
{
    public class VariablePrototypeDeclaration : Declaration
    {
        VariableDeclaration vadecl;
        [Rule(@"<Var Prototype>     ::= ~global <Var Decl>")]
        public VariablePrototypeDeclaration(VariableDeclaration v)
        {
            vadecl = v;
            v.IsAbstract = true;

        }

       public override bool Resolve(ResolveContext rc)
        {
            return vadecl.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            return vadecl.DoResolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return vadecl.Emit(ec);
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return vadecl.DoFlowAnalysis(fc);
        }
       


    }
}
