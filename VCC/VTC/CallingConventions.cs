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
      int GetParameterSize(TypeSpec tp,bool reference)
      {
          if (reference)
              return 2;
          else if (tp.IsFloat && tp.IsPointer)
              return 2;
          else
          {
              if (tp.Size != 1 && tp.Size % 2 != 0)
                  return (tp.Size == 1) ? 2 : tp.Size + 1;
              else return (tp.Size == 1) ? 2 : tp.Size ;
          }
      }
      void HandleLeftToRight(ref List<ParameterSpec> L_R)
      {     
          int paramidx = 4; // Initial Stack Position
       for(int i = L_R.Count - 1; i >= 0;i--)
       {
              L_R[i].StackIdx = paramidx;
              L_R[i].InitialStackIndex = paramidx;

              paramidx += GetParameterSize(L_R[i].MemberType, L_R[i].IsReference);
             
          }

      }
      void HandleRightToLeft(ref List<ParameterSpec> L_R, CallingConventions ccv)
      {
          if (ccv != CallingConventions.FastCall && ccv != CallingConventions.VatuSysCall)
          {
              int paramidx = 4; // Initial Stack Position
              for (int i = 0; i < L_R.Count; i++)
              {
                  L_R[i].StackIdx = paramidx;
                  L_R[i].InitialStackIndex = paramidx;

                  paramidx += GetParameterSize(L_R[i].MemberType, L_R[i].IsReference);
              
              }
          }
          else if(L_R.Count > 2 && CallingConventions.FastCall == ccv)
          {
              int paramidx = 4; // Initial Stack Position
              for (int i = 2; i < L_R.Count; i++)
              {
                  L_R[i].StackIdx = paramidx;
                  L_R[i].InitialStackIndex = paramidx;
                  paramidx += GetParameterSize(L_R[i].MemberType, L_R[i].IsReference);
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
      public void EmitVSysCall(EmitContext ec, List<ParameterSpec> L_R)
      {
          int paramidx = 0;
          for (int i = 0; i < L_R.Count; i++)
          {
              RegisterSpec rs = new RegisterSpec(L_R[i].MemberType, RegistersEnum.DI, Location.Null, paramidx);
              rs.EmitToStack(ec);
              L_R[i].EmitFromStack(ec);
              paramidx += GetParameterSize(L_R[i].MemberType, L_R[i].IsReference);
          }
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
      public void ReserveVSysCall(ResolveContext rc, List<ParameterSpec> par)
      {

        for(int i = 0; i < par.Count;i++)
        {
            rc.LocalStackIndex -= GetParameterSize(par[i].MemberType, par[i].IsReference);
              par[i].StackIdx = rc.LocalStackIndex;
              par[i].InitialStackIndex = rc.LocalStackIndex;
 
        }

          

      }
  
      public void SetParametersIndex(ref  List<ParameterSpec> L_R, CallingConventions ccv)
      {
          if (ccv == CallingConventions.StdCall || ccv == CallingConventions.Cdecl || ccv == CallingConventions.FastCall || ccv == CallingConventions.VatuSysCall)
              HandleRightToLeft(ref L_R,ccv);
          else if (ccv == CallingConventions.Pascal || ccv == CallingConventions.Default)
              HandleLeftToRight(ref L_R);

      }
      public void EmitDecl(EmitContext ec,ref  List<ParameterSpec> L_R, CallingConventions ccv)
      {
          int size = 0;
          foreach (ParameterSpec p in L_R)
          {
              //if (p.MemberType.Size > 1 && !p.IsReference)
              //{
              //    size += p.MemberType.Size;
              //    if (p.MemberType.Size % 2 != 0)
              //        size++;
              //}
              //else size += 2;
              size += GetParameterSize(p.MemberType, p.IsReference);
          }
         
          if (ccv == CallingConventions.StdCall || ccv == CallingConventions.Pascal || ccv == CallingConventions.Default )
          {
              if (size > 0)
                  ec.EmitInstruction(new Return() { DestinationValue = (ushort)size  });
              else ec.EmitInstruction(new SimpleReturn());
          }
          else if (ccv == CallingConventions.Cdecl || ccv == CallingConventions.VatuSysCall)
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
                  //if (exp[i].Type.Size > 1)
                  //{

                  //    size += (exp[i].Type.Size == 1 ) ? 2 : exp[i].Type.Size;
                  //    if (exp[i].Type.Size % 2 != 0)
                  //        size++;
                  //}
                  //else size += 2;
                  size += GetParameterSize(exp[i].Type, false);

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
                  //if (exp[i].Type.Size > 1)
                  //{
                  //    size += (exp[i].Type.Size == 1) ? 2 : exp[i].Type.Size;
                  //    if (exp[i].Type.Size % 2 != 0)
                  //        size++;
                  //}
                  //else size += 2;
                  size += GetParameterSize(exp[i].Type, false);


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
          else if (method.CallingConvention == CallingConventions.VatuSysCall)
          {
              for (int i = exp.Count - 1; i >= 0; i--)
              {

                  exp[i].EmitToStack(ec);
                  size += GetParameterSize(exp[i].Type, false);

                  if (exp[i].Type.IsFloat && !exp[i].Type.IsPointer) // store floating point to stack
                  {
                      ec.EmitInstruction(new Sub() { DestinationReg = RegistersEnum.SP, SourceValue = (ushort)exp[i].Type.Size });
                      ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, SourceReg = EmitContext.SP }); 
                      ec.EmitStoreFloat(RegistersEnum.SI, exp[i].Type.FloatSizeBits, true);
                  }
              }
           // Set descriptor
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceValue = method.VSCDescriptor ,Size = 16});
              ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.DI, SourceReg = EmitContext.SP });

          }
          // call
          if (method.CallingConvention == CallingConventions.VatuSysCall)
              ec.EmitInstruction(new INT() { DestinationValue = method.VSCInterrupt});
          else
          ec.EmitCall(method);

          if (method.CallingConvention == CallingConventions.Cdecl || method.CallingConvention == CallingConventions.VatuSysCall && size > 0)
               ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SP, SourceValue = (ushort)size, Size = 80 });
           

      }
      public void EmitCall(EmitContext ec, List<Expr> exp, MemberSpec  method, CallingConventions ccv)
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
                  //if (exp[i].Type.Size > 1)
                  //{
                  //    size += (exp[i].Type.Size == 1) ? 2 : exp[i].Type.Size;
                  //    if (exp[i].Type.Size % 2 != 0)
                  //        size++;
                  //}
                  //else size += 2;
                  size += GetParameterSize(exp[i].Type, false);


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
                  //if (exp[i].Type.Size > 1)
                  //{
                  //    size += (exp[i].Type.Size == 1) ? 2 : exp[i].Type.Size;
                  //    if (exp[i].Type.Size % 2 != 0)
                  //        size++;
                  //}
                  //else size += 2;
                  size += GetParameterSize(exp[i].Type, false);

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
        
          // call
          method.EmitToStack(ec);
          ec.EmitPop(RegistersEnum.AX);
          ec.EmitInstruction(new Call() { DestinationReg =  RegistersEnum.AX });

          if (ccv == CallingConventions.Cdecl || ccv == CallingConventions.VatuSysCall && size > 0)
              ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SP, SourceValue = (ushort)size, Size = 80 });


      }
      public void EmitCall(EmitContext ec, List<Expr> exp, Expr ms, CallingConventions ccv)
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
                  //if (exp[i].Type.Size > 1)
                  //{
                  //    size += (exp[i].Type.Size == 1) ? 2 : exp[i].Type.Size;
                  //    if (exp[i].Type.Size % 2 != 0)
                  //        size++;
                  //}
                  //else size += 2;
                  size += GetParameterSize(exp[i].Type, false);


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
                  //if (exp[i].Type.Size > 1)
                  //{
                  //    size += (exp[i].Type.Size == 1) ? 2 : exp[i].Type.Size;
                  //    if (exp[i].Type.Size % 2 != 0)
                  //        size++;
                  //}
                  //else size += 2;
                  size += GetParameterSize(exp[i].Type, false);

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

          // call
          ms.EmitToStack(ec);
          ec.EmitPop(RegistersEnum.AX);
          ec.EmitInstruction(new Call() { DestinationReg = RegistersEnum.AX });

          if (ccv == CallingConventions.Cdecl || ccv == CallingConventions.VatuSysCall && size > 0)
              ec.EmitInstruction(new Add() { DestinationReg = EmitContext.SP, SourceValue = (ushort)size, Size = 80 });


      }
    }
}
