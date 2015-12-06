using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("in")]
    public class IN : InstructionWithDestinationAndSize {
        public override void WriteText( Vasm.EmitContext aAssembler, AssemblyWriter aOutput )
        {
            base.WriteText(aAssembler, aOutput);
            aOutput.Write(", DX");
        }
    }
}
