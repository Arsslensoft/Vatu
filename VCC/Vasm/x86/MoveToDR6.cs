using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
  public class MoveToDR6 : Vasm.Instruction {
    protected UInt32 mValue;

    public MoveToDR6(UInt32 aValue) {
      mValue = aValue;
    }

    // This is a hack for now to just get DR6 support. DR can only be used with move, so its best to keep it a separate
    // op anwyays and not give it general support in the register list.
    // We we clean up the assemblers take this into consideration.
    public override void WriteText(AsmContext aAssembler, AssemblyWriter aOutput) {
      aOutput.WriteLine("mov DR6, " + mValue);
    }
  }
}
