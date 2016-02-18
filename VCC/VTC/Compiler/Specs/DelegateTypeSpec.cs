using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	public class DelegateTypeSpec : TypeSpec
    {
        public TypeSpec ReturnType { get; set; }
        public List<TypeSpec> Parameters { get; set; }
        public List<ParameterSpec> ParametersSpecs { get; set; }
        public CallingConventions CCV { get; set; }
        public int LastParameterIdx { get; set; }
        public DelegateTypeSpec(Namespace ns, string name, TypeSpec ret, List<TypeSpec> param, CallingConventions ccv, Modifiers mods, List<ParameterSpec> parameters, Location loc)
            : base(ns, name, BuiltinTypes.Unknown, TypeFlags.Delegate | TypeFlags.Pointer, mods, loc)
        {

            Parameters = param;
            ParametersSpecs = parameters;
            Size = 0;
            ReturnType = ret;
                Size = 2;
                CCV = ccv;
        }

        public override string ToString()
        {
            return Signature.ToString();
        }

    
    }

	
	
}