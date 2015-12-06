﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Flags]
    public enum InstructionPrefixes {
        None,
        Lock,
        Repeat,
        RepeatTillEqual
    }

    public interface IInstructionWithPrefix {
        InstructionPrefixes Prefixes {
            get;
            set;
        }
    }
}
