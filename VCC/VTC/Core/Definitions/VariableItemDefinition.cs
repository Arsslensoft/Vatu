using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	public class VariableItemDefinition : Definition
    {
        public bool IsAbstract=false;
        TypePointer _ptr;
        public VariableDefinition _vardef;
        [Rule(@"<Var Item> ::= <Pointers> <Var>")]
        public VariableItemDefinition(TypePointer ptr, VariableDefinition var)
        {
           
            _vardef = var;
            _ptr = ptr;
        }
       public override bool Resolve(ResolveContext rc)
        {
           return _vardef.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _vardef.IsAbstract = IsAbstract;
            _vardef = (VariableDefinition)_vardef.DoResolve(rc);
            _ptr = (TypePointer)_ptr.DoResolve(rc);
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _vardef.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    
}