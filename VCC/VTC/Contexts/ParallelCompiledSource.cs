using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VTC
{
   public class ParallelCompiledSource
    {
       public bool Result { get; set; }
       public CompiledSource Source { get; private set; }
       public Stopwatch Time { get; private set; }
       public ParallelCompiledSource(CompiledSource s)
       {
           Time = new Stopwatch();
           Source = s;
       }
    }
}
