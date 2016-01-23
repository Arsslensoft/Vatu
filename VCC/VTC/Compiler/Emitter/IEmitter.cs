using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
   public interface IEmitter
    {
   

       bool EmitToStack(EmitContext ec);
       bool EmitFromStack(EmitContext ec);
       bool ValueOf(EmitContext ec);
       bool ValueOfStack(EmitContext ec);
       bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem);
       bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem);
       bool LoadEffectiveAddress(EmitContext ec);
    }
}
