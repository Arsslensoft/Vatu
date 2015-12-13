using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm
{
   public class AsmContext
    {
       private static AsmContext mCurrentInstance;
       Stack<RegistersEnum> rg = new Stack<RegistersEnum>();
       public RegistersEnum GetNextRegister()
        {
         // AX>BX>CX>DX>STACK
            if (rg.Contains(RegistersEnum.AX))
            {
                if (rg.Contains(RegistersEnum.CX))
                {
                    if (rg.Contains(RegistersEnum.DX))
                    {
                        if (rg.Contains(RegistersEnum.BX))
                            return RegistersEnum.SP;
                        else { rg.Push(RegistersEnum.BX); return RegistersEnum.BX; }
                    }
                    else { rg.Push(RegistersEnum.DX); return RegistersEnum.DX; }
                }
                else { rg.Push(RegistersEnum.CX); return RegistersEnum.CX; }
            }
            else { rg.Push(RegistersEnum.AX); return RegistersEnum.AX; }
        }
       public void FreeRegister()
       {
          // if (rg.Contains(reg))
               rg.Pop();
           
       }
       public RegistersEnum PeekRegister()
       {
           return (rg.Count > 0)?rg.Peek():GetNextRegister();
       }
       public AssemblyWriter AssemblerWriter { get; set; }
       public string EntryPoint { get; set; }
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

        bool SameOperands(InstructionWithDestinationAndSource a, InstructionWithDestinationAndSource b)
        {
            bool m = false;
            if (a.DestinationDisplacement == b.DestinationDisplacement)
                if (a.DestinationIsIndirect == b.DestinationIsIndirect)
                    if (a.DestinationRef == b.DestinationRef)
                        if (a.DestinationReg == b.DestinationReg)
                            if (a.DestinationValue == b.DestinationValue)
                                m = true;
            if (a.SourceDisplacement == b.SourceDisplacement)
                if (a.SourceIsIndirect == b.SourceIsIndirect)
                    if (a.SourceRef == b.SourceRef)
                        if (a.SourceReg == b.SourceReg)
                            if (a.SourceValue == b.SourceValue)
                                m &= true;
            return m;
        }
        bool SameOperands(InstructionWithDestinationAndSize a, InstructionWithDestinationAndSize b)
        {
            bool m = false;
            if (a.DestinationDisplacement == b.DestinationDisplacement)
                if (a.DestinationIsIndirect == b.DestinationIsIndirect)
                    if (a.DestinationRef == b.DestinationRef)
                        if (a.DestinationReg == b.DestinationReg)
                            if (a.DestinationValue == b.DestinationValue)
                                m = true;
            if (a.Size == b.Size)
             
                                m &= true;
            return m;
        }
     
       public void Optimize()
        {
   
            int i = 0;
            for (i = 0; i < mInstructions.Count; i++ )
            {
                Instruction ins = mInstructions[i];
                if (i > 0)
                {
                    // Push Pop optimize
                    if (ins is Pop && mInstructions[i - 1] is Push)
                    {
                        if (SameOperands((Pop)ins, (Push)mInstructions[i - 1]))
                        {
                             mInstructions[i].Emit = false;
                            mInstructions[i-1].Emit = false;
                        }
                    }

                }
                i++;
            }

              
        }
        public Dictionary<string, StructElement> DeclaredStructVars = new Dictionary<string, StructElement>();

        public StructElement GetStruct(string name)
        {
            foreach (StructElement se in mStructs)
                if (se.Name == name)
                    return se;

            return null;
        }



        protected List<DataMember> mDataMembers = new List<DataMember>();
        public List<DataMember> DataMembers
        {
            get { return mDataMembers; }
            set { mDataMembers = value; }
        }

        protected List<string> mextern = new List<string>();
        public List<string> Externals
        {
            get { return mextern; }
            set { mextern = value; }
        }

        protected List<StructElement> mStructs = new List<StructElement>();
        public List<StructElement> Structs
        {
            get { return mStructs; }
            set { mStructs = value; }
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

       public DataMember Declare(string name,byte[] data)
       {
           return new DataMember(name, data);
       }
       public bool DefineData(DataMember data)
       {
           if (!IsDataDefined(data.Name))
               DataMembers.Add(data);
           else return false;

           return true;
       }

       public Label DefineGlobal(string name)
       {
           return new Label(name,true);
       }
       public Label DefineLabel(string name)
       {
           return new Label(name);
       }
       public void MarkLabel(Label lb)
       {
           if(!Instructions.Contains(lb))
           Instructions.Add(lb);
       }

       public bool DefineStruct(StructElement data)
       {
         
               Structs.Add(data);
        

           return true;
       }

       public bool IsDataDefined(string name)
       {
           foreach (DataMember dm in DataMembers)
               if (dm.Name == name)
                   return true;

           return false;
       }
       public bool DefineStructInstance(string varname, string type)
       {
           StructElement se = GetStruct(type);
           if (se != null && !DeclaredStructVars.ContainsKey(varname))
           { DeclaredStructVars.Add(varname, se); return true; }
           else return false;
       }
       public void Emit(Instruction ins)
       {
           Instructions.Add(ins);
       }

       public void AddExtern(string func)
       {
           Externals.Add(func);
       }
       public virtual void EmitPrepare(AssemblyWriter writer)
       {
           // Emit

           writer.WriteLine("bits 16");
           // define
           foreach (StructElement xMember in mStructs)
           {
               xMember.Emit(writer);
              
               writer.WriteLine();
           }
       
           // alloc vars
           writer.WriteLine("section .bss");
           writer.WriteLine();
           foreach (KeyValuePair<string,StructElement> p in DeclaredStructVars)
           {
               p.Value.EmitAlloc(writer, p.Key);
               writer.WriteLine();
           }
           writer.WriteLine();
       }
       public virtual void EmitFinalize(AssemblyWriter writer)
       {

       }
       public virtual void Emit(AssemblyWriter writer)
       {
           // Optimize
           Optimize();

           // prepare emit
           EmitPrepare(writer);
           // Write out data declarations
           writer.WriteLine("section .data");
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
           // declare vars
           writer.WriteLine();
           foreach (KeyValuePair<string, StructElement> p in DeclaredStructVars)
           {
               p.Value.EmitDecl(writer, p.Key);
               writer.WriteLine();
           }
           writer.WriteLine();
           writer.WriteLine("section .text");
           // define externs
           foreach (string ex in Externals)
               writer.WriteLine("extern\t" + ex);
           writer.WriteLine();
           writer.Write("global	" + EntryPoint);
           // Write out code
           for (int i = 0; i < mInstructions.Count; i++)
           {

               var xOp = mInstructions[i];
               if (xOp.Emit)
               {
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
           }

           // Emit Finalize
           EmitFinalize(writer);
       }
    }
}
