using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public abstract class DeclarationToken : SimpleToken, IEmit
    {



        public DeclarationToken(Location lc)
        {
          //  Location = lc;
        }
        public DeclarationToken()
            : this(Location.Null)
        {
          
        }


        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }



    }
}
