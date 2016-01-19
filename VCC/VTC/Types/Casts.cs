using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Core;

namespace VTC.Types
{
   public interface ICast
    {
       TypeSpec Source { get; set; }
       TypeSpec Destination { get; set; }
       bool Explicit { get; set; }

       bool EmitCast(EmitContext ec, Expr expression);
       bool EmitCastFromStack(EmitContext ec, Expr expression);
       
      
    }
}
