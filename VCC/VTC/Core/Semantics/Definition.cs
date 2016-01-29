﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class Definition : SimpleToken, IEmit, IResolve
    {

        public Definition()
        {
            loc = CompilerContext.TranslateLocation(position);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    }
}