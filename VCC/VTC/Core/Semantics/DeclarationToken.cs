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
            loc = lc;
        }
        public DeclarationToken()
            : this(Location.Null)
        {
            loc = CompilerContext.TranslateLocation(position);
        }


        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }



    }
}
