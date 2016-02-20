using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	 /// <summary>
    /// TypeMemberSpec Specs
    /// </summary>
    public class TypeMemberSpec : MemberSpec, IEquatable<TypeMemberSpec>
    {

        public bool IsMethod { get; set; }
        public MethodSpec DefaultMethod { get; set; }
        TypeSpec th;
        public int Index { get; set; }
     
        public TypeSpec TypeHost
        {
            get
            {
                return th;
            }
        }
       
        public TypeMemberSpec(Namespace ns, string name, TypeSpec host, TypeSpec type, Location loc, int idx)
            : base(name, new MemberSignature(ns,host.Name + "_" + name, loc), Modifiers.NoModifier,ReferenceKind.Member)
        {
            th = host;
            NS = ns;
            memberType = type;
            Index = 0;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }

        public override bool EmitToStack(EmitContext ec)
        {
            
            return base.EmitToStack(ec);
        }
        public bool Equals(TypeMemberSpec m)
        {
            return m.Name == Name;
        }

    }

	
}