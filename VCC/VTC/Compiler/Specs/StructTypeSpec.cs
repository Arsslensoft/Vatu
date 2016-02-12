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
    public class StructTypeSpec : TypeSpec
    {
        public List<TypeMemberSpec> Members { get; set; }
        public List<StructTypeSpec> Inherited { get; private set; }
        public List<TemplateTypeSpec> Templates { get; set; }
        public StructTypeSpec Primitive { get; set; }

        public StructTypeSpec(Namespace ns,string name, List<TypeMemberSpec> mem,List<StructTypeSpec> ihd,List<TemplateTypeSpec> templates, Location loc)
            : base(ns,name, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, loc)
        {
            Members = mem;
            Size = 0;
            Templates = templates;
            Inherited = ihd;
            IsTemplateBased = templates.Count > 0;
            foreach (TypeMemberSpec m in mem)
                Size += GetSize( m.MemberType);

          
        }

       
        public void UpdateSize()
        {
            Size = 0;
            foreach (TypeMemberSpec m in Members)
                Size += GetSize(m.MemberType);
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
        public StructTypeSpec ResolveMemberHost(TypeMemberSpec m)
        {
            int start = 0;
            foreach (StructTypeSpec inh in Inherited)
            {
                if (m.Index >= start&& m.Index < start + inh.Size)
                    return inh;
            }

            return this;
        }
        int GetTemplateIdx(TemplateTypeSpec tts)
        {
            for (int i = 0; i < Templates.Count; i++)
                if (Templates[i].Template == tts.Template)
                    return i;

            return -1;
        }
        public StructTypeSpec CopyWithTemplate(List<TypeSpec> type)
        {
            List<TypeMemberSpec> tmp = new List<TypeMemberSpec>();
            StructTypeSpec newsts = new StructTypeSpec(NS,Name, new List<TypeMemberSpec>(), Inherited,new List<TemplateTypeSpec>(), Signature.Location);
            newsts.Signature = new MemberSignature(NS, Name,type.ToArray(),Signature.Location);
            newsts.Name = newsts.Signature.NoNamespaceTypeSignature;
            newsts.Primitive = this;
            int midx = 0;
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
                else if (mt.IsStruct && (mt.IsPointer || mt.IsArray))
                    mt = mt.CloneBase(mt, newsts, this);
                else if (mt.IsStruct)
                    mt = (mt as StructTypeSpec).CopyWithTemplate(type);
                else if (mt.IsUnion)
                    mt = (mt as UnionTypeSpec).CopyWithTemplate(type);


                TypeMemberSpec nm = new TypeMemberSpec(m.NS, m.Name, newsts, mt, m.Signature.Location, m.Index);
                nm.Index = midx;
                midx += nm.MemberType.GetSize(nm.MemberType);
                tmp.Add(nm);
            }

            // new sts update
            newsts = new StructTypeSpec(NS, Name, tmp, Inherited, new List<TemplateTypeSpec>(),Signature.Location);
            newsts.Signature = new MemberSignature(NS, Name, type.ToArray(), Signature.Location);
            newsts.UpdateSize();
            newsts.Name = newsts.Signature.NoNamespaceTypeSignature;
            newsts.Primitive = this;
            newsts.ExtendedFields = ExtendedFields;

            return newsts;
        }


    }

	
	
}