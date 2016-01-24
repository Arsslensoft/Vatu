using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ELFSharp.ELF.Sections
{
   public enum RelocationType : uint
    {
       None = 0,
       P32,
       PC32,
       GOT32,
       PLT32,
       COPY,
       GLOB_DATA,
       JMP_SLOT,
       RELATIVE,
       GOTOFF,
       GOTPC
    }
   public enum PRelocationType : uint
   {
       R_P32 = 1,
       R_PC32=2,
       R_P16 = 20,
       R_PC16 = 21
   }
}
