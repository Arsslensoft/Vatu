using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{	 
    /// <summary>
    /// Reference Type
    /// </summary>
    public class ReferenceTypeSpec : TypeSpec, IEquatable<ReferenceTypeSpec>
    {
      

        public ReferenceTypeSpec(Namespace ns, TypeSpec _basetype)
            : this(ns, _basetype, 0)
        {

            _size = 4;
        }
        public ReferenceTypeSpec(Namespace ns, TypeSpec _basetype, TypeFlags _flags)
            : base(ns, _basetype.Name + "&", _basetype.Size, _basetype.BuiltinType, _basetype.Flags | TypeFlags.Reference| _flags, _basetype.Modifiers, _basetype.Signature.Location, _basetype)
        {
            _size = 4;
        }

        public ReferenceTypeSpec(Namespace ns, TypeSpec _basetype, TypeFlags _flags, int size)
            : base(ns, _basetype.Name + "&" + size.ToString(), _basetype.Size, _basetype.BuiltinType, _basetype.Flags | TypeFlags.Reference | _flags, _basetype.Modifiers, _basetype.Signature.Location, _basetype)
        {
            _size = 4;
        }
        public static TypeSpec MakeReference(TypeSpec tp, int count)
        {
            if (count == 0)
                return tp;
            else return new ReferenceTypeSpec(tp.NS, MakeReference(tp, count - 1));
        }

        public bool Equals(ReferenceTypeSpec pt)
        {
            TypeSpec a = null, b = null;
            GetBase(this, ref a);
            GetBase(pt, ref b);
            if (a.Equals(b))
                return RefCount(pt) == RefCount(this);
            return false;

        }
        public int RefCount(TypeSpec pts)
        {
            if (pts == null)
                return 0;
            if (pts.IsReference)
                return 1 + RefCount(pts.BaseType);
            else return 0;
        }
        public override string ToString()
        {
            return GetTypeName(this);
        }
    }


	
	
}