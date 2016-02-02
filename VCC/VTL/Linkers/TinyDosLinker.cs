using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
    public class TinyDosLinker : Linker
    {
        public TinyDosLinker(Settings opt)
            : base(opt)
        {

        }

        protected override void LoadObjectsAndMap(string[] inobj)
        {
            //interrupt_enabled = false;
            Origin = 256;
            base.LoadObjectsAndMap(inobj);
        }

    }
}
