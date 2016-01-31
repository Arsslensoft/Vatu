using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
   /// <summary>
    /// Method Specs
    /// </summary>
    public class MethodSpec : MemberSpec, IEquatable<MethodSpec>
    {
        public bool IsVariadic { get; set; }
        public CallingConventions CallingConvention { get; set; }
        public List<ParameterSpec> Parameters { get; set; }
        public bool IsOperator { get; set; }
      
        public bool IsPrototype
        {
            get
            {
                return (Modifiers & Modifiers.Prototype) == Modifiers.Prototype;
            }
        }

        public MethodSpec(Namespace ns, string name, Modifiers mods, TypeSpec type,CallingConventions ccv ,TypeSpec[] param,Location loc)
            : base(name, new MemberSignature(ns, name, param,loc), mods ,ReferenceKind.Method)
        {
            IsOperator = false;
            IsVariadic = false;
            NS = ns;
            CallingConvention =ccv;
            memberType = type;
            Parameters = new List<ParameterSpec>();
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