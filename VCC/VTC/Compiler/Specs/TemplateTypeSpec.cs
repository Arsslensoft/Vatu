using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	public class TemplateTypeSpec : TypeSpec
    {
        public char Template { get; set; }
        public TypeSpec HostType{get;set;}
        public bool IsGlobal {get;set;}
        public TemplateTypeSpec(Namespace ns, string name,TypeSpec host, bool isglobal, Location loc,int size = 2)
            : base(ns, name, BuiltinTypes.Unknown, TypeFlags.Template, Modifiers.NoModifier, loc)
        {
            Template = name[0];
                _size = size;
                IsGlobal = isglobal;
                HostType = host;
        }

        public override string ToString()
        {
            return Signature.ToString();
        }

    
    }

	
	
}