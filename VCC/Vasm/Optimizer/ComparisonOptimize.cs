using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
  public  class ComparisonOptimizer : IOptimizer
    {
      public ComparisonOptimizer()
      {
          Level = 1;
          Priority = 1;

      }
      public int Priority { get; set; }
      public int Level { get; set; }
      Push GetLastPush(List<Instruction> ins, int start, ref int idx)
      {
          for (int i = start; i >= 0; i--)
          {
              if (!ins[i].Emit || ins[i] is Comment)
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
      Pop GetLastPop(List<Instruction> ins, int start, ref int idx)
      {
          for (int i = start; i >= 0; i--)
          {
              if (!ins[i].Emit || ins[i] is Comment)
                  continue;
              else if (ins[i] is Pop)
              {
                  idx = i;
                  return ins[i] as Pop;
              }
              else return null;

          }
          return null;
      }
      int FirstPushIndex = 0;
      int FirstPopIndex = 0;
      int SecondPushIndex = 0;
      int SecondPopIndex = 0;
      bool MatchComparePattern(List<Instruction> ins,int cur)
      {

          Pop first = GetLastPop(ins, cur - 1, ref FirstPopIndex), second = GetLastPop(ins, FirstPopIndex-1, ref SecondPopIndex);
          Push pfirst = GetLastPush(ins, SecondPopIndex - 1, ref FirstPushIndex), psecond = GetLastPush(ins, FirstPushIndex-1, ref SecondPushIndex);
          if (pfirst != null && psecond != null && first != null && second != null)
          {
              if (OptimizeUtils.IsDestinationMemory(pfirst) && OptimizeUtils.IsDestinationMemory(psecond))
                  return false;

              else
                  return true;
          }
          else return false;
          
      }
      public bool CheckForOptimization(List<Instruction> mInstructions, List<Instruction> externals = null)
      {
         
          return true;
      }

      public bool Optimize(ref List<Instruction> mInstructions)
      {
          for (int curpos = 0; curpos < mInstructions.Count; curpos++)
          {
          if (mInstructions[curpos] is Compare)
          {
              Compare cmp = (mInstructions[curpos] as Compare);
              if (MatchComparePattern(mInstructions, curpos))
              {
                  mInstructions[FirstPushIndex].Emit = false;
                  mInstructions[FirstPopIndex].Emit = false;
                  mInstructions[SecondPopIndex].Emit = false;
                  mInstructions[SecondPushIndex].Emit = false;

                  Compare fix = new Compare() { Size = cmp.Size };
                  InstructionWithDestinationAndSourceAndSize cm = (InstructionWithDestinationAndSourceAndSize)fix;
                  OptimizeUtils.CopyDestination(mInstructions[FirstPushIndex] as Push, ref cm, true);
                  OptimizeUtils.CopyDestination(mInstructions[SecondPushIndex] as Push, ref cm, false);
                  fix = (Compare)cm;

                  mInstructions[curpos] = cm;

                  Optimizer.Optimizations++;
              }

          }
          }
          return true;
      }
    }
}
