using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Declarations
{
    public class OperatorDefinitionDeclaration : Declaration
    {
        OperatorSpec ops;
        string _name = null;
        OperatorDefinition opdef;
        Modifier _mod;
        bool islogic;
        [Rule(@"<Operator Definition Decl>  ::=  <Mod> ~define Id ~operator <Operator Def>  ~';' ")]
        public OperatorDefinitionDeclaration(Modifier mod, Identifier name, OperatorDefinition oper)
        {
            _name = name.Name;
            _mod = mod;
            opdef = oper;
        }
     
        // Comparison Operator
        [Rule(@"<Operator Definition Decl>   ::=  <Mod> ~define bool Id ~operator <Operator Def> ~';' ")]
        public OperatorDefinitionDeclaration(Modifier mod, SimpleToken t, Identifier name, OperatorDefinition oper)
        {
            islogic = true;
            _name = name.Name;
            _mod = mod;
            opdef = oper;
        }
     
       public override bool Resolve(ResolveContext rc)
        {


            return true;

        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            string val;
            if (opdef.IsBinary)
                val = opdef.Binary.Value.GetValue().ToString();
            else val = opdef.Unary.Value.GetValue().ToString();
            ops = new OperatorSpec(rc.CurrentNamespace, _name, val, _mod.ModifierList, loc);
            ops.IsBinary = opdef.IsBinary;
            ops.IsLogic = islogic;
            rc.Resolver.KnowOperator(ops);
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {


            return true;
        }
    }
}
