using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
    /// <summary>
    /// Label Optimizer (Removes unused labels)
    /// </summary>
   public class LabelOptimizer :  IOptimizer
    {
       public LabelOptimizer()
       {
           Level = 5;
       }
       public int Level { get; set; }
       Dictionary<string,int> _unused;
       public bool CheckForOptimization(List<Instruction> ins)
       {
           _unused = new Dictionary<string, int>();
           for (int i = 0; i < ins.Count; i++)
           {
               if (ins[i] is Label)
               {
                   Label lb = (Label)ins[i];
                   if (!_unused.ContainsKey(lb.Name))
                       _unused.Add(lb.Name,i);
               }
           }
           // Look for jumps
           foreach (Instruction inst in ins)
           {
               if (inst is Jump && _unused.ContainsKey((inst as Jump).DestinationLabel))
                   _unused.Remove((inst as Jump).DestinationLabel);
               else if (inst is ConditionalJump && _unused.ContainsKey((inst as ConditionalJump).DestinationLabel))
                   _unused.Remove((inst as ConditionalJump).DestinationLabel);
           }

           return _unused.Count > 0;
       }
       public bool Optimize(ref List<Instruction> src)
       {
           foreach (KeyValuePair<string, int> unu in _unused)
               src[unu.Value].Emit = false;
           return _unused.Count > 0;
       }
    }

}
