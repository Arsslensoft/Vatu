using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("mul")]
	public class Multiply: InstructionWithDestinationAndSize {

        public Multiply()
        {
            OptimizingBehaviour = OptimizationKind.None;
        }
	}
    [Vasm.OpCode("imul")]
    public class SignedMultiply : InstructionWithDestinationAndSize
    {
        public SignedMultiply()
        {
            OptimizingBehaviour = OptimizationKind.None;
        }
    }
}
