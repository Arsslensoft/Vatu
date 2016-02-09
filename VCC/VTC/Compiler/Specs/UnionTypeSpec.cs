using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	 /// <summary>
    ///  Struct Type Spec
    /// </summary>
    public class UnionTypeSpec : TypeSpec
    {
        public List<TypeMemberSpec> Members { get; set; }
        public List<TemplateTypeSpec> Templates { get; set; }

        public UnionTypeSpec(Namespace ns, string name, List<TypeMemberSpec> mem,List<TemplateTypeSpec> templates, Location loc)
            : base(ns, name, BuiltinTypes.Unknown, TypeFlags.Union, Modifiers.NoModifier, loc)
        {
            Members = mem;
            Size = 0;
            Templates = templates;
            foreach (TypeMemberSpec m in mem)
                if (Size < GetSize(m.MemberType))
                    Size = GetSize(m.MemberType);

        }

        public TypeMemberSpec ResolveMember(string name)
        {
            foreach (TypeMemberSpec kt in Members)
                if (kt.Name == name)
                    return kt;

            return null;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }
        public void UpdateSize()
        {
            Size = 0;
            foreach (TypeMemberSpec m in Members)
                if (Size < GetSize(m.MemberType))
                    Size = GetSize(m.MemberType);
        }
        public TypeMemberSpec GetMemberAt(int offset)
        {
            int off = 0;
            foreach (TypeMemberSpec tm in Members)
            {
                if (off == offset)
                    return tm;

                off += GetSize(tm.MemberType);
            }
            return null;
        }
        int GetTemplateIdx(TemplateTypeSpec tts)
        {
            for (int i = 0; i < Templates.Count; i++)
                if (Templates[i].Template == tts.Template)
                    return i;

            return -1;
        }
        public UnionTypeSpec CopyWithTemplate(List<TypeSpec> type)
        {
            List<TypeMemberSpec> tmp = new List<TypeMemberSpec>();
            UnionTypeSpec newsts = new UnionTypeSpec(NS, Name, new List<TypeMemberSpec>(),  new List<TemplateTypeSpec>(), Signature.Location);
            newsts.Signature = new MemberSignature(NS, Name, type.ToArray(), Signature.Location);

            foreach (TypeMemberSpec m in Members)
            {
                TypeSpec mt = m.MemberType;
                if (mt is TemplateTypeSpec)
                {
                    int idx = GetTemplateIdx(mt as TemplateTypeSpec);
                    if (idx == -1)
                        return null;
                    else mt = type[idx];
                }
                TypeMemberSpec nm = new TypeMemberSpec(m.NS, m.Name, newsts, mt, m.Signature.Location, m.Index);
                nm.Index = 0;
                tmp.Add(nm);
            }

            // new sts update
            newsts = new UnionTypeSpec(NS, Name, tmp,  new List<TemplateTypeSpec>(),Signature.Location);
            newsts.Signature = new MemberSignature(NS, Name, type.ToArray(), Signature.Location);
            newsts.UpdateSize();

            return newsts;
        }
    }
   
	
}