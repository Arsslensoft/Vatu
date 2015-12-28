using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public struct Reachability
    {
        readonly bool unreachable;

        Reachability(bool unreachable)
        {
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

        bool GetBit(int index)
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

  

    public class FlowAnalysisContext
    {
        public bool UnreachableReported { get; set; }

        /*
                public bool IsDefinitelyAssigned(VariableInfo variable)
                {
                    return variable.IsAssigned(DefiniteAssignment);
                }

                public bool IsStructFieldDefinitelyAssigned(VariableInfo variable, string name)
                {
                    return variable.IsStructFieldAssigned(DefiniteAssignment, name);
                }

                public void SetVariableAssigned(VariableInfo variable, bool generatedAssignment = false)
                {
                    variable.SetAssigned(DefiniteAssignment, generatedAssignment);
                }

                public void SetStructFieldAssigned(VariableInfo variable, string name)
                {
                    variable.SetStructFieldAssigned(DefiniteAssignment, name);
                }
                */
    }
}
