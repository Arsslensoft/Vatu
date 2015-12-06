namespace Vasm.x86
{
    [Vasm.OpCode("NOP")]
    public class Noop : Instruction
    {
    }

    [Vasm.OpCode("NOP ; INT3")]
    public class DebugNoop : Instruction
    {
    }
}