using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.Optimizer
{
    interface IOptimizer
    {
        int Level { get; set; }
        bool CheckForOptimization(List<Instruction> ins);
        bool Optimize(ref List<Instruction> src);
    }
}
