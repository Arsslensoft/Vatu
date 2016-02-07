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
           try
           {
               IOptimizer[] opt = { new PushPopO1(), new MovO1(),new PushPopO0(), new PopPushO1(), new PPO() };

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
           catch
           {

           }
       }
       public static int OptimizationsSize = 0;
       public static int Optimizations = 0;
        static Dictionary<int , List<IOptimizer>> Optimizers { get; set; }
        public static List<IOptimizer>  GetAllAvailableOptimizersByLevel(int lvl)
        {
            List<IOptimizer> opt = new List<IOptimizer>();
            foreach (KeyValuePair<int, List<IOptimizer>> o in Optimizers)
            {
                if (o.Key != lvl)
                    continue;


                opt.AddRange(o.Value);
            }

            opt.Sort();
            return opt;
        }
        public static List<IOptimizer> GetAllAvailableOptimizersByPriority(int lvl,List<IOptimizer> OPT)
        {
            List<IOptimizer> opt = new List<IOptimizer>();
            foreach (IOptimizer o in OPT)
                if (o.Priority == lvl)
                    opt.Add(o);
       
            return opt;
        }
        public static bool OptimizeForLevel(int lvl, ref List<Instruction> ins, List<Instruction> ext)
        {
            bool opt = true;
            List<IOptimizer> AVOP = GetAllAvailableOptimizersByLevel(lvl);
            for (int MAX_PRIO = 0; MAX_PRIO < 9; MAX_PRIO++)
            {

                List<IOptimizer> p = GetAllAvailableOptimizersByPriority(MAX_PRIO, AVOP);
                if (p.Count == 0)
                    continue;
                OptimizeForPriority(ref ins, p, MAX_PRIO);

            }
            return opt;
        }
       public static bool Optimize(int lvl, ref List<Instruction> ins, List<Instruction> ext)
       {
           bool opt = true;
           for (int l = 1; l <= lvl; l++)
               opt &= OptimizeForLevel(l, ref ins, ext);
           return opt;
       }
       public static bool OptimizeForPriority(ref List<Instruction> ins, List<IOptimizer> AVOP,int priority)
       {
             int i = 0;
             bool ok = true;
             for (i = 0; i < ins.Count; i++)
             {
                 foreach (IOptimizer o in AVOP)
                 {
                     if (o.Match(ins[i], i))
                       ok &=  o.Optimize(ref ins);
                 }
             }
             return ok;
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
