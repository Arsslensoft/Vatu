using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{

    /// <summary>
    /// Jump Optimizer
    /// </summary>
  public  class JumpOptimizer:  IOptimizer
  {
      /*
       * jne label
       * label:
       *    jmp lb2
       *    
       * jne lb2
       * 
       * */
      public int Level { get; set; }
      public JumpOptimizer()
      {
          Level = 4;
      }
       Dictionary<string, string> lbtranslate;
       List<int> ToBeCorrected;

       public bool CheckForOptimization(List<Instruction> ins, List<Instruction> externals = null)
       {
           ToBeCorrected = new List<int>();
           lbtranslate = new Dictionary<string, string>();
           // look for label: jmp label2 => queue for removal and translate label to label2
           for (int i = 0; i < ins.Count-1; i++)
           {
               if (ins[i] is Label && ins[i + 1] is Jump && !lbtranslate.ContainsKey((ins[i] as Label).Name))
               {
                   lbtranslate.Add((ins[i] as Label).Name, (ins[i + 1] as Jump).DestinationLabel);
                   ToBeCorrected.Add(i);
               }
           }

           return ToBeCorrected.Count > 0;
       }
       public bool Optimize(ref List<Instruction> src)
       {
           foreach (int unu in ToBeCorrected)
           {
               src[unu].Emit = false;
               src[unu+1].Emit = false;
           }
           // translate labels
           for (int i = 0; i < src.Count; i++)
           {
               if (src[i] is ConditionalJump)
               {
                   ConditionalJump cjmp = src[i] as ConditionalJump;
                   if (lbtranslate.ContainsKey(cjmp.DestinationLabel))
                   {
                       cjmp.DestinationLabel = lbtranslate[cjmp.DestinationLabel];
                       src[i] = cjmp;
                   }
               }
               else if (src[i] is ConditionalJump)
               {
                   Jump jmp = src[i] as Jump;
                   if (lbtranslate.ContainsKey(jmp.DestinationLabel))
                   {
                       jmp.DestinationLabel = lbtranslate[jmp.DestinationLabel];
                       src[i] = jmp;
                   }
               }
           }
           Optimizer.Optimizations += ToBeCorrected.Count;
           return ToBeCorrected.Count > 0;
       }
    }
   
}
