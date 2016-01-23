using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Register Type
    /// </summary>
    public class RegisterTypeSpec : TypeSpec
    {
        public RegisterTypeSpec(int size)
            : base(Namespace.Default, "ASM_NATIVE_REGISTER_TYPE", BuiltinTypes.Unknown, TypeFlags.Register, Modifiers.NoModifier,Location.Null, size == 2?BuiltinTypeSpec.UInt:BuiltinTypeSpec.Byte )
        {
            _name = BaseType.Name + " register ";
        }

        public static RegisterTypeSpec RegisterByte = new RegisterTypeSpec(1);
        public static RegisterTypeSpec RegisterWord = new RegisterTypeSpec(2);
      
        public override string ToString()
        {
            return Name                ;
        }
    }
   
	
	
}