using System;
using System.Linq;

namespace Vasm.x86 {
    /// <summary>
    /// Subtracts the source operand from the destination operand and 
    /// replaces the destination operand with the result. 
    /// </summary>
    [Vasm.OpCode("sub")]
    public class Sub : InstructionWithDestinationAndSourceAndSize {
    }
}