using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
  public  class CallingConventionsHandler
    {
      void HandleLeftToRight(ref List<ParameterSpec> L_R)
      {     
          int paramidx = 4; // Initial Stack Position
       for(int i = L_R.Count - 1; i >= 0;i--)
       {
              L_R[i].StackIdx = paramidx;
              paramidx += (L_R[i].MemberType.Size == 1) ? 2 : L_R[i].MemberType.Size;
            if(L_R[i].MemberType.Size % 2 != 0)
                paramidx++;
          }

      }
      void HandleRightToLeft(ref List<ParameterSpec> L_R, CallingConventions ccv)
      {
          if (ccv != CallingConventions.FastCall)
          {
              int paramidx = 4; // Initial Stack Position
              for (int i = 0; i < L_R.Count; i++)
              {
                  L_R[i].StackIdx = paramidx;
                  paramidx += (L_R[i].MemberType.Size == 1) ? 2 : L_R[i].MemberType.Size;

                  if (L_R[i].MemberType.Size % 2 != 0)
                      paramidx++;
              }
          }
          else if(L_R.Count > 2)
          {
              int paramidx = 4; // Initial Stack Position
              for (int i = 2; i < L_R.Count; i++)
              {
                  L_R[i].StackIdx = paramidx;
                  paramidx += (L_R[i].MemberType.Size == 1) ? 2 : L_R[i].MemberType.Size;
                  if (L_R[i].MemberType.Size % 2 != 0)
                      paramidx++;
              }
          }
      }
      public void EmitFastCall(EmitContext ec,int par)
      {
          if (par >= 2)
          {
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -2, DestinationIsIndirect = true, SourceReg = EmitContext.C });
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -4, DestinationIsIndirect = true, SourceReg = EmitContext.D });
          }
          else if(par == 1)
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -2, DestinationIsIndirect = true, SourceReg = EmitContext.C });

      }
      public void ReserveFastCall(ResolveContext rc, List<ParameterSpec> par)
      {
          if (par.Count >= 2)
          {
              rc.LocalStackIndex -= 2;
              par[0].StackIdx = rc.LocalStackIndex;
              rc.LocalStackIndex -= 2;
              par[1].StackIdx = rc.LocalStackIndex;
             
          }
          else if (par.Count == 1)
          {
              rc.LocalStackIndex -= 2;
              par[0].StackIdx = rc.LocalStackIndex;
           
          }

      }
      public void SetParametersIndex(ref  List<ParameterSpec> L_R, CallingConventions ccv)
      {
          if (ccv == CallingConventions.StdCall || ccv == CallingConventions.Cdecl || ccv == CallingConventions.FastCall)
              HandleRightToLeft(ref L_R,ccv);
          else if (ccv == CallingConventions.Pascal || ccv == CallingConventions.Default)
              HandleLeftToRight(ref L_R);

      }
      public void EmitDecl(EmitContext ec,ref  List<ParameterSpec> L_R, CallingConventions ccv)
      {
          int size = 0;
          foreach (ParameterSpec p in L_R)
          {
              if (p.MemberType.Size > 1)
              {
                  size += p.MemberType.Size;
                  if (p.MemberType.Size % 2 != 0)
                      size++;
              }
              else size += 2;
          }
         
          if (ccv == CallingConventions.StdCall || ccv == CallingConventions.Pascal || ccv == CallingConventions.Default)
          {
              if (size > 0)
                  ec.EmitInstruction(new Return() { DestinationValue = (ushort)size  });
              else ec.EmitInstruction(new SimpleReturn());
          }
          else if(ccv == CallingConventions.Cdecl)
              ec.EmitInstruction(new SimpleReturn());
          else if (ccv == CallingConventions.FastCall)
          {
              if (size > 4)
                  ec.EmitInstruction(new Return() { DestinationValue = (ushort)(size - 4) });
              else ec.EmitInstruction(new SimpleReturn());
          }

        
        
            
      }
      public void EmitCall(EmitContext ec, List<Expr> exp, MethodSpec method)
      {
          int size = 0;
          if (method.CallingConvention == CallingConventions.Pascal || method.CallingConvention == CallingConventions.Default)
          {
              foreach (Expr e in exp)
                  e.EmitToStack(ec);
          }
          else if (method.CallingConvention == CallingConventions.StdCall || method.CallingConvention == CallingConventions.Cdecl)
          {

              for (int i = exp.Count - 1; i >= 0; i--)
              {
                  exp[i].EmitToStack(ec);
                  if (exp[i].Type.Size > 1)
                  {
                      size += (exp[i].Type.Size == 1) ? 2 : exp[i].Type.Size;
                      if (exp[i].Type.Size % 2 != 0)
                          size++;
                  }
                  else size += 2;
              }
             
          }
          else if (method.CallingConvention == CallingConventions.FastCall)
          {
              for (int i = exp.Count -1; i >= 0; i--)
              {
                  
                  exp[i].EmitToStack(ec);
                  if (exp[i].Type.Size > 1)
                  {
                      size += (exp[i].Type.Size == 1) ? 2 : exp[i].Type.Size;
                      if (exp[i].Type.Size % 2 != 0)
                          size++;
                  }
                  else size += 2;
              }
              if (exp.Count >= 2)
              {
                  ec.EmitPop(EmitContext.C);
                  ec.EmitPop(EmitContext.D);
              }
              else if(exp.Count == 1)
                  ec.EmitPop(EmitContext.C);

          }

          // call
          ec.EmitCall(method);

           if(method.CallingConvention == CallingConventions.Cdecl && size > 0)
               ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SP, SourceValue = (ushort)size, Size = 80 });
           

      }

    }
}
