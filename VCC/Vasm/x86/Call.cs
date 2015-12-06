using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("call")]
	public class Call: JumpBase {
        public Call():base("call") {
            mNear = false;
        }
	}
}