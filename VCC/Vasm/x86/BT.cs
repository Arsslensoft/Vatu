using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86
{
    [Vasm.OpCode("bt")]
    public class BitTest : InstructionWithDestinationAndSourceAndSize
    {
    }
    [Vasm.OpCode("btc")]
    public class BitTestAndComplement : InstructionWithDestinationAndSourceAndSize
    {
    }
    [Vasm.OpCode("btr")]
    public class BitTestAndReset : InstructionWithDestinationAndSourceAndSize
    {
    }

    [Vasm.OpCode("bts")]
    public class BitTestAndSet : InstructionWithDestinationAndSourceAndSize
    {
    }
}
