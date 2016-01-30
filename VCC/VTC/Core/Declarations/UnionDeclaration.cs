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
        bool istypedef = false;
        [Rule(@"<Union Decl>  ::= <Mod> ~union Id ~'{' <Struct Def> ~'}'  ~';' ")]
        public UnionDeclaration(Modifier mod, Identifier id, StructDefinition sdef)
        {
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
        }
        [Rule(@"<Union Decl>  ::= <Mod> ~typedef ~union  ~'{' <Struct Def> ~'}' Id ~';' ")]
        public UnionDeclaration(Modifier mod, StructDefinition sdef, Identifier id)
        {
            istypedef = true;
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
            _mod = (Modifier)_mod.DoResolve(rc);
            TypeName = new UnionTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), loc);
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

            UnionTypeSpec NewType = new UnionTypeSpec(rc.CurrentNamespace, _name.Name, _def.Members, loc);
            NewType.Modifiers = _mod.ModifierList;
            foreach (int id in tobeupdated)
                _def.Members[id].MemberType.MakeBase(ref _def.Members[id].memberType, NewType);

            rc.UpdateType(TypeName, NewType);



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
