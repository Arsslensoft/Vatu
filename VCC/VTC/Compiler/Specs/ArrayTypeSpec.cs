using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Array type
    /// </summary>
    public class ArrayTypeSpec : PointerTypeSpec, IEquatable<TypeSpec>
    {
        public int ArrayCount { get; set; }
        public ArrayTypeSpec(Namespace ns, TypeSpec _basetype, int size)
            : base(ns,_basetype, TypeFlags.Array, size)
        {
            ArrayCount = size;
         
            _size = _basetype.Size;
        }
        public bool Equals(TypeSpec ar)
        {
            if (ar.BaseType == this.BaseType && ar == BuiltinTypeSpec.String)
                return true;
            else return false;

        }
        public override string ToString()
        {
            return GetTypeName(this);
        }
    }

	
	
}