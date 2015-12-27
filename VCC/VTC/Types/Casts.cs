using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Types
{
   public interface ICast
    {
       TypeSpec SourceCast { get; set; }
       bool EmitCast(EmitContext ec, ICast dst);
       
      
    }
}
