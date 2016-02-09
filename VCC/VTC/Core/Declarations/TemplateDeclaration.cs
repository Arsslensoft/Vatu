using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Declarations
{
    public class TemplateDeclaration : Declaration
    {
        public TemplateTypeSpec TypeName { get; set; }

        TemplateDefinition _tdl;
        ushort size = 2;
        Modifier _mod;
        [Rule(@"<Template Decl>  ::=  <Mod> <Template Def> ~';' ")]
        public TemplateDeclaration(Modifier mod, TemplateDefinition def)
        {
          
            _tdl = def;
            _mod = mod;
     
        }
        [Rule(@"<Template Decl>  ::=  <Mod> <Template Def> ~sizeof <Integral Const> ~';' ")]
        public TemplateDeclaration(Modifier mod, TemplateDefinition def, Literal lit)
        {

            _tdl = def;
            _mod = mod;
            size = ushort.Parse(lit.Value.GetValue().ToString());
        }

       public override bool Resolve(ResolveContext rc)
        {


            return true;

        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            _tdl = (TemplateDefinition)_tdl.DoResolve(rc);
            foreach (TemplateTypeSpec ts in _tdl.Templates)
            {
                TemplateTypeSpec tt = new TemplateTypeSpec(rc.CurrentNamespace, ts.Name, null, true, Location,(int)size);
                tt.Modifiers = _mod.ModifierList;

                rc.KnowType(tt);
            }


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
