using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class StructDeclaration : Declaration
    {
        public StructTypeSpec TypeName { get; set; }
        public int Size { get; set; }
        StructDefinition _def;
        InheritanceDefinition _ihd;
        Modifier _mod;
        TemplateDefinition _tdef;
     
        [Rule(@"<Struct Decl>  ::= <Mod> ~struct Id <Template Def> <Inheritance> ~'{' <Struct Def> ~'}'   ")]
        public StructDeclaration(Modifier mod, Identifier id, TemplateDefinition tdef, InheritanceDefinition ihd, StructDefinition sdef)
        {
            _tdef = tdef;
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
            _ihd = ihd;
        }
     
       
       public override bool Resolve(ResolveContext rc)
        {


            return _def.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            List<TypeMemberSpec> members = new List<TypeMemberSpec>();
            List<TemplateTypeSpec> templates = new List<TemplateTypeSpec>();
            _mod.AllowSealedModifier = true;
            _mod = (Modifier)_mod.DoResolve(rc);
            TypeSpec type = null;
            if (rc.Resolver.TryResolveType(_name.Name, ref type))
                ResolveContext.Report.Error(0, Location, "Duplicate type declaration ");
            if (_tdef != null && _tdef._tid != null)
            {
                _tdef = (TemplateDefinition)_tdef.DoResolve(rc);
                templates.AddRange(_tdef.Templates);
                foreach(TemplateTypeSpec tts in _tdef.Templates)
                rc.KnowType(tts);
            }
            else _tdef = null;


            if (_ihd != null)
            {
                _ihd = (InheritanceDefinition)_ihd.DoResolve(rc);
                TypeName = new StructTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), _ihd.Inherited, templates, Location);
            }
            else TypeName = new StructTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), new List<StructTypeSpec>(), templates, Location);
            rc.KnowType(TypeName);
            _def = (StructDefinition)_def.DoResolve(rc);
            if (_def != null)
                Size = _def.Size;

            int idx = 0;

            // Copy Members of inherited
            if (_ihd != null)
            {
                foreach (StructTypeSpec st in _ihd.Inherited)
                {
                    foreach (TypeMemberSpec m in st.Members)
                    {
                       
                        TypeMemberSpec newm = new TypeMemberSpec(TypeName.NS, m.Name, TypeName, m.memberType, Location, idx);
                        newm.Index = idx;
                        idx += newm.MemberType.GetSize(newm.MemberType);
                        if (members.Contains(newm))
                            ResolveContext.Report.Error(0, Location, "Duplicate member declaration, " + newm.Name + " is already defined in " + st.Name);

                        members.Add(newm);
                    }
                }
            }

            // Duplicate names check

            int i = 0;
            List<int> tobeupdated = new List<int>();
            TypeSpec ts = null;
            foreach (TypeMemberSpec m in _def.Members)
            {

                m.Index = idx;
                idx += m.MemberType.GetSize(m.MemberType);
                // used for recursive declarations
                m.MemberType.GetBase(m.MemberType, ref ts);
                if (ts == TypeName)
                    tobeupdated.Add(i);

                if (members.Contains(m))
                    ResolveContext.Report.Error(0, Location, "Duplicate member declaration, " + m.Name + " is already defined in " + TypeName.Name);
                i++;
            }
                StructTypeSpec NewType = null;

       
            if(_ihd != null)
             NewType = new StructTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, _ihd.Inherited,templates, Location);
            else NewType = new StructTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, new List<StructTypeSpec>(), templates, Location);
           
            NewType.Modifiers = _mod.ModifierList;
            foreach (int id in tobeupdated)
                _def.Members[id].MemberType.MakeBase(ref _def.Members[id].memberType, NewType);


            // insert inherited
            _def.Members.InsertRange(0, members);
            // Update Size
            NewType.UpdateSize();
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

            ec.EmitStructDef(TypeName);

            return true;
        }
    }
}
