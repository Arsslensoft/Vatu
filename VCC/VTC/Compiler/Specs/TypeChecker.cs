using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	
    public class TypeChecker
    {
        public static bool ArtihmeticsAllowed(TypeSpec a, TypeSpec b)
        {
            return  !((a.IsForeignType && !a.IsPointer) || (b.IsForeignType && !b.IsPointer));
        }
        public static bool Equals(TypeSpec a, TypeSpec b)
        {
            return a == b;
        }
        static bool IsPointerHolder(TypeSpec a, TypeSpec b)
        {
            if (a.Equals(BuiltinTypeSpec.Pointer) && b.IsPointer)
                return true;
            else if (b.Equals(BuiltinTypeSpec.Pointer) && a.IsPointer)
                return true;
            else if (a.Equals(BuiltinTypeSpec.UInt) && b.IsPointer)
                return true;
            else if (b.Equals(BuiltinTypeSpec.UInt) && a.IsPointer)
                return true;
            else return false;
        }
        public static bool CompatibleTypes(TypeSpec a, TypeSpec b)
        {
            if (a.Equals(b))
                return true;
            else if (IsPointerHolder(a, b))
                return true;
            else if ((a.IsForeignType && !a.IsPointer) || (b.IsForeignType && !b.IsPointer))
                return false;
            else if (a.IsArray || b.IsArray)
                return b.Equals(a);

            else if (a.Size > b.Size)
                return true;
            else
                return a.Size == b.Size;

       

          
        }
        public static bool BitAccessible(TypeSpec a)
        {
            return a == BuiltinTypeSpec.Byte || a == BuiltinTypeSpec.Int || a == BuiltinTypeSpec.Pointer || a == BuiltinTypeSpec.SByte || a == BuiltinTypeSpec.UInt;
        }
        public static bool ByteAccessible(TypeSpec a)
        {
            return a.IsForeignType && !a.IsPointer;
        }
        public static bool IsNumeric(TypeSpec a)
        {
            return a.IsNumeric;
        }
    }
   
	
}