using System;
using System.Linq;

namespace Vasm.x86 {
    [Vasm.OpCode("pop")]
	public class Pop: InstructionWithDestinationAndSize{
        public Pop() : base("pop")
        {
        }
	}

}