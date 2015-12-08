using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86.SSE
{
    public abstract class InstructionWithDestinationAndSourceAndPseudoOpcodes : InstructionWithDestinationAndSource
    {
        public byte pseudoOpcode
        {
            get;
            set;
        }
        public override void WriteText(Vasm.AsmContext aAssembler, AssemblyWriter aOutput)
        {
            aOutput.Write(mMnemonic);
            aOutput.Write(" ");
            aOutput.Write(this.GetDestinationAsString());
            aOutput.Write(", ");
            aOutput.Write(this.GetSourceAsString());
            aOutput.Write(", ");
            aOutput.Write(this.pseudoOpcode);
        }
    }
}
