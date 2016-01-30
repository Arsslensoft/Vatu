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
    public class OperatorSpec : MemberSpec, IEquatable<MethodSpec>
    {
        public bool IsLogic { get; set; }
        public bool IsBinary { get; set; }
   
        public string Symbol { get; set; }
        public OperatorSpec(Namespace ns, string name,string sym, Modifiers mods, Location loc)
            : base(name, new MemberSignature(ns, name,  loc), mods, ReferenceKind.Operator)
        {
            NS = ns;
            Symbol = sym;
  
        }
        public override string ToString()
        {
            return Signature.ToString();
        }
        public bool Equals(MethodSpec tp)
        {
            return tp.Signature == Signature;
        }

    }
   
	
	
}