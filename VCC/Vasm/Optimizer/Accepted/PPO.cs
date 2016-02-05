using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
   public class PPO : IOptimizer
   {
       public PPO()
       {
           Level = 5;
           Priority = 2;
       }
       public int Level { get; set; }
      public int Priority { get; set; }

      Push Left = null;
      Push Right = null;
      Pop RightPop = null;
      Pop LeftPop = null;

      int LeftIdx = -1;
      int RightIdx = -1;

      int LeftPopIdx = -1;
      int RightPopIdx = -1;
    
      int CurrentIndex = -1;

      public static int Optimizations = 0;

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

      public bool Optimize(ref List<Instruction> src)
      {
   
          if (CurrentIndex == -1)
              return false;

          Instruction Operator = src[CurrentIndex];
          LeftPop = OptimizeUtils.GetLastPop(src, CurrentIndex - 1, ref LeftPopIdx);
          if (LeftPop != null)
          {
              RightPop = OptimizeUtils.GetLastPop(src, LeftPopIdx - 1, ref RightPopIdx);
              if (RightPop != null)
              {
                  Right = OptimizeUtils.GetLastOptimizablePush(src, RightPopIdx - 1, ref RightIdx);
                  if (Right != null)
                  {
                      Left = OptimizeUtils.GetLastOptimizablePush(src, RightIdx - 1, ref LeftIdx);
                      if (Left != null)
                      {
                          // optimize
                          src[LeftIdx].Emit = false;
                          src[RightIdx].Emit = false;

                          InstructionWithDestinationAndSourceAndSize mv = new Mov();
                          OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)Right, ref mv, true);
                          OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)RightPop, ref mv, false);
                          src[RightPopIdx] = mv;

                          mv = new Mov();
                          OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)Left, ref mv, true);
                          OptimizeUtils.CopyDestination((InstructionWithDestinationAndSize)LeftPop, ref mv, false);
                          src[LeftPopIdx] = mv;

                        Optimizer.Optimizations+=2;
                          Optimizations++;
                      }
                      else return false;
                  }
                  else return false;
              }
              else return false;
          }
          else return false;

          return true;

      }
      public bool Match(Instruction ins, int idx)
      {
          if (ins.OptimizingBehaviour != OptimizationKind.PPO)
              return false;

          CurrentIndex = idx;
          return true;

      }
    }
}
