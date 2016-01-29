using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
  public  class OptimizeUtils
    {
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
              ok = IsDestinationRegister(ins) || IsDestinationValue(ins) || IsDestinationField(ins);
          else return false;


          if (ins2 != null)
              ok &= IsDestinationRegister(ins2) || IsDestinationValue(ins2) || IsDestinationField(ins2);

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
