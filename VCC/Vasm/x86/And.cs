using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("and")]
	public class And: InstructionWithDestinationAndSourceAndSize {

        public And()
        {
            OptimizingBehaviour = OptimizationKind.None;
        }
	}
}
