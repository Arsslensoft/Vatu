using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    public interface IInstructionWithSize {
        byte Size {
            get;
            set;
        }
    }
}