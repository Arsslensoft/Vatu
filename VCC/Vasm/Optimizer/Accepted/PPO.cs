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
           Level = 3;
           Priority = 1;
       }
       public int Level { get; set; }
      public int Priority { get; set; }

      Mov Move = null;
      Mov SecondMove = null;
      int MoveIdx = -1;
      int SecondMoveIdx = -1;

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
   
          if (CurrentIndex < 1)
              return false;
          if (!(src[CurrentIndex] is InstructionWithDestinationAndSourceAndSize))
              return false;
          InstructionWithDestinationAndSourceAndSize Operator = src[CurrentIndex] as InstructionWithDestinationAndSourceAndSize;
   

          Move = OptimizeUtils.GetLastMove(src, CurrentIndex - 1,ref MoveIdx) ;
          SecondMove = OptimizeUtils.GetLastMove(src, MoveIdx - 1, ref SecondMoveIdx);

          if (Move != null && SecondMove == null) // just a single mov
                      {
                          // optimize
                          src[MoveIdx].Emit = false; // eliminate mov
                          OptimizeUtils.CopySource(Move, ref Operator, true);

                        Optimizer.Optimizations+=1;
                        Optimizer.OptimizationsSize += 3;
                          Optimizations++;
                      }
          else if (Move != null && !OptimizeUtils.BothIndirect(Move, SecondMove, true)  && !OptimizeUtils.IsImmediate(SecondMove, true)) // just a single mov (s
          {
              // optimize
              src[MoveIdx].Emit = false; // eliminate mov
              src[SecondMoveIdx].Emit = false; // eliminate mov

              OptimizeUtils.CopySource(Move, ref Operator, true);
              OptimizeUtils.CopySource(SecondMove, ref Operator, false);

              Optimizer.Optimizations += 2;
              Optimizer.OptimizationsSize += 6;
              Optimizations++;
          }
                      else return false;
                 

          return true;

      }
      public bool Match(Instruction ins, int idx)
      {
          if ( ins != null && ins.OptimizingBehaviour != OptimizationKind.PPO)
              return false;

          CurrentIndex = idx;
          return true;

      }
    }
}
