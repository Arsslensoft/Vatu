using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm
{

    /// <summary>
    /// Basic Emit for CodeGen
    /// </summary>
   public interface IEmitExpr
    {

       /// <summary>
       /// Emit code
       /// </summary>
       /// <returns>Success or fail</returns>
       bool EmitFromStack(AsmContext ec);
       /// <summary>
       /// Emit code to stack [push]
       /// </summary>
       /// <returns>Success or fail</returns>
       bool EmitToStack(AsmContext ec);
    }


   /// <summary>
   /// Basic Emit for CodeGen
   /// </summary>
   public interface IEmit
   {
       /// <summary>
       /// Emit code
       /// </summary>
       /// <returns>Success or fail</returns>
       bool Emit(AsmContext ec);
      
   }
}
