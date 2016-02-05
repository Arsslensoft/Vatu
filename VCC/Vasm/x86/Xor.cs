using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("xor")]
	public class Xor: InstructionWithDestinationAndSourceAndSize {
        public Xor()
        {
            OptimizingBehaviour = OptimizationKind.None;
        }
	}
}
