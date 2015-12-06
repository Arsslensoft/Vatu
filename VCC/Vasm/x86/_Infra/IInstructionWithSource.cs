using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    public interface IInstructionWithSource {
        Vasm.ElementReference SourceRef {
            get;
            set;
        }

        RegistersEnum? SourceReg
        {
            get;
            set;
        }

        uint? SourceValue
        {
            get;
            set;
        }

        bool SourceIsIndirect {
            get;
            set;
        }

        int SourceDisplacement {
            get;
            set;
        }
        bool SourceEmpty
        {
            get;
            set;
        }
    }
}