using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
   public class VatuExecutableLinker : Linker
    {
       public VatuExecutableLinker(Settings opt)
            : base(opt)
        {

        }

       protected override void ReserveHeader()
       {
           base.ReserveHeader();
       }
       public override void WriteHeader()
       {
           base.WriteHeader();
       }
    }
}
