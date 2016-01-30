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
       
        public StructTypeSpec(Namespace ns,string name, List<TypeMemberSpec> mem,List<StructTypeSpec> ihd, Location loc)
            : base(ns,name, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, loc)
        {
            Members = mem;
            Size = 0;

            Inherited = ihd;

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
    }

	
	
}