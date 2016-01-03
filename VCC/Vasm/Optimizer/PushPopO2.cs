using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm.Optimizer
{
  
  
 public  class PushPopO2 : IOptimizer
  {
     Stack<int> PushIdxStack;
     Stack<string> CallLb;
     Dictionary<string, int> CallAndRet;
      public PushPopO2()
       {
           Level =2;
       }
       public int Level { get; set; }

       public bool LookForChange(List<Instruction> ins, int start, int stop, RegistersEnum rg, ElementReference re, int disp)
       {
       
           for (int i = start+1; i < stop; i++)
           {
              
               // used as dst
               if (ins[i] is IInstructionWithDestination)
               {
                   IInstructionWithDestination id = (IInstructionWithDestination)ins[i];
                   if (rg != null && id.DestinationReg == rg && id.DestinationDisplacement == disp)
                       return true;
                   else if (re != null && id.DestinationRef == re && id.DestinationDisplacement == disp)
                       return true;

               }
               else if (ins[i] is IInstructionWithSource)
               {
                   IInstructionWithSource id = (IInstructionWithSource)ins[i];
                   if (rg != null && id.SourceReg == rg)
                       return true;
                   else if (re != null && id.SourceRef == re)
                       return true;
               }

           }
           return false;
       }

       void LookForReturn(List<Instruction> mInstructions, int start)
       {
           for (int i = start; i < mInstructions.Count; i++)
           {
               Instruction ins = mInstructions[i];
               if (ins is Return)
               {
                   Return ad = ins as Return;

                   if (ad.DestinationValue.HasValue)
                       CallAndRet.Add(CallLb.Pop(), (int)ad.DestinationValue.Value);
                   else CallAndRet.Add(CallLb.Pop(), 0);

                   return;
               }
               else if (ins is SimpleReturn)
               {
                   CallAndRet.Add(CallLb.Pop(), 0);
                   return;
               }

           }
       }
     Push GetLastPush(List<Instruction> ins, int start, ref int idx)
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
     void BeforeCheck(int i,Instruction ins, List<Instruction> mInstructions)
     {
         if (ins is Push)
             PushIdxStack.Push(i);
         else if (ins is Add)
         {
             Add ad = ins as Add;
             if (ad.DestinationReg.HasValue && ad.SourceValue.HasValue && ad.DestinationReg.Value == RegistersEnum.SP)
             {
                 for (int j = 0; j < ad.SourceValue.Value / 2; j++)
                     PushIdxStack.Pop();
             }
         }
         else if (ins is Leave)
             PushIdxStack.Pop();
         else if (ins is Label && (ins as Label).Method)
         {
             CallLb.Push((ins as Label).Name);
             LookForReturn(mInstructions, i + 1);
         }
         else if (ins is Call && !(mInstructions[i + 1] is Add))
         {

             if (CallAndRet.ContainsKey((ins as Call).DestinationLabel))
             {
                 for (int j = 0; j < CallAndRet[(ins as Call).DestinationLabel] / 2; j++)
                     if (PushIdxStack.Count > 0)
                         PushIdxStack.Pop();
                     else Console.WriteLine("Stack error");


             }
             else Console.WriteLine("Method error");
         }
     }
     public bool CheckForOptimization(List<Instruction> mInstructions, List<Instruction> externals = null)
     {
         PushIdxStack = new Stack<int>();
         CallLb = new Stack<string>();
         CallAndRet = new Dictionary<string, int>();
           int pushidx;
           int popidx;
           for (int i = 0; i < mInstructions.Count; i++)
           {
               
                Instruction ins = mInstructions[i];

                BeforeCheck(i, ins, mInstructions);
                if (i > 0)
                {

                    // Push Pop optimize
                    if (ins is Pop)
                    {
                        Pop OldPop = (Pop)mInstructions[i];
                        popidx = i;

                        pushidx = PushIdxStack.Pop();
                        Push OldPush = mInstructions[pushidx] as Push;

                       bool ch = LookForChange(mInstructions, pushidx, popidx, OldPop.DestinationReg.Value, OldPop.DestinationRef, OldPop.DestinationDisplacement);
                        if (OldPush == null || ch)
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
                         mInstructions[pushidx] = mv;
                         mInstructions[popidx] = null;
                         
                        }
                    }
                }
           }

           while (PushIdxStack.Count > 0)
           {
               using (AssemblyWriter str = new AssemblyWriter(Console.OpenStandardOutput(),Encoding.ASCII))
               {
                   int i = 0;
                   mInstructions[i=PushIdxStack.Pop()].WriteText(null,str);
                   Console.WriteLine(i);
               }
           }
           return true;
       }
       public bool Optimize(ref List<Instruction> src)
       {

       
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
