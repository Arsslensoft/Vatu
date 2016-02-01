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
  public class InterruptDef
  {
      public ushort Number { get; set; }
      public Label Destination { get; set; }
  }
  public class InstallINTInstruction : Instruction
  {
      public string Value { get; set; }
      public InstallINTInstruction(List<InterruptDef> interrupts, string name)
      {
          StringBuilder sb = new StringBuilder();
          sb.AppendLine(name+":");
          sb.AppendLine("\t\tcli");
          sb.AppendLine("\t\tpush es");
          sb.AppendLine("\t\txor     ax, ax");
          sb.AppendLine("\t\tmov     es, ax");
          foreach (InterruptDef idef in interrupts)
          {
              sb.AppendLine(string.Format("\t\t;Installing interrupt {0}", idef.Number));
              sb.AppendLine(string.Format("\t\tmov     dx, {0}", idef.Destination.Name));
              sb.AppendLine(string.Format("\t\tmov     [es:{0}*4], dx", idef.Number));
              sb.AppendLine("\t\tmov     ax, ds"); // changed cs -> ds
              sb.AppendLine(string.Format("\t\tmov     [es:{0}*4+2], ax", idef.Number));
          }
          sb.AppendLine("\t\tpop es");
          sb.AppendLine("\t\tsti");
          sb.AppendLine("\t\tret");

          Value = sb.ToString();
      }
      public override void WriteText(AsmContext ec, AssemblyWriter aOutput)
      {
          aOutput.Write(Value);
      }
  }
}
