using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class UnionDeclaration : Declaration
    {
        public UnionTypeSpec TypeName { get; set; }
        public int Size { get; set; }
        StructDefinition _def;
        Modifier _mod;

        TemplateDefinition _tdef;
        [Rule(@"<Union Decl>  ::= <Mod> ~union Id <Template Def> ~'{' <Struct Def> ~'}' ")]
        public UnionDeclaration(Modifier mod, Identifier id, TemplateDefinition tdef, StructDefinition sdef)
        {
            _tdef = tdef;
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
        }
     
       public override bool Resolve(ResolveContext rc)
        {


            return _def.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
       {
           List<TemplateTypeSpec> templates = new List<TemplateTypeSpec>();
           _mod = (Modifier)_mod.DoResolve(rc);
           if (_tdef != null && _tdef._tid != null)
           {
               _tdef = (TemplateDefinition)_tdef.DoResolve(rc);
               templates.AddRange(_tdef.Templates);
               foreach (TemplateTypeSpec tts in _tdef.Templates)
                   rc.KnowType(tts);
           }
           else _tdef = null;


           TypeName = new UnionTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), templates, Location);
            rc.KnowType(TypeName);
            _def = (StructDefinition)_def.DoResolve(rc);
            if (_def != null)
                Size = _def.Size;
            int idx = 0;
            int i = 0;
            List<int> tobeupdated = new List<int>();
            TypeSpec ts = null;
            foreach (TypeMemberSpec m in _def.Members)
            {
                m.Index = 0;
                idx += m.MemberType.GetSize(m.MemberType);
                m.MemberType.GetBase(m.MemberType, ref ts);
                if (ts == TypeName)
                    tobeupdated.Add(i);

                i++;
            }

            UnionTypeSpec NewType = new UnionTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, templates, Location);
            NewType.Modifiers = _mod.ModifierList;
            foreach (int id in tobeupdated)
                _def.Members[id].MemberType.MakeBase(ref _def.Members[id].memberType, NewType);

            rc.UpdateType(TypeName, NewType);

            if (templates.Count > 0)
            {
                foreach (TemplateTypeSpec tts in templates)
                    rc.Resolver.KnownTypes.Remove(tts);
            }


            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _def.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {

            //ec.EmitStructDef(TypeName);

            return true;
        }
    }
}
