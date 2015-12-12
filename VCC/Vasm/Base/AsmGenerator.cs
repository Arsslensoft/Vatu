using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.Base
{
   public class AsmGenerator
    {
       public AssemblyWriter Writer { get; set; }
       public AsmContext Context { get; set; }
     public  void GenStartCommentLine(string comment)
     {
         Writer.Write("; " + comment);
     }
   public  void GenWordAlignment(bool bss)
     {
        Writer.WriteLine(bss ? "\talignb 2" : "\talign 2");
     }
   public void GenZeroData(uint Size, bool bss)
{
         Writer.WriteLine(bss ? "\tresb\t%u" : "\ttimes\t%u db 0");
}

    }
}
