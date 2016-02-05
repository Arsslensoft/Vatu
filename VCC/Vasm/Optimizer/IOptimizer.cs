using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.Optimizer
{
    public interface IOptimizer : IComparer<IOptimizer>, IComparable<IOptimizer>
    {
        int Priority { get; set; }
        int Level { get; set; }
        bool Optimize(ref List<Instruction> src);
        bool Match(Instruction ins, int idx);
    }
}
