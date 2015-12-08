namespace Vasm.x86 {
    [Vasm.OpCode("int")]
    public class INT : InstructionWithDestination {
        public override void WriteText( Vasm.AsmContext aAssembler, AssemblyWriter aOutput )
        {
          //TODO: In base have a property that has the opcode from above and we can reuse it.
            aOutput.Write("Int " + DestinationValue);
        }
    }
}