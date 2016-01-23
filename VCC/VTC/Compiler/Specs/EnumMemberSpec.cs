using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Enum Member Spec
    /// </summary>
    public class EnumMemberSpec : MemberSpec
    {
      
        TypeSpec th;
        public bool IsAssigned { get; set; }
        public ushort Value { get; set; }
     
        public TypeSpec TypeHost
        {
            get
            {
                return th;
            }
            set { th = value; }
        }
        public Namespace NS { get; set; }
        public EnumMemberSpec(Namespace ns, string name, TypeSpec host, TypeSpec type, Location loc)
            : base(name, new MemberSignature(ns,host.Name + "_" + name, loc), Modifiers.NoModifier,ReferenceKind.EnumValue)
        {
            NS = ns;
            th = host;
            memberType = type;
            Value = 0;
            IsAssigned = false;
        }
        public EnumMemberSpec(Namespace ns, string name, ushort val, TypeSpec host, TypeSpec type, Location loc)
            : base(name, new MemberSignature(ns, host.Name + "_" + name, loc), Modifiers.NoModifier, ReferenceKind.EnumValue)
        {
            th = host;
            memberType = type;
            Value = val;
            IsAssigned = true;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }


        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = Value,Size = th.SizeInBits });
            return true;
        }
    }

	
	
}