using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
 
  public  class PushPopO1 : IOptimizer
  {
 
      public PushPopO1()
       {
           Level = 1;
       }
       public int Level { get; set; }

       Push GetLastPush(List<Instruction> ins, int start,ref int idx)
       {
           for (int i = start; i >= 0; i--)
           {
               if (ins[i] is Comment)
                   continue;
               else if (ins[i] is Push)
               {
                   idx = i;
                   return ins[i] as Push;
               }
               else return null;

           }
           return null;
       }
       public bool CheckForOptimization(List<Instruction> mInstructions, List<Instruction> externals = null)
       {
           return true;
           int pushidx;
           int popidx;
           for (int i = 0; i < mInstructions.Count; i++)
           {
                Instruction ins = mInstructions[i];
                if (i > 0)
                {

                    // Push Pop optimize
                    if (ins is Pop)
                    {
                        Pop OldPop = (Pop)mInstructions[i];
                        popidx = i;

                        pushidx = -1;
                        Push OldPush = GetLastPush(mInstructions, i-1 , ref pushidx) ;

                        if (OldPush == null)
                            continue;
                         
                        // check for same operands
                        if (OptimizeUtils.SameOperands(OldPop, OldPush))
                        {
                            mInstructions[popidx].Emit = false;
                            mInstructions[pushidx].Emit = false;

                            continue;
                        }
                        // Transfer to mov dst,src
                        if (OptimizeUtils.CanTransfer(OldPush, OldPop))
                        {
                            mInstructions[popidx].Emit = false;
                            mInstructions[pushidx].Emit = false;
                        
                         InstructionWithDestinationAndSourceAndSize mv = new Mov();
                         OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)OldPush, ref mv, true);
                         OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)OldPop, ref mv, false);
                         mInstructions[popidx] = mv;
                         mInstructions[pushidx] = null;
                         
                        }
                    }
                }
           }
           return true;
       }
       public bool Optimize(ref List<Instruction> src)
       {
           return true;
       
           for (int i = 0; i < src.Count; i++)
           {
               if (src[i] == null)
               {
                   src.RemoveAt(i);
                   Optimizer.Optimizations++;
               }
           }
           return true;
       }


       
    }
}
