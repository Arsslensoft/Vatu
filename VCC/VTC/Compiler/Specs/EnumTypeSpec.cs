using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	
    /// <summary>
    /// Enum Type Spec
    /// </summary>
    public class EnumTypeSpec : TypeSpec
    {
        public List<EnumMemberSpec> Members { get; set; }
        public bool IsFlags { get; set; }
        public EnumTypeSpec(Namespace ns, string name, int size, List<EnumMemberSpec> mem, Location loc)
            : base(ns,name, BuiltinTypes.Unknown, TypeFlags.Enum, Modifiers.NoModifier, loc)
        {
            Members = mem;
            Size = size;
            foreach (EnumMemberSpec m in mem)
                m.TypeHost = this;
          

        }

        public EnumMemberSpec ResolveMember(string name)
        {
            foreach (EnumMemberSpec kt in Members)
                if (kt.Name == name)
                    return kt;

            return null;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }
    }

	
}