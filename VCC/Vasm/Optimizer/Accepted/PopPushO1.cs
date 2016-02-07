using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
 
  public  class PopPushO1 : IOptimizer
  {

      public PopPushO1()
       {
           Level = 2;
           Priority =2;
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

           Push OldPush = (Push)src[CurrentIndex];
           pushidx = -1;
           Pop OldPop  = OptimizeUtils.GetLastPop(src, CurrentIndex - 1, ref pushidx);

           if (OldPop == null)
               return false;
       
           // check for same operands
           if (OptimizeUtils.SameOperands(OldPop, OldPush))
           {
               src[CurrentIndex].Emit = false;
               src[pushidx].Emit = false;
               SamePushPop++;
               Optimizer.OptimizationsSize += 3;
               Optimizer.Optimizations++;
               return true;
           }



          
           return true;
       }
       public bool Match(Instruction ins, int idx)
       {
           CurrentIndex = idx;
           return (ins is Push);
           
       }


       public int Compare(IOptimizer x, IOptimizer y)
       {
           if (x.Level == y.Level)
           {
               if (x.Priority > y.Priority)
                   return 1;
               else if (x.Priority == y.Priority)
                   return 0;
               else return -1;
           }
           else if (x.Level > y.Level)
               return 1;
           else return -1;
       }
       public int CompareTo(IOptimizer y)
       {
           if (Level == y.Level)
           {
               if (Priority > y.Priority)
                   return 1;
               else if (Priority == y.Priority)
                   return 0;
               else return -1;
           }
           else if (Level > y.Level)
               return 1;
           else return -1;
       }
    }
}
