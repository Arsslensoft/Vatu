using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    public interface IInstructionWithArgument {
		Vasm.ElementReference ArgumentRef
		{
            get;
            set;
        }

		RegistersEnum? ArgumentReg
        {
            get;
            set;
        }

		uint? ArgumentValue
        {
            get;
            set;
        }

		bool ArgumentIsIndirect
		{
            get;
            set;
        }

		int ArgumentDisplacement
		{
            get;
            set;
        }

		bool ArgumentEmpty
        {
            get;
            set;
        }
    }
}