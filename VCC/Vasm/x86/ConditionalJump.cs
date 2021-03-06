﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("jcc")]
    public class ConditionalJump: JumpBase, IInstructionWithCondition {
        public ConditionalTestEnum Condition {
            get;
            set;
        }

        public override void WriteText( Vasm.AsmContext aAssembler, AssemblyWriter aOutput )
        {
            mMnemonic = String.Intern("j" + Condition.GetMnemonic() + " ");
            base.WriteText(aAssembler, aOutput);
        }
    }
}