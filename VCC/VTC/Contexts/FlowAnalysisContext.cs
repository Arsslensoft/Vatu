using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Core;

namespace VTC
{
    public struct Reachability
    {
        readonly bool unreachable;
        public Location Loc;
        Reachability(bool unreachable)
        {
            Loc = Location.Null;
            this.unreachable = unreachable;
        }

        public bool IsUnreachable
        {
            get
            {
                return unreachable;
            }
        }

        public static Reachability CreateUnreachable()
        {
            return new Reachability(true);
        }

        public static Reachability operator &(Reachability a, Reachability b)
        {
            return new Reachability(a.unreachable && b.unreachable);
        }

        public static Reachability operator |(Reachability a, Reachability b)
        {
            return new Reachability(a.unreachable | b.unreachable);
        }
    }
    public class DefiniteAssignmentBitSet
    {
        const uint copy_on_write_flag = 1u << 31;

        uint bits;

        // Used when bits overflows
        int[] large_bits;

        public static readonly DefiniteAssignmentBitSet Empty = new DefiniteAssignmentBitSet(0);

        public DefiniteAssignmentBitSet(int length)
        {
            if (length > 31)
                large_bits = new int[(length + 31) / 32];
        }

        public DefiniteAssignmentBitSet(DefiniteAssignmentBitSet source)
        {
            if (source.large_bits != null)
            {
                large_bits = source.large_bits;
                bits = source.bits | copy_on_write_flag;
            }
            else
            {
                bits = source.bits & ~copy_on_write_flag;
            }
        }

        public static DefiniteAssignmentBitSet operator &(DefiniteAssignmentBitSet a, DefiniteAssignmentBitSet b)
        {
            if (AreEqual(a, b))
                return a;

            DefiniteAssignmentBitSet res;
            if (a.large_bits == null)
            {
                res = new DefiniteAssignmentBitSet(a);
                res.bits &= (b.bits & ~copy_on_write_flag);
                return res;
            }

            res = new DefiniteAssignmentBitSet(a);
            res.Clone();
            var dest = res.large_bits;
            var src = b.large_bits;
            for (int i = 0; i < dest.Length; ++i)
            {
                dest[i] &= src[i];
            }

            return res;
        }

        public static DefiniteAssignmentBitSet operator |(DefiniteAssignmentBitSet a, DefiniteAssignmentBitSet b)
        {
            if (AreEqual(a, b))
                return a;

            DefiniteAssignmentBitSet res;
            if (a.large_bits == null)
            {
                res = new DefiniteAssignmentBitSet(a);
                res.bits |= b.bits;
                res.bits &= ~copy_on_write_flag;
                return res;
            }

            res = new DefiniteAssignmentBitSet(a);
            res.Clone();
            var dest = res.large_bits;
            var src = b.large_bits;

            for (int i = 0; i < dest.Length; ++i)
            {
                dest[i] |= src[i];
            }

            return res;
        }

        public static DefiniteAssignmentBitSet And(List<DefiniteAssignmentBitSet> das)
        {
            if (das.Count == 0)
                throw new ArgumentException("Empty das");

            DefiniteAssignmentBitSet res = das[0];
            for (int i = 1; i < das.Count; ++i)
            {
                res &= das[i];
            }

            return res;
        }

        bool CopyOnWrite
        {
            get
            {
                return (bits & copy_on_write_flag) != 0;
            }
        }

        int Length
        {
            get
            {
                return large_bits == null ? 31 : large_bits.Length * 32;
            }
        }

        public void Set(int index)
        {
            if (CopyOnWrite && !this[index])
                Clone();

            SetBit(index);
        }

        public void Set(int index, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                if (CopyOnWrite && !this[index + i])
                    Clone();

                SetBit(index + i);
            }
        }

        public bool this[int index]
        {
            get
            {
                return GetBit(index);
            }
        }

        public override string ToString()
        {
            var length = Length;
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; ++i)
            {
                sb.Append(this[i] ? '1' : '0');
            }

            return sb.ToString();
        }

        void Clone()
        {
            large_bits = (int[])large_bits.Clone();
        }

       public bool GetBit(int index)
        {
            return large_bits == null ?
                (bits & (1 << index)) != 0 :
                (large_bits[index >> 5] & (1 << (index & 31))) != 0;
        }

        void SetBit(int index)
        {
            if (large_bits == null)
                bits = (uint)((int)bits | (1 << index));
            else
                large_bits[index >> 5] |= (1 << (index & 31));
        }

        public static bool AreEqual(DefiniteAssignmentBitSet a, DefiniteAssignmentBitSet b)
        {
            if (a.large_bits == null)
                return (a.bits & ~copy_on_write_flag) == (b.bits & ~copy_on_write_flag);

            for (int i = 0; i < a.large_bits.Length; ++i)
            {
                if (a.large_bits[i] != b.large_bits[i])
                    return false;
            }

            return true;
        }
    }
    public class CodePath
    {
        public Location PathLocation { get; set; }
        public bool Returns { get; set; }
        public List<CodePath> CodePaths { get; set; }

        public CodePath(Location loc)
        {
            Returns = false;
            CodePaths = new List<CodePath>();
            PathLocation = loc;
        }

        public void AddPath(CodePath cp)
        {
            CodePaths.Add(cp);
        }
       
        public bool CheckReturn()
        {
        
            if (CodePaths.Count == 0 || Returns) // end of path or returns
                return Returns;

            bool ret = true;
            foreach (CodePath cp in CodePaths)
                ret &= cp.CheckReturn();
           
            return ret;
        }
    }
    public interface IFlow : IFlowAnalysis
    {
        Reachability MarkReachable(Reachability rc);

    }
    public interface IFlowAnalysis
    {
  
        bool DoFlowAnalysis(FlowAnalysisContext fc);
    }

    public class FlowAnalysisContext
    {
        public bool NoReturnCheck=false;
        public CodePath CodePathReturn { get; set; }
        public DefiniteAssignmentBitSet AssignmentBitSet { get; set; }
        public bool UnreachableReported { get; set; }
        public Declaration Decl { get; set; }
        public FlowAnalysisContext(int variable_count,Declaration decl)
        {
            CodePathReturn = new CodePath(decl.loc);
            Decl = decl;
            AssignmentBitSet = new DefiniteAssignmentBitSet(variable_count);
        }
        public bool FindUnreachableCode(ref Location loc)
        {
            Reachability rc = new Reachability();
            rc = Decl.MarkReachable(rc);
            loc = rc.Loc;
            return rc.IsUnreachable;
        }
        public bool FindNoReturn()
        {
         return   CodePathReturn.CheckReturn();
        }
        public bool DoFlowAnalysis(ResolveContext rc)
        {
     
            
          bool fl =  Decl.DoFlowAnalysis(this);
          if (!NoReturnCheck && !FindNoReturn())
              ResolveContext.Report.Warning(CodePathReturn.PathLocation, "Not all code paths returns a value ");
          Location loc = CodePathReturn.PathLocation;
          if (FindUnreachableCode(ref loc))
              ResolveContext.Report.Warning(loc, "Unreachable code detected");

          for (int i = 0; i < rc.Resolver.KnownLocalVars.Count; i++)
            if(!AssignmentBitSet.GetBit(i))
                ResolveContext.Report.Warning(CodePathReturn.PathLocation, "Use of unassigned local variable " + rc.Resolver.KnownLocalVars[i].Name);
          return fl;
        }
       
    }
}
