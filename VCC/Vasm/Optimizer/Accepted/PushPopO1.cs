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
           Priority = 5;
       }
       public int Level { get; set; }
      public int Priority { get; set; }


       public static int SamePushPop = 0;
       public static int PushPopCount = 0;


       int CurrentIndex = -1;


       public bool Optimize(ref List<Instruction> src)
       {
           int pushidx;
  
           if (CurrentIndex == -1)
               return false;

           Pop OldPop = (Pop)src[CurrentIndex];
           pushidx = -1;
           Push OldPush = OptimizeUtils.GetLastPush(src,CurrentIndex - 1, ref pushidx);

           if (OldPush == null)
               return false;
           else if (OldPop.DestinationIsIndirect && OldPush.DestinationIsIndirect)
               return false ;

           // check for same operands
           if (OptimizeUtils.SameOperands(OldPop, OldPush))
           {
               src[CurrentIndex].Emit = false;
               src[pushidx].Emit = false;
               SamePushPop++;
               Optimizer.Optimizations++;
               return true;
           }



           // Transfer to mov dst,src
           if (OptimizeUtils.CanTransfer(OldPush, OldPop))
           {
               src[CurrentIndex].Emit = false;
               src[pushidx].Emit = false;

               InstructionWithDestinationAndSourceAndSize mv = new Mov();
               OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)OldPush, ref mv, true);
               OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)OldPop, ref mv, false);
               src[CurrentIndex] = mv;
               src[pushidx] = null;

               PushPopCount++;
               Optimizer.Optimizations++;

           }

           return true;
       }
       public bool Match(Instruction ins, int idx)
       {
           CurrentIndex = idx;
           return (ins is Pop);
           
       }


       public int Compare(IOptimizer x, IOptimizer y)
       {
           if (x.Priority > y.Priority)
               return 1;
           else if (x.Priority == y.Priority)
               return 0;
           else return -1;
       }
       public int CompareTo(IOptimizer y)
       {
           if (Priority > y.Priority)
               return 1;
           else if (Priority == y.Priority)
               return 0;
           else return -1;
       }
    }
}
