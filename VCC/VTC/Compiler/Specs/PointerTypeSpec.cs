using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	  /// <summary>
    /// Pointer Type
    /// </summary>
    public class PointerTypeSpec : TypeSpec, IEquatable<PointerTypeSpec>
    {
        public PointerTypeSpec(Namespace ns, TypeSpec _basetype)
            : this(ns,_basetype,0)
        {
             
            _size = 2;
        }
        public PointerTypeSpec(Namespace ns,TypeSpec _basetype,TypeFlags _flags)
            : base(ns,_basetype.Name+"*", _basetype.Size,_basetype.BuiltinType, _basetype.Flags | TypeFlags.Pointer | _flags, _basetype.Modifiers, _basetype.Signature.Location, _basetype)
        {
            _size = 2;
        }

        public PointerTypeSpec(Namespace ns, TypeSpec _basetype, TypeFlags _flags,int size)
            : base(ns, _basetype.Name + "*"+size.ToString(), _basetype.Size, _basetype.BuiltinType, _basetype.Flags | TypeFlags.Pointer | _flags, _basetype.Modifiers, _basetype.Signature.Location, _basetype)
        {
            _size = 2;
        }
        public static TypeSpec MakePointer(TypeSpec tp, int count)
        {
            if (count == 0)
                return tp;
            else return new PointerTypeSpec(tp.NS, MakePointer(tp, count - 1));
        }
       
        public bool Equals(PointerTypeSpec pt)
        {
            TypeSpec a=null, b=null;
            GetBase(this, ref a);
            GetBase(pt, ref b);
            if (a.Equals(b))
                return PointerCount(pt) == PointerCount(this);
            return false;

        }
        public int PointerCount(TypeSpec pts)
        {
            if (pts == null)
                return 0;
            if (pts.IsPointer)
                return 1 + PointerCount(pts.BaseType);
            else return 0;
        }
        public override string ToString()
        {
            return GetTypeName(this);
        }
    }

	
	
}