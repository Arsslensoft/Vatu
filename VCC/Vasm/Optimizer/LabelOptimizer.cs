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
       Dictionary<string, string> lbtranslate;
     

       public string LookForRoot(string lb)
       {
           if (lbtranslate.ContainsKey(lb))
               return LookForRoot(lbtranslate[lb]);
           return lb;
       }

       public bool CheckForOptimization(List<Instruction> ins, List<Instruction> externals = null)
       {
 
           lbtranslate = new Dictionary<string, string>();

           List<string> lbr = new List<string>();
           _unused = new Dictionary<string, int>();
           Instruction last = null;
           for (int i = 0; i < ins.Count; i++)
           {
               if (ins[i] is Label)
               {
                   if (last is Label)
                   {
                       // translate last to current
                       if ( ins[i].Emit && !lbtranslate.ContainsKey((last as Label).Name ))
                           lbtranslate.Add((last as Label).Name, (ins[i] as Label).Name);
                   }
                   Label lb = (Label)ins[i];

                   if (!_unused.ContainsKey(lb.Name))
                       _unused.Add(lb.Name,i);
               }
               if(!(ins[i] is Comment) && ins[i].Emit)
               last = ins[i];
           }
         
           // Look for jumps
         for(int j = 0; j < ins.Count; j++){

             if (ins[j] is JumpBase && _unused.ContainsKey((ins[j] as JumpBase).DestinationLabel))
                   _unused.Remove((ins[j] as JumpBase).DestinationLabel);
                 
               
               //else if (inst is ConditionalJump && _unused.ContainsKey((inst as ConditionalJump).DestinationLabel))
               //    _unused.Remove((inst as ConditionalJump).DestinationLabel);
               //else if (inst is Call && _unused.ContainsKey((inst as Call).DestinationLabel))
               //    _unused.Remove((inst as Call).DestinationLabel);
             else if (ins[j] is InlineInstruction)
               {
                   InlineInstruction iins = (InlineInstruction)ins[j];

                   foreach (KeyValuePair<string, int> kp in _unused)
                   {
                       if (iins.Value.Contains(kp.Key))
                       {
                           if (!lbr.Contains(kp.Key))
                               lbr.Add(kp.Key);
                       }
                   }

               }
           }

          // look for external asm
           foreach (Instruction inst in externals)
           {
               InlineInstruction iins = (InlineInstruction)inst;
               foreach (KeyValuePair<string, int> kp in _unused)
               {
                   if (iins.Value.Contains(kp.Key))
                   {
                       if (!lbr.Contains(kp.Key))
                           lbr.Add(kp.Key);
                   }
               }
           }

           // remove labels
           foreach (string lb in lbr)
               _unused.Remove(lb);
           return _unused.Count > 0;
       }
       public bool Optimize(ref List<Instruction> src)
       {
           foreach (KeyValuePair<string, int> unu in _unused)
               if (!lbtranslate.ContainsValue(unu.Key))
                   src[unu.Value].Emit = false;
              

          for(int j = 0; j < src.Count;j++)
           {
               if (src[j] is JumpBase)
               {
                   string lb = (src[j] as JumpBase).DestinationLabel;
                   (src[j] as JumpBase).DestinationLabel = LookForRoot(lb);
                   if (lb != (src[j] as JumpBase).DestinationLabel)
                       Optimizer.Optimizations++;

               }
               else if (src[j] is Label && LookForRoot((src[j] as Label).Name) != (src[j] as Label).Name)
               {
                src[j].Emit = false;
                   Optimizer.Optimizations++;
               }
           }
           Optimizer.Optimizations += _unused.Count;
           return _unused.Count > 0;
       }
    }

}
