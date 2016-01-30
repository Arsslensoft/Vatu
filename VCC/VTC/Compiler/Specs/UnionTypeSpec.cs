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
   

        public UnionTypeSpec(Namespace ns, string name, List<TypeMemberSpec> mem, Location loc)
            : base(ns, name, BuiltinTypes.Unknown, TypeFlags.Union, Modifiers.NoModifier, loc)
        {
            Members = mem;
            Size = 0; 
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
    }
   
	
}