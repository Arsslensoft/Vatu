using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	/// <summary>
    /// Member Modifiers
    /// </summary>
    [Flags]
    public enum Modifiers
    {
        Static = 1,
        Extern = 1 << 1,
        NoModifier = 1 << 2,
        Prototype = 1 << 3,
       
        Const = 1 << 4,
        Private = 1 << 5,
        Ref = 1 << 6,
        Public = 1 << 7
    }

	
}