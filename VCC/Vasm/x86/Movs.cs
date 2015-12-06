using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("movs")]
    public class Movs : InstructionWithSize, IInstructionWithPrefix {
        public InstructionPrefixes Prefixes {
            get;
            set;
        }

        public override void WriteText( Vasm.EmitContext aAssembler, AssemblyWriter aOutput )
        {
            if ((Prefixes & InstructionPrefixes.Repeat) != 0) {
                aOutput.Write("rep ");
            }
            switch (Size) {
                case 32:
                    aOutput.Write("movsd");
                    return;
                case 16:
                    aOutput.Write("movsw");
                    return;
                case 8:
                    aOutput.Write("movsb");
                    return;
                default: throw new Exception("Size not supported!");
            }
        }
    }
}