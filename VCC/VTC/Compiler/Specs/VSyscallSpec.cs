using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Operator Spec
    /// </summary>
    public class VSyscallSpec : MemberSpec, IEquatable<VSyscallSpec>
    {
    public ushort Interrupt {get;set;}
    public FieldSpec DescriptorsField { get; set; }
    public FieldSpec DescriptorsSizeField { get; set; }
    public Dictionary<ushort,string> MethodDescriptorTable { get; set; }
  
    public VSyscallSpec(Namespace ns, string name, ushort sym, Modifiers mods, Location loc)
            : base(name, new MemberSignature(ns, name,  loc), mods, ReferenceKind.Operator)
        {
            NS = ns;
            Interrupt = sym;
            MethodDescriptorTable = new Dictionary<ushort, string>();
            DescriptorsField = new FieldSpec(NS, Name + "_DESCRITOR_TABLE", Modifiers, BuiltinTypeSpec.Pointer.MakePointer(), loc, false);
            DescriptorsSizeField = new FieldSpec(NS, Name + "_DESCRITOR_TABLE_SIZE", Modifiers, BuiltinTypeSpec.UInt, loc, false);
  
    }
        public override string ToString()
        {
            return Signature.ToString();
        }
        public bool Equals(VSyscallSpec tp)
        {
            return tp.Signature == Signature;
        }

    }
   
	
	
}