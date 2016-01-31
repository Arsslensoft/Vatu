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
              L_R[i].InitialStackIndex = paramidx;
              if (L_R[i].IsReference)
                  paramidx += 2;
              else
              {
                  paramidx += (L_R[i].MemberType.Size == 1) ? 2 : L_R[i].MemberType.Size;

                  if (L_R[i].MemberType.Size != 1 && L_R[i].MemberType.Size % 2 != 0)
                      paramidx++;
              }
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
                  L_R[i].InitialStackIndex = paramidx;
                  if (L_R[i].IsReference)
                      paramidx += 2;
                  else
                  {
                      paramidx += (L_R[i].MemberType.Size == 1) ? 2 : L_R[i].MemberType.Size;

                      if (L_R[i].MemberType.Size != 1 && L_R[i].MemberType.Size % 2 != 0)
                          paramidx++;
                  }
              }
          }
          else if(L_R.Count > 2 && CallingConventions.FastCall == ccv)
          {
              int paramidx = 4; // Initial Stack Position
              for (int i = 2; i < L_R.Count; i++)
              {
                  L_R[i].StackIdx = paramidx;
                  L_R[i].InitialStackIndex = paramidx;
                  if (L_R[i].IsReference)
                      paramidx += 2;
                  else
                  {
                      paramidx += (L_R[i].MemberType.Size == 1) ? 2 : L_R[i].MemberType.Size;
                      if (L_R[i].MemberType.Size != 1 && L_R[i].MemberType.Size % 2 != 0)
                          paramidx++;
                  }
              }
          }
          else if (L_R.Count > 4 && CallingConventions.VeryFastCall == ccv)
          {
              int paramidx = 4; // Initial Stack Position
              for (int i = 2; i < L_R.Count; i++)
              {
                  L_R[i].StackIdx = paramidx;
                  L_R[i].InitialStackIndex = paramidx;
                  if (L_R[i].IsReference)
                      paramidx += 2;
                  else
                  {
                      paramidx += (L_R[i].MemberType.Size == 1) ? 2 : L_R[i].MemberType.Size;
                      if (L_R[i].MemberType.Size != 1 && L_R[i].MemberType.Size % 2 != 0)
                          paramidx++;
                  }
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
      public void EmitVFastCall(EmitContext ec, int par)
      {
         
          if (par == 4)
          {
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -2, DestinationIsIndirect = true, SourceReg = EmitContext.A });
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -4, DestinationIsIndirect = true, SourceReg = EmitContext.B });
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -6, DestinationIsIndirect = true, SourceReg = EmitContext.C });
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -8, DestinationIsIndirect = true, SourceReg = EmitContext.D});
          }
          else if (par == 3)
          {
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -2, DestinationIsIndirect = true, SourceReg = EmitContext.A });
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -4, DestinationIsIndirect = true, SourceReg = EmitContext.B });
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -6, DestinationIsIndirect = true, SourceReg = EmitContext.C });
           
          }
          else if (par == 2)
          {
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -2, DestinationIsIndirect = true, SourceReg = EmitContext.A });
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -4, DestinationIsIndirect = true, SourceReg = EmitContext.B });

          }
          else if (par == 1)
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationDisplacement = -2, DestinationIsIndirect = true, SourceReg = EmitContext.A });

          
      }
      public void ReserveFastCall(ResolveContext rc, List<ParameterSpec> par)
      {
          if (par.Count >= 2)
          {
              rc.LocalStackIndex -= 2;
              par[0].StackIdx = rc.LocalStackIndex;
              par[0].InitialStackIndex = rc.LocalStackIndex;
              rc.LocalStackIndex -= 2;
              par[1].StackIdx = rc.LocalStackIndex;
              par[1].InitialStackIndex = rc.LocalStackIndex;
          }
          else if (par.Count == 1)
          {
              rc.LocalStackIndex -= 2;
              par[0].StackIdx = rc.LocalStackIndex;
              par[0].InitialStackIndex = rc.LocalStackIndex;
          }

      }

      public void ReserveVFastCall(ResolveContext rc, List<ParameterSpec> par)
      {
          for (int i = 0; i < par.Count; i++)
          {
              rc.LocalStackIndex -= 2;
              par[i].StackIdx = rc.LocalStackIndex;
              par[i].InitialStackIndex = rc.LocalStackIndex;
          }
       

      }

      public void SetParametersIndex(ref  List<ParameterSpec> L_R, CallingConventions ccv)
      {
          if (ccv == CallingConventions.StdCall || ccv == CallingConventions.Cdecl || ccv == CallingConventions.FastCall || ccv == CallingConventions.VeryFastCall)
              HandleRightToLeft(ref L_R,ccv);
          else if (ccv == CallingConventions.Pascal || ccv == CallingConventions.Default)
              HandleLeftToRight(ref L_R);

      }
      public void EmitDecl(EmitContext ec,ref  List<ParameterSpec> L_R, CallingConventions ccv)
      {
          int size = 0;
          foreach (ParameterSpec p in L_R)
          {
              if (p.MemberType.Size > 1 && !p.IsReference)
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
          else if (ccv == CallingConventions.VeryFastCall)
          {
              if (size > 8)
                  ec.EmitInstruction(new Return() { DestinationValue = (ushort)(size - 8) });
              else ec.EmitInstruction(new SimpleReturn());
          }
        
        
            
      }
      public void EmitCall(EmitContext ec, List<Expr> exp, MethodSpec method)
      {
         
          int size = 0;
          if (method.CallingConvention == CallingConventions.Pascal || method.CallingConvention == CallingConventions.Default)
          {
              foreach (Expr e in exp)
              {
                  e.EmitToStack(ec);
                  if (e.Type.IsFloat && !e.Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)e.Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP });
                      ec.EmitStoreFloat(RegistersEnum.SI, e.Type.FloatSizeBits, true);
                  }
              }
          }
          else if (method.CallingConvention == CallingConventions.StdCall || method.CallingConvention == CallingConventions.Cdecl)
          {

              for (int i = exp.Count - 1; i >= 0; i--)
              {
                  exp[i].EmitToStack(ec);
                  if (exp[i].Type.Size > 1)
                  {

                      size += (exp[i].Type.Size == 1 ) ? 2 : exp[i].Type.Size;
                      if (exp[i].Type.Size % 2 != 0)
                          size++;
                  }
                  else size += 2;

                  if (exp[i].Type.IsFloat && !exp[i].Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)exp[i].Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, exp[i].Type.FloatSizeBits, true);
                  }
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

                  if (exp[i].Type.IsFloat && !exp[i].Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)exp[i].Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, exp[i].Type.FloatSizeBits, true);
                  }
              }
              if (exp.Count >= 2)
              {
                  ec.EmitPop(EmitContext.C);
                  ec.EmitPop(EmitContext.D);
              }
              else if(exp.Count == 1)
                  ec.EmitPop(EmitContext.C);

          }
          else if (method.CallingConvention == CallingConventions.VeryFastCall)
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


                  if (exp[i].Type.IsFloat && !exp[i].Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)exp[i].Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, exp[i].Type.FloatSizeBits, true);
                  }
              }
              if (exp.Count >= 4)
              {
                  ec.EmitPop(EmitContext.A);
                  ec.EmitPop(EmitContext.B);
                  ec.EmitPop(EmitContext.C);
                  ec.EmitPop(EmitContext.D);
              }
              else if (exp.Count == 3)
              {
                  ec.EmitPop(EmitContext.A);
                  ec.EmitPop(EmitContext.B);
                  ec.EmitPop(EmitContext.C);

              }
              else if (exp.Count == 1)
              {
                  ec.EmitPop(EmitContext.A);
                  ec.EmitPop(EmitContext.B);

              }
              else if (exp.Count == 1)
                  ec.EmitPop(EmitContext.A);

          }
          // call
          ec.EmitCall(method);

           if(method.CallingConvention == CallingConventions.Cdecl && size > 0)
               ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SP, SourceValue = (ushort)size, Size = 80 });
           

      }
      public void EmitCall(EmitContext ec, List<Expr> exp, MemberSpec  method, RegistersEnum rg, CallingConventions ccv)
      {
          int size = 0;
          if (ccv == CallingConventions.Pascal || ccv == CallingConventions.Default)
          {
              foreach (Expr e in exp)
              {
                  e.EmitToStack(ec);
                  if (e.Type.IsFloat && !e.Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)e.Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, e.Type.FloatSizeBits, true);
                  }
              }
          }
          else if (ccv == CallingConventions.StdCall || ccv == CallingConventions.Cdecl)
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

                  if (exp[i].Type.IsFloat && !exp[i].Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)exp[i].Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, exp[i].Type.FloatSizeBits, true);
                  }
              }

          }
          else if (ccv == CallingConventions.FastCall)
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

                  if (exp[i].Type.IsFloat && !exp[i].Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)exp[i].Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, exp[i].Type.FloatSizeBits, true);
                  }
              }
              if (exp.Count >= 2)
              {
                  ec.EmitPop(EmitContext.C);
                  ec.EmitPop(EmitContext.D);
              }
              else if (exp.Count == 1)
                  ec.EmitPop(EmitContext.C);

          }
          else if (ccv == CallingConventions.VeryFastCall)
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

                  if (exp[i].Type.IsFloat && !exp[i].Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)exp[i].Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, exp[i].Type.FloatSizeBits, true);
                  }
              }
              if (exp.Count >= 4)
              {
                  ec.EmitPop(EmitContext.A);
                  ec.EmitPop(EmitContext.B);
                  ec.EmitPop(EmitContext.C);
                  ec.EmitPop(EmitContext.D);
              }
              else if (exp.Count == 3)
              {
                  ec.EmitPop(EmitContext.A);
                  ec.EmitPop(EmitContext.B);
                  ec.EmitPop(EmitContext.C);

              }
              else if (exp.Count == 1)
              {
                  ec.EmitPop(EmitContext.A);
                  ec.EmitPop(EmitContext.B);

              }
              else if (exp.Count == 1)
                  ec.EmitPop(EmitContext.A);

          }
          // call
          ec.EmitInstruction(new Call() { DestinationReg = rg });

          if (ccv == CallingConventions.Cdecl && size > 0)
              ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SP, SourceValue = (ushort)size, Size = 80 });


      }
    }
}
