using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
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
        bool EmitFromStack(EmitContext ec);
        /// <summary>
        /// Emit code to stack [push]
        /// </summary>
        /// <returns>Success or fail</returns>
        bool EmitToStack(EmitContext ec);
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
        bool Emit(EmitContext ec);

    }

    /// <summary>
    /// Emit Context
    /// </summary>
   public class EmitContext
    {
    }
}
