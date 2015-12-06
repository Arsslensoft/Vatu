using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    public interface IInstructionWithCondition {
        ConditionalTestEnum Condition {
            get;
            set;
        }
    }
}
