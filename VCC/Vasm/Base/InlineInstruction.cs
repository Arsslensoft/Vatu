using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm
{
  public  class InlineInstruction : Instruction
    {
      public string Value { get; set; }
      public InlineInstruction(string ins = "nop")
      {
          Value = ins;
      }
      public override void WriteText(AsmContext ec, AssemblyWriter aOutput)
      {
          aOutput.Write(Value);
      }
    }
}
