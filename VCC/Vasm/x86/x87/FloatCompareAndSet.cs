using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86.x87
{
	/// <summary>
	/// ST(0) > ST(i): ZF, PF, CF = 000;
	/// ST(0) < ST(i): ZF, PF, CF = 001;
	/// ST(0) = ST(i): ZF, PF, CF = 100;
	/// </summary>
    [Vasm.OpCode("fcomi")]
    public class FloatCompareAndSet : InstructionWithDestination
    {
    }
}
