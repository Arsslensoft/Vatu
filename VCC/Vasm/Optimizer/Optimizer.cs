using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.Optimizer
{
   public class Optimizer
    {

       static Optimizer()
       {
           Optimizers = new Dictionary<int, List<IOptimizer>>();
           // in order 1 -> 5
           IOptimizer[] opt = {  new PushPopO1(), new PushPopO2(), new CommentOptimizer() , new JumpOptimizer(),new LabelOptimizer() };

           foreach (IOptimizer o in opt)
           {
               if (Optimizers.ContainsKey(o.Level))
                   Optimizers[o.Level].Add(o);

               else
               {
                   Optimizers.Add(o.Level, new List<IOptimizer>());
                   Optimizers[o.Level].Add(o);
               }
           }

          
       }

       public static int Optimizations = 0;
        static Dictionary<int , List<IOptimizer>> Optimizers { get; set; }
       public static bool Optimize(int lvl, ref List<Instruction> ins, List<Instruction> ext)
       {
           bool opt = true;
           foreach (KeyValuePair<int, List<IOptimizer>> o in Optimizers)
           {
               if (o.Key > lvl)
                   continue;

               foreach (IOptimizer vopt in o.Value)
               {
                   opt &= vopt.CheckForOptimization(ins,ext);
                   if(opt)
                       opt &= vopt.Optimize(ref ins);
               }
           }
           return opt;
       }

    }

   public class OptimizedInstruction : IEquatable<OptimizedInstruction>
   {
       public bool Equals(OptimizedInstruction t)
       {
           if (t == null)
               return false;
           return t.Index == Index;
       }
       public int Index { get; set; }
       public Instruction OldInstruction { get; set; }
       public Instruction Replacement { get; set; }
   }
}
