using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.Optimizer
{
    /// <summary>
    /// Label Optimizer (Removes unused labels)
    /// </summary>
    public class CommentOptimizer : IOptimizer
    {
        public CommentOptimizer()
        {
            Level = 3;
        }
        public int Level { get; set; }

        public bool CheckForOptimization(List<Instruction> ins)
        {
         
            return true;
        }
        public bool Optimize(ref List<Instruction> src)
        {
            for (int i = 0; i < src.Count; i++)
                if (src[i] is Comment)
                    src[i].Emit = false;
            return true;
        }
    }
 
}
