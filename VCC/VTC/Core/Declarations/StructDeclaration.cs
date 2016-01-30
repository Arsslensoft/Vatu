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
        bool istypedef = false;
        [Rule(@"<Struct Decl>  ::= <Mod> ~struct Id <Inheritance> ~'{' <Struct Def> ~'}'  ~';' ")]
        public StructDeclaration(Modifier mod, Identifier id,InheritanceDefinition ihd, StructDefinition sdef)
        {
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
            _ihd = ihd;
        }
        [Rule(@"<Struct Decl>  ::= <Mod> ~typedef ~struct  <Inheritance>  ~'{' <Struct Def> ~'}' Id ~';' ")]
        public StructDeclaration(Modifier mod,InheritanceDefinition ihd, StructDefinition sdef, Identifier id)
        {
            istypedef = true;
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
            _mod = (Modifier)_mod.DoResolve(rc);
            if (_ihd != null)
            {
                _ihd = (InheritanceDefinition)_ihd.DoResolve(rc);
                TypeName = new StructTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), _ihd.Inherited, loc);
            }
            else TypeName = new StructTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), new List<StructTypeSpec>(), loc);
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
                       
                        TypeMemberSpec newm = new TypeMemberSpec(TypeName.NS, m.Name, TypeName, m.memberType, loc, idx);
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
             NewType = new StructTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, _ihd.Inherited, loc);
            else NewType = new StructTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members,new List<StructTypeSpec>(), loc);
           
            NewType.Modifiers = _mod.ModifierList;
            foreach (int id in tobeupdated)
                _def.Members[id].MemberType.MakeBase(ref _def.Members[id].memberType, NewType);


            // insert inherited
            _def.Members.InsertRange(0, members);
            // Update Size
            NewType.UpdateSize();
            rc.UpdateType(TypeName, NewType);


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
