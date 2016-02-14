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
            return a.Equals(b);
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

   public   static  bool TemplateImplicitCast(TypeSpec a, TypeSpec b)
        {
            if (a.IsForeignType && b.IsForeignType && (a.IsTemplateBased || b.IsTemplateBased) && a.Size == b.Size)
                return true;
            else return false;
        }
        public static bool CompatibleTypes(TypeSpec a, TypeSpec b)
        {
           
            if (a.Equals(b))
                return true;
            else if (IsPointerHolder(a, b))
                return true;
            else if (a.IsClass && b.IsClass && !a.IsPointer && !b.IsPointer)
            {
                ClassTypeSpec A = a as ClassTypeSpec;
                ClassTypeSpec B = b as ClassTypeSpec;
                if (IsGeneralisationOf(A, B) || IsSpecialisationOf(A, B))
                    return true;
          
                else return false;

            }
            else if (TemplateImplicitCast(a, b))
                return true;
            else if ((a.IsForeignType && !a.IsPointer) || (b.IsForeignType && !b.IsPointer))
                return false;
            else if (a.IsPointer && !a.IsArray && b.IsArray)
                return true;
            else if (a.IsArray || b.IsArray)
                return b.Equals(a);

            else if (a.Size > b.Size)
                return true;
            else
                return a.Size == b.Size;

       

          
        }
        public static bool IsGeneralisationOf(ClassTypeSpec a, ClassTypeSpec b)
        {
            // a < b 
            if (b.Inherited.Contains(a))
                return true;
            else return false;


        }
        public static bool IsSpecialisationOf(ClassTypeSpec a, ClassTypeSpec b)
        {
            // a > b 
            if (a.Inherited.Contains(b))
                return true;
            else return false;


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