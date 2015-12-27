using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("push")]
    public class Push : InstructionWithDestinationAndSize {

        public Push():base("push") {
            Size = 32;
        }
    }
    [Vasm.OpCode("org")]
    public class Org : InstructionWithDestinationAndSize
    {

        public Org()
            : base("org")
        {
            Size = 80;
        }
    }
}
