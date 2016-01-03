using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("ret")]
	public class Return: InstructionWithDestination {
        public Return() {
            DestinationValue = 0;
        }
    }
    [Vasm.OpCode("ret")]
    public class SimpleReturn : Instruction
    {
        public SimpleReturn()
        {
       
        }
    }
}