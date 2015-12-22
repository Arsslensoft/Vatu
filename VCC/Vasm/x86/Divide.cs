using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
	/// <summary>
	/// Puts the result of the divide into EAX, and the remainder in EDX
	/// </summary>
    [Vasm.OpCode("div")]
	public class Divide: InstructionWithDestinationAndSize {
	}

    [Vasm.OpCode("idiv")]
    public class SignedDivide : InstructionWithDestinationAndSize
    {
    }
}