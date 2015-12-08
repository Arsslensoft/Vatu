using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm
{
   public class AsmContext
    {
       private static AsmContext mCurrentInstance;

       public AssemblyWriter AssemblerWriter { get; set; }

       public AsmContext(string file)
       {
           AssemblerWriter = new AssemblyWriter(file);
           mCurrentInstance = this;
       }
       public AsmContext(AssemblyWriter writer)
       {
           AssemblerWriter = writer;
           mCurrentInstance = this;
       }
       public bool EmitAsmLabels { get; set; }
       protected int mAsmIlIdx;
        public int AsmIlIdx
        {
            get { return mAsmIlIdx; }
        }

        protected List<DataMember> mDataMembers = new List<DataMember>();
        public List<DataMember> DataMembers
        {
            get { return mDataMembers; }
            set { mDataMembers = value; }
        }

        protected internal List<Instruction> mInstructions = new List<Instruction>();
        public List<Instruction> Instructions
        {
            get { return mInstructions; }
            set { mInstructions = value; }
        }

        public static AsmContext CurrentInstance
        {
            get { return mCurrentInstance; }
        }

        internal int AllAssemblerElementCount
        {
            get { return mInstructions.Count + mDataMembers.Count; }
        }

        public BaseAssemblerElement GetAssemblerElement(int aIndex)
        {
            if (aIndex >= mInstructions.Count)
            {
                return mDataMembers[aIndex - mInstructions.Count];
            }
            return mInstructions[aIndex];
        }

       public BaseAssemblerElement TryResolveReference(Vasm.ElementReference aReference)
        {
            foreach (var xInstruction in mInstructions)
            {
                var xLabel = xInstruction as Label;
                if (xLabel != null)
                {
                    if (xLabel.QualifiedName.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return xLabel;
                    }
                }
            }
            foreach (var xDataMember in mDataMembers)
            {
                if (xDataMember.Name.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return xDataMember;
                }
            }
            return null;
        }


       public void Add(Instruction aReader)
       {
           if (EmitAsmLabels)
           {
               if (!(aReader is Label)
                   && !(aReader is Comment))
               {
                   // Only issue label if its executable code.
                   new Label("." + AsmIlIdx.ToString("X2"), "Asm");
                   mAsmIlIdx++;
               }
           }
           mInstructions.Add(aReader);
       }

       public void Add(params Instruction[] aReaders)
       {
           mInstructions.Capacity += aReaders.Length;
           foreach (Instruction xInstruction in aReaders)
           {
               mInstructions.Add(xInstruction);
           }
       }

       public virtual void EmitPrepare(AssemblyWriter writer)
       {

       }
       public virtual void EmitFinalize(AssemblyWriter writer)
       {

       }
       public virtual void Emit(AssemblyWriter writer)
       {
           // prepare emit
           EmitPrepare(writer);
           // Write out data declarations
           writer.WriteLine();
           foreach (DataMember xMember in mDataMembers)
           {
               writer.Write("\t");
               if (xMember.IsComment)
               {
                   writer.Write(xMember.Name);
               }
               else
               {
                   xMember.WriteText(this, writer);
               }
               writer.WriteLine();
           }
           writer.WriteLine();

           // Write out code
           for (int i = 0; i < mInstructions.Count; i++)
           {
               var xOp = mInstructions[i];
               string prefix = "\t\t\t";
               if (xOp is Label)
               {
                   var xLabel = (Label)xOp;
                   writer.WriteLine();
                   prefix = "\t\t";
                   writer.Write(prefix);
                   xLabel.WriteText(this, writer);
                   writer.WriteLine();
               }
               else
               {
                   writer.Write(prefix);
                   xOp.WriteText(this, writer);
                   writer.WriteLine();
               }
           }

           // Emit Finalize
           EmitFinalize(writer);
       }
    }
}
