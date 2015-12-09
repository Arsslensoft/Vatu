using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VJay;

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


    public interface IResolve
    {
        bool Resolve(ResolveContext rc);
    }

    /// <summary>
    /// Emit Context
    /// </summary>
   public class EmitContext
    {
    }

   public class ResolveContext : IDisposable
   {


       #region IDisposable Members

       public void Dispose()
       {

       }

       #endregion

   }

   public class CompilerContext
   {
       public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
       {
           return new Location(new SourceFile("this", "null", 0), li.Line, li.Column);
       }
   }
}
