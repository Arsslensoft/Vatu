using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("cmp")]
	public class Compare: InstructionWithDestinationAndSourceAndSize {
        public Compare() : base("cmp")
        {
        }
	}
}