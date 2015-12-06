using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("out")]
    public class Out : InstructionWithDestinationAndSize {
        public override void WriteText( Vasm.EmitContext aAssembler, AssemblyWriter aOutput )
        {
            aOutput.Write(mMnemonic);
            aOutput.Write(" DX, ");
            aOutput.Write(this.GetDestinationAsString());
        }
	}
}