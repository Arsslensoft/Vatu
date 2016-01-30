using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace Vasm
{
   public class AsmContext
    {
       public bool IsInterruptOverload { get; set; }
       public bool IsFlat { get; set; }
       public bool IsLibrary { get; set; }
       public bool IsVTExec { get; set; }
       public int OLevel { get; set; }
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
       public RegistersEnum SetAsUsed(RegistersEnum reg)
       {

           if (rg.Contains(reg))
               return GetNextRegister();
           else
           {
               rg.Push(reg);
               return reg;
           }
       }
       public RegistersEnum GetLow(RegistersEnum reg)
       {
           if (reg == RegistersEnum.AX)
               return RegistersEnum.AL;
           else if (reg == RegistersEnum.BX)
               return RegistersEnum.BL;
           else if (reg == RegistersEnum.CX)
               return RegistersEnum.CL;
           else if (reg == RegistersEnum.DX)
               return RegistersEnum.DL;
           else return reg;
       }
       public RegistersEnum GetHigh(RegistersEnum reg)
       {
           if (reg == RegistersEnum.AX)
               return RegistersEnum.AH;
           else if (reg == RegistersEnum.BX)
               return RegistersEnum.BH;
           else if (reg == RegistersEnum.CX)
               return RegistersEnum.CH;
           else if (reg == RegistersEnum.DX)
               return RegistersEnum.DH;
           else return reg;
       }
       public RegistersEnum GetHolder(RegistersEnum reg)
       {
           if (reg == RegistersEnum.AL | reg == RegistersEnum.AH)
               return RegistersEnum.AX;
           else if (reg == RegistersEnum.BL | reg == RegistersEnum.BH)
               return RegistersEnum.BX;
           else if (reg == RegistersEnum.CL | reg == RegistersEnum.CH)
               return RegistersEnum.CX;
           else if (reg == RegistersEnum.DL | reg == RegistersEnum.DH)
               return RegistersEnum.DX;
           else return reg;
       }

       public void FreeRegister()
       {
          // if (rg.Contains(reg))
               rg.Pop();
           
       }
       public RegistersEnum PeekRegister()
       {
           return (rg.Count > 0)?rg.Peek():RegistersEnum.AX;
       }
       public AssemblyWriter AssemblerWriter { get; set; }
       public string EntryPoint { get; set; }
       public AsmContext(string file)
       {
           IsFlat = false;
           AssemblerWriter = new AssemblyWriter(file);
           mCurrentInstance = this;
       }
       public AsmContext(AssemblyWriter writer)
       {
           IsFlat = false;
           AssemblerWriter = writer;
           mCurrentInstance = this;
          
       }
       public bool EmitAsmLabels { get; set; }
       protected int mAsmIlIdx;
        public int AsmIlIdx
        {
            get { return mAsmIlIdx; }
        }

      
        public bool Optimize()
        {
            //InlineInstruction entry = new InlineInstruction(EntryPoint);
            //if(!string.IsNullOrEmpty(EntryPoint))
            //    mDefInstructions.Add(entry);
            bool ok = Optimizer.Optimizer.Optimize(OLevel,ref mInstructions,mDefInstructions);
            //if (!string.IsNullOrEmpty(EntryPoint))
            //    mDefInstructions.Remove(entry);

            return ok;
   /*
            int i = 0;
            for (i = 0; i < mInstructions.Count; i++ )
            {
                Instruction ins = mInstructions[i];
                if (i > 0)
                {

                    // Push Pop optimize
                    if (ins is Pop && mInstructions[i - 1] is Push)
                    {
                         Push OldPush = (Push)mInstructions[i - 1];
                           Pop OldPop= (Pop)mInstructions[i ];
                        if (SameOperands(OldPop, OldPush)) // PUSH REG1 POP REG1
                        {
                             mInstructions[i].Emit = false;
                            mInstructions[i-1].Emit = false;
                        }
                        else if (SamePushPopRegisters(OldPop, OldPush)) // PUSH REG1 POP REG2
                        {
                           
                           mInstructions[i].Emit = false;
                           // mInstructions[i - 1].Emit = false;
                           mInstructions[i - 1] = new Mov() { SourceReg = OldPush.DestinationReg, DestinationReg = OldPop.DestinationReg, Size = OldPush.Size };
                        }
                        else if (PushValuePopRegister(OldPop, OldPush)) // Push VAL Pop REG
                        {
                            mInstructions[i].Emit = false;
                            // mInstructions[i - 1].Emit = false;
                            mInstructions[i - 1] = new Mov() { SourceValue = OldPush.DestinationValue, DestinationReg = OldPop.DestinationReg, Size = OldPush.Size };
                        }
                        else if (PushReferencePopRegister(OldPop, OldPush)) // Push REF Pop REG
                        {
                            mInstructions[i].Emit = false;
                            // mInstructions[i - 1].Emit = false;
                            mInstructions[i - 1] = new Mov() { SourceRef = OldPush.DestinationRef, DestinationReg = OldPop.DestinationReg, Size = OldPush.Size };
                        }
                    }

                }
              
            }
            */
              
        }
        public Dictionary<string, StructElement> DeclaredStructVars = new Dictionary<string, StructElement>();

        public StructElement GetStruct(string name)
        {
            foreach (StructElement se in mStructs)
                if (se.Name == name)
                    return se;

            return null;
        }

        protected List<InterruptDef> mInt = new List<InterruptDef>();
        public List<InterruptDef> Interrupts
        {
            get { return mInt; }
            set { mInt = value; }
        }

        protected List<DataMember> mCDataMembers = new List<DataMember>();
        public List<DataMember> ConstantDataMembers
        {
            get { return mCDataMembers; }
            set { mCDataMembers = value; }
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
        protected List<string> mglobals = new List<string>();
        public List<string> Globals
        {
            get { return mglobals; }
            set { mglobals = value; }
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
        protected internal List<Instruction> mDefInstructions = new List<Instruction>();
        public List<Instruction> DefaultInstructions
        {
            get { return mDefInstructions; }
            set { mDefInstructions = value; }
        }

        protected internal List<Instruction> mInitInstructions = new List<Instruction>();
        public List<Instruction> InitInstructions
        {
            get { return mInitInstructions; }
            set { mInitInstructions = value; }
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
       public void AddDefault(Instruction aReader)
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
           mDefInstructions.Add(aReader);
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
       public bool DefineConstantData(DataMember data)
       {
           if (!IsCDataDefined(data.Name))
               mCDataMembers.Add(data);
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
       public bool IsCDataDefined(string name)
       {
           foreach (DataMember dm in mCDataMembers)
               if (dm.Name == name)
                   return true;

           return false;
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
       void InitCode(AssemblyWriter writer)
       {
           // Write out code
           for (int i = 0; i < mInitInstructions.Count; i++)
           {

               var xOp = mInitInstructions[i];
               if (xOp == null)
                   continue;
               if (xOp.Emit)
               {
                   string prefix = "\t\t";
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
       }
       string interrupt_name;
       void EmitInterruptInstallCode(AssemblyWriter writer)
       {


           if (!string.IsNullOrEmpty(EntryPoint))
           {
               writer.WriteLine("global ___vatu_entry");
               writer.WriteLine("___vatu_entry:");

           }

           if (!IsLibrary)
               interrupt_name = "______INSTALL_INTERRUPTS";
           else if (IsLibrary)
               interrupt_name = "______INSTALL_INTERRUPTS_" + DateTime.Now.ToFileTimeUtc().ToString();
          
           if(IsLibrary && IsInterruptOverload && Interrupts.Count > 0)
                   writer.WriteLine("\t\tglobal	"+interrupt_name);
           else if (!IsLibrary && IsInterruptOverload && Interrupts.Count > 0)
               writer.WriteLine("\t\tcall "+interrupt_name);

           foreach (Instruction inst in mDefInstructions)
           {
               writer.Write("\t");
               inst.WriteText(this, writer);
               writer.WriteLine();
           }
           if(!string.IsNullOrEmpty(EntryPoint))
               writer.Write("\t\tcall " + EntryPoint);
               
         
               writer.WriteLine();
           
       }
       public virtual void EmitPrepare(AssemblyWriter writer)
       {
           // Emit
        
            writer.WriteLine("bits 16");
           if(!IsLibrary)
            writer.WriteLine("PROGRAM_ORG:");

          
             
            
           // INT Install         
           if (IsFlat)           
            EmitInterruptInstallCode(writer);
           //// define
           //foreach (StructElement xMember in mStructs)
           //{
           //    xMember.Emit(writer);           
           //    writer.WriteLine();
           //}
       
           // alloc vars
           if (!IsFlat)
                 writer.WriteLine(";section .bss");
           foreach (KeyValuePair<string,StructElement> p in DeclaredStructVars)
           {
               p.Value.EmitAlloc(writer, p.Key);
               writer.WriteLine();
           }
           writer.WriteLine();
       }
       public virtual void EmitFinalize(AssemblyWriter writer)
       {
           if (!IsLibrary)
           writer.WriteLine("PROGRAM_END:");
       }
       public virtual void Emit(AssemblyWriter writer)
       {
           // Optimize
           Optimize();

           // prepare emit
           EmitPrepare(writer);
           // Write out readonly
           if(!IsFlat)
                  writer.WriteLine(";section .rodata");

           foreach (DataMember xMember in mCDataMembers)
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
           // Write out data declarations
           if (!IsFlat)
           writer.WriteLine(";section .data");
 
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
           if (!IsFlat)
              writer.WriteLine(";section .text");

           if (!IsFlat)
           {
               // define externs
               foreach (string ex in Externals)
                   writer.WriteLine("extern\t" + ex);
               writer.WriteLine();
           }
           // define globals
           foreach (string ex in Globals)
               writer.WriteLine("global\t" + ex);
           writer.WriteLine();

           if(!IsFlat)
                 EmitInterruptInstallCode(writer);

           if (IsInterruptOverload && Interrupts.Count > 0)
           {
               InstallINTInstruction iit = new InstallINTInstruction(Interrupts, interrupt_name);
               writer.WriteLine();
               writer.Write("\t\t");
               iit.WriteText(this, writer);
           }
           // Write out code
           for (int i = 0; i < mInstructions.Count; i++)
           {

               var xOp = mInstructions[i];
               if (xOp == null)
                   continue;
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
