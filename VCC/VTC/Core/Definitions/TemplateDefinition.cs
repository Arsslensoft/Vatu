using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
 public class TemplateDefinition : Definition
    {
     public List<TemplateTypeSpec> Templates { get; set; }
     internal TemplateSequence _tid;
        [Rule(@"<Template Def> ::=   ~template ~'<' <Templates> ~'>'")]
     public TemplateDefinition(TemplateSequence tid)
        {
            _tid = tid;
        }
        [Rule(@"<Template Def> ::= ")]
        public TemplateDefinition()
        {
     
        }
       public override bool Resolve(ResolveContext rc)
        {
      
            return true;
        }
    public override SimpleToken DoResolve(ResolveContext rc)
        {
        if(_tid != null){
            _tid = (TemplateSequence)_tid.DoResolve(rc);
            Templates = new List<TemplateTypeSpec>();
           
            foreach (TemplateIdentifier t in _tid.Templates)
            {
                             
                TypeSpec ts = null;
                rc.Resolver.TryResolveType(t.Name, ref ts);
                if (ts != null)
                    ResolveContext.Report.Error(0, Location, "Duplicate template definition or type conflict");
                else Templates.Add(new TemplateTypeSpec(rc.CurrentNamespace, t.Name,t.Name[0], rc.CurrentType, rc.CurrentType == null, Location));
            }
        }
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    
	
}