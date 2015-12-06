using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    public interface IInstructionWithDestination {
        Vasm.ElementReference DestinationRef {
            get;
            set;
        }

        RegistersEnum? DestinationReg
        {
            get;
            set;
        }

        uint? DestinationValue
        {
            get;
            set;
        }

        bool DestinationIsIndirect {
            get;
            set;
        }

        int DestinationDisplacement {
            get;
            set;
        }

        bool DestinationEmpty
        {
            get;
            set;
        }
    }
}
