using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	/// <summary>
    /// Type Flags
    /// </summary>
    [Flags]
    public enum TypeFlags
    {
        Struct = 1,
        TypeDef = 1 << 1,
        Builtin = 1 << 2,
        Enum = 1 << 3,
        Pointer = 1 << 4,
        Array = 1 << 5,
        Missing = 1 << 6,
        Void = 1 << 7,
        Null = 1 << 8,
        Register = 1 << 9,
        Union = 1 << 10,
        Delegate = 1 << 11,
        Template = 1 << 12,
        Class = 1 <<13,
        Reference = 1 << 14
        
    }
    
	
	
}