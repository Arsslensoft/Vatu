using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	 /// <summary>
    /// Builtin Types [Not like C99 Specs]
    /// </summary>
    public enum BuiltinTypes : byte
    {
        Bool=0,
        Byte = 1, // 8 bits unsigned [unsigned char]
        SByte =2, // 8 bits signed [signed char]
        Int = 3, // 32 bits signed [long int]

        UInt=4, // 32 bits unsigned
        Void=5, // void
       
        String=6,
        Pointer = 7,
        Float = 8,
        Unknown = 9
    }
    
	
}