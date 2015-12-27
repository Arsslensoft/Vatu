using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("cmovcc")]
    public class ConditionalMove: InstructionWithDestinationAndSourceAndSize, IInstructionWithCondition {
        public ConditionalTestEnum Condition {
            get;
            set;
        }

        public override void WriteText(Vasm.AsmContext aAssembler, AssemblyWriter aOutput)
        {
            mMnemonic = "cmov" + Condition.GetMnemonic();
            base.WriteText(aAssembler, aOutput);
        }
    }

    [Vasm.OpCode("setcc")]
    public class ConditionalSet : InstructionWithDestinationAndSize, IInstructionWithCondition
    {
        public ConditionalTestEnum Condition
        {
            get;
            set;
        }

        public override void WriteText(Vasm.AsmContext aAssembler, AssemblyWriter aOutput)
        {
            mMnemonic = "set" + Condition.GetMnemonic();
            base.WriteText(aAssembler, aOutput);
        }
    }
}