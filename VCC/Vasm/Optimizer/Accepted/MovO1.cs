using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
 
  public  class MovO1 : IOptimizer
  {

      public MovO1()
       {
           Level = 2;
           Priority = 3;
       }
       public int Level { get; set; }
      public int Priority { get; set; }


       public static int SameMov = 0;
       int CurrentIndex = -1;


       public bool Optimize(ref List<Instruction> src)
       {
           int pushidx;
  
           if (CurrentIndex == -1)
               return false;

           Mov OldMov = (Mov)src[CurrentIndex];
     if (OldMov.DestinationIsIndirect && OldMov.SourceIsIndirect)
               return false ;
           // check for same operands
           if (OptimizeUtils.SameOperands(OldMov)  )
           {
               src[CurrentIndex].Emit = false;
               Optimizer.Optimizations++;
               Optimizer.OptimizationsSize += 3;
               SameMov++;
               return true;
           }
           return true;
       }
       public bool Match(Instruction ins, int idx)
       {
           CurrentIndex = idx;
           return (ins is Mov);
           
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
