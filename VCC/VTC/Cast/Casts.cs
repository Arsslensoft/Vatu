using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Core;

namespace VTC
{
   public interface ICast
    {
       bool CanHandle(TypeSpec src, TypeSpec target);
       bool ResolveCast(TypeSpec src, TypeSpec dst);
       bool EmitCast(EmitContext ec, Expr expression);
       bool EmitCastFromStack(EmitContext ec, Expr expression);
       
      
    }
}
