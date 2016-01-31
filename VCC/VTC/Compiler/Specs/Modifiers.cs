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
        NoModifier = 0,
        Static = 1,
        Extern = 1 << 1,

        Prototype = 1 << 2,
       
        Const = 1 << 3,
        Private = 1 << 4,
        Ref = 1 << 5,
        Public = 1 << 6
    }

	
}