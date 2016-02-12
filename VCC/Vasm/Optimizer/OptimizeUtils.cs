using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
  public  class OptimizeUtils
    {
      public static bool CrossUsage(InstructionWithDestinationAndSize a, InstructionWithDestinationAndSize b)
      {
          if (!a.DestinationReg.HasValue || !b.DestinationReg.HasValue)
              return false;
              return a.DestinationReg.Value == b.DestinationReg.Value;
     
      }
      public static bool IsSegment(InstructionWithDestinationAndSize ins)
      {
          if (ins.DestinationReg.HasValue && Registers.IsSegment(ins.DestinationReg.Value))
              return true;
          else return false;

      }
      public static bool BothIndirect(InstructionWithDestinationAndSourceAndSize a, InstructionWithDestinationAndSourceAndSize b,bool src)
      {
          if (src)
              return a.SourceIsIndirect && b.SourceIsIndirect;
          else return b.DestinationIsIndirect && a.DestinationIsIndirect;
      }
      public static bool IsImmediate(InstructionWithDestinationAndSourceAndSize a,bool src)
      {
          if (src)
              return a.SourceValue.HasValue || (!a.SourceIsIndirect && a.SourceRef != null);
          else return a.DestinationValue.HasValue || (!a.DestinationIsIndirect && a.DestinationRef != null);
      }
      
      public static Mov GetLastMove(List<Instruction> ins, int start, ref int idx)
      {
          if (start <= 0 || start >= ins.Count || ins == null)
              return null;
          for (int i = start; i > 0; i--)
          {
              if (ins[i] == null)
                  continue;
              if (!ins[i].Emit || ins[i] is Comment)
                  continue;
              else if (ins[i] is Mov)
              {
                  idx = i;
                  return ins[i] as Mov;
              }
              else return null;

          }
          return null;
      }
       public static Push GetLastPush(List<Instruction> ins, int start, ref int idx)
      {
          if (start <= 0 || start >= ins.Count || ins == null)
              return null;
          for (int i = start; i > 0; i--)
          {
              if (ins[i] == null)
                  continue;
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
       public static Push GetLastOptimizablePush(List<Instruction> ins, int start, ref int idx)
       {
           if (start <= 0 || start >= ins.Count || ins == null)
               return null;
           for (int i = start; i > 0; i--)
           {
               if (ins[i] == null)
                   continue;
               if (!ins[i].Emit || ins[i] is Comment)
                   continue;
               else if (ins[i] is Push && ins[i].IsOperationPush)
               {
                   idx = i;
                   return ins[i] as Push;
               }
               


           }
           return null;
       }
      public static  Pop GetLastPop(List<Instruction> ins, int start, ref int idx)
        {
            if (start <= 0 || start >= ins.Count || ins == null)
                return null;
            for (int i = start; i > 0; i--)
            {
                if (ins[i] == null)
                    continue;
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
      public static bool IsDestinationRegister(InstructionWithDestinationAndSize ins)
      {
          return ins.DestinationReg.HasValue;
      }
      public static bool CopyDestination(InstructionWithDestinationAndSize src,ref InstructionWithDestinationAndSourceAndSize ins, bool assrc)
      {
          if (assrc && !src.DestinationEmpty)
          {
              ins.SourceReg = src.DestinationReg;
              ins.SourceValue = src.DestinationValue;
              ins.SourceRef = src.DestinationRef;
              ins.Size = src.Size;
              ins.SourceDisplacement = src.DestinationDisplacement;
              ins.SourceIsIndirect = src.DestinationIsIndirect;
              return true;
          }
          else if(!src.DestinationEmpty)
          {
              ins.DestinationReg = src.DestinationReg;
              ins.DestinationValue = src.DestinationValue;
              ins.DestinationRef = src.DestinationRef;
              ins.Size = src.Size;
              ins.DestinationDisplacement = src.DestinationDisplacement;
              ins.DestinationIsIndirect = src.DestinationIsIndirect;
              return true;
          }
          return false;
      }
      public static bool CopyDestination(Mov src, ref InstructionWithDestinationAndSourceAndSize ins, bool assrc)
      {
          if (assrc && !src.DestinationEmpty)
          {
              ins.SourceReg = src.DestinationReg;
              ins.SourceValue = src.DestinationValue;
              ins.SourceRef = src.DestinationRef;
              ins.Size = src.Size;
              ins.SourceDisplacement = src.DestinationDisplacement;
              ins.SourceIsIndirect = src.DestinationIsIndirect;
              return true;
          }
          else if (!src.DestinationEmpty)
          {
              ins.DestinationReg = src.DestinationReg;
              ins.DestinationValue = src.DestinationValue;
              ins.DestinationRef = src.DestinationRef;
              ins.Size = src.Size;
              ins.DestinationDisplacement = src.DestinationDisplacement;
              ins.DestinationIsIndirect = src.DestinationIsIndirect;
              return true;
          }
          return false;
      }
      public static bool CopySource(Mov src, ref InstructionWithDestinationAndSourceAndSize ins, bool assrc)
      {
          if (assrc && !src.DestinationEmpty)
          {
              ins.SourceReg = src.SourceReg;
              ins.SourceValue = src.SourceValue;
              ins.SourceRef = src.SourceRef;
          if(src.SourceReg.HasValue)
                ins.Size = Registers.GetSize(src.SourceReg.Value);
              ins.SourceDisplacement = src.SourceDisplacement;
              ins.SourceIsIndirect = src.SourceIsIndirect;
              return true;
          }
          else if (!src.DestinationEmpty)
          {
              ins.DestinationReg = src.SourceReg;
              ins.DestinationValue = src.SourceValue;
              ins.DestinationRef = src.SourceRef;
                 if(src.SourceReg.HasValue)
                ins.Size = Registers.GetSize(src.SourceReg.Value);
              ins.DestinationDisplacement = src.SourceDisplacement;
              ins.DestinationIsIndirect = src.SourceIsIndirect;
              return true;
          }
          return false;
      }
      
      public static bool IsDestinationField(InstructionWithDestinationAndSize ins)
      {
          return ins.DestinationRef != null;
      }
      public static bool IsDestinationValue(InstructionWithDestinationAndSize ins)
      {
          return ins.DestinationValue.HasValue;
      }
      public static bool CanTransfer(InstructionWithDestinationAndSize ins, InstructionWithDestinationAndSize ins2)
      {
          bool ok = false;
          if (ins != null)
              ok = (IsDestinationRegister(ins) && !IsSegment(ins)) || IsDestinationValue(ins) || IsDestinationField(ins) ;
          else return false;


          if (ins2 != null)
              ok &= (IsDestinationRegister(ins2) && !IsSegment(ins2)) || IsDestinationValue(ins2) || IsDestinationField(ins2);

          return ok;
          
      }
     public static bool SameOperands(Pop a, Push b)
      {
          bool m = false;
          if (a.DestinationDisplacement == b.DestinationDisplacement)
              if (a.DestinationIsIndirect == b.DestinationIsIndirect)
                  if (a.DestinationRef == b.DestinationRef)
                      if (a.DestinationReg == b.DestinationReg)
                          if (a.DestinationValue == b.DestinationValue)
                              m = true;
          if (a.Size == b.Size)

              m &= true;
          return m;
      }
     public static bool SameOperands(Mov a)
     {
         bool m = false;
         if (a.DestinationDisplacement == a.SourceDisplacement)
             if (a.DestinationIsIndirect == a.SourceIsIndirect)
                 if (a.DestinationRef == a.SourceRef)
                     if (a.DestinationReg == a.SourceReg)
                         if (a.DestinationValue == a.SourceValue)
                             m = true;
     
         return m;
     }
     public static bool IsDestinationMemory(InstructionWithDestinationAndSize ins)
     {
         return ins.DestinationIsIndirect;
     }
     public static bool IsSourceMemory(InstructionWithDestinationAndSource ins)
     {
         return ins.SourceIsIndirect;
     }

    }
}
