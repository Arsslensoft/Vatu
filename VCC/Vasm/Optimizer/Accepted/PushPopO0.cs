using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
 
  public  class PushPopO0 : IOptimizer
  {

      public PushPopO0()
       {
           Level = 1;
           Priority =1;
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

      public bool Optimize(ref List<Instruction> src)
      {

          if (CurrentIndex == -1)
              return false;

          LeftPopIdx = CurrentIndex;
          LeftPop = src[CurrentIndex] as Pop;
          if (LeftPop != null)
          {
              RightPop = OptimizeUtils.GetLastPop(src, CurrentIndex - 1, ref RightPopIdx);
              if (RightPop != null)
              {
                  Right = OptimizeUtils.GetLastPush(src, RightPopIdx - 1, ref RightIdx);
                  if (Right != null)
                  {
                      Left = OptimizeUtils.GetLastPush(src, RightIdx - 1, ref LeftIdx);
                      if (Left != null)
                      {
                          if ((RightPop.DestinationIsIndirect && Right.DestinationIsIndirect) || (LeftPop.DestinationIsIndirect && Left.DestinationIsIndirect))
                              return false;
                          if (OptimizeUtils.CrossUsage(Left, RightPop) || OptimizeUtils.CrossUsage(Right, LeftPop))
                              return false;
                          if (OptimizeUtils.IsSegment(Left) || OptimizeUtils.IsSegment(LeftPop) || OptimizeUtils.IsSegment(Right) || OptimizeUtils.IsSegment(RightPop))
                              return false;
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

                          Optimizer.Optimizations += 2;
                          Optimizations++;
                          Optimizer.OptimizationsSize += 6;
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
          if (ins is Pop)
          {

              CurrentIndex = idx;
              return true;
          }
          return false;
      }
    }
}
