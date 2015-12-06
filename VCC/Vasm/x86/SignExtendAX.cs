using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("cdq")]
    public class SignExtendAX : InstructionWithSize {
        public override void WriteText( Vasm.EmitContext aAssembler, AssemblyWriter aOutput )
        {
            switch (Size) {
                case 32:
                    aOutput.Write("cdq");
                    return;
                case 16:
                    aOutput.Write("cwde");
                    return;
                case 8:
                    aOutput.Write("cbw");
                    return;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}