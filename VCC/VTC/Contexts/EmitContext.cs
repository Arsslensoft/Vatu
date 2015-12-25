﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
    /// <summary>
    /// Special Emit for expressions
    /// </summary>
    public interface IEmitExpr
    {
        bool EmitToStack(EmitContext ec);
        bool EmitFromStack(EmitContext ec);
        bool EmitToRegister(EmitContext ec, RegistersEnum rg);
        bool EmitBranchable(EmitContext ec, Label truecase, bool val);
    }


    /// <summary>
    /// Basic Emit for CodeGen
    /// </summary>
    public interface IEmit
    {
        /// <summary>
        /// Emit code
        /// </summary>
        /// <returns>Success or fail</returns>
        bool Emit(EmitContext ec);



    }




    /// <summary>
    /// Emit Context
    /// </summary>
    public class EmitContext
    {


        public const byte TRUE = 255;
        public const RegistersEnum A = RegistersEnum.AX;
        public const RegistersEnum B = RegistersEnum.BX;
        public const RegistersEnum C = RegistersEnum.CX;
        public const RegistersEnum D = RegistersEnum.DX;
        public const RegistersEnum SP = RegistersEnum.SP;
        public const RegistersEnum BP = RegistersEnum.BP;
        public const RegistersEnum DI = RegistersEnum.DI;
        public const RegistersEnum SI = RegistersEnum.SI;
        //#elif _32BITS
        //        public const RegistersEnum A = RegistersEnum.EAX;
        //        public const RegistersEnum B = RegistersEnum.EBX;
        //        public const RegistersEnum C = RegistersEnum.ECX;
        //        public const RegistersEnum D = RegistersEnum.EDX;
        //        public const RegistersEnum SP = RegistersEnum.ESP;
        //        public const RegistersEnum BP = RegistersEnum.EBP;
        //        public const RegistersEnum DI = RegistersEnum.EDI;
        //        public const RegistersEnum SI = RegistersEnum.ESI;



        Vasm.AsmContext ag;
        List<string> _names;
        Label enterLoop, exitLoop;
        Dictionary<string, VarSpec> Variables = new Dictionary<string, VarSpec>();
        public RegistersEnum GetLow(RegistersEnum reg)
        {
            return ag.GetLow(reg);
        }
        public RegistersEnum GetHigh(RegistersEnum reg)
        {
            return ag.GetHigh(reg);
        }
        /* public RegistersEnum GetNextRegister()
          {
              RegistersEnum acc = ag.GetNextRegister();
              if (acc == EmitContext.SP)
                  throw new ArgumentException("All Registers used");
              return acc;
          }
         public RegistersEnum SetAsUsed(RegistersEnum reg)
         {
      
       
             return ag.SetAsUsed(reg);
         }
         public void FreeRegister()
         {
             ag.FreeRegister();
         }
         public RegistersEnum FirstRegister()
         {
         return    ag.PeekRegister();
         }*/
        public ResolveContext CurrentResolve { get; set; }



        public static Dictionary<LabelType, int> Labels = new Dictionary<LabelType, int>();
        EmitContext()
        {

            _names = new List<string>();
        }
        public void SetCurrentResolve(ResolveContext rc)
        {
            CurrentResolve = rc;
        }
        public static string GenerateLabelName(LabelType lb)
        {
            if (Labels.ContainsKey(lb))
            {
                Labels[lb]++;
                return lb.ToString() + "_" + Labels[lb].ToString();
            }
            else
            {
                Labels.Add(lb, 0);
                return lb.ToString() + "_" + Labels[lb].ToString();
            }
        }

        public void SetEntry(string name)
        {
            ag.EntryPoint = name;
        }
        public EmitContext(Vasm.AssemblyWriter asmw)
        {
            ag = new Vasm.AsmContext(asmw);
        }

        public EmitContext(Vasm.AsmContext ac)
        {
            ag = ac;
        }

        public void EmitInstruction(Vasm.Instruction ins)
        {
            ag.Emit(ins);
        }
        public void EmitPop(RegistersEnum rg, byte size = 80, bool adr = false)
        {
            EmitInstruction(new Pop() { DestinationReg = rg, Size = size, DestinationIsIndirect = adr });
        }
        public void EmitPush(bool v)
        {
            EmitInstruction(new Push() { DestinationValue = (v ? (uint)EmitContext.TRUE : 0), Size = 8 });
        }
        public void EmitPush(byte v)
        {
            EmitInstruction(new Push() { DestinationValue = v, Size = 8 });
        }
        public void EmitPush(ushort v)
        {
            EmitInstruction(new Push() { DestinationValue = v, Size = 16 });
        }
        public void EmitPush(RegistersEnum rg, byte size = 80, bool adr = false)
        {
            EmitInstruction(new Push() { DestinationReg = rg, Size = size, DestinationIsIndirect = adr });
        }







        public void EmitBoolean(RegistersEnum rg, ConditionalTestEnum tr, ConditionalTestEnum fls)
        {
            EmitInstruction(new ConditionalMove() { Condition = tr, DestinationReg = rg, Size = 80, SourceValue = TRUE });
            EmitInstruction(new ConditionalMove() { Condition = fls, DestinationReg = rg, Size = 80, SourceValue = 0 });
        }
        public void EmitBooleanWithJump(RegistersEnum rg, ConditionalTestEnum TR)
        {
            string lbname = EmitContext.GenerateLabelName(LabelType.BOOL_EXPR);
            Label truelb = DefineLabel(lbname + "_TRUE");
            Label falselb = DefineLabel(lbname + "_FALSE");
            Label boolexprlb = DefineLabel(lbname + "_END");
            // jumps
            EmitInstruction(new ConditionalJump() { Condition = TR, DestinationLabel = truelb.Name });
            EmitInstruction(new Jump() { DestinationLabel = falselb.Name }); // false
            // emit true and false
            // true
            MarkLabel(truelb);
            EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceValue = TRUE, Size = 8 });
            EmitInstruction(new Jump() { DestinationLabel = boolexprlb.Name }); // exit
            // false
            MarkLabel(falselb);
            EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceValue = 0, Size = 8 });
            // mark exit
            MarkLabel(boolexprlb);
        }
        public void EmitBoolean(RegistersEnum rg, bool v)
        {
            EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (v ? (uint)EmitContext.TRUE : 0), Size = 80 });
        }
        public void EmitCall(MethodSpec m)
        {
            EmitInstruction(new Call() { DestinationLabel = m.Signature.ToString() });
        }
        public void EmitComment(string str)
        {
            ag.Emit(new Comment(ag, str));
        }
        public void EmitStructDef(StructTypeSpec m)
        {
            StructElement st = new StructElement();
            st.Name = m.Signature.ToString();
            st.Vars = new List<StructVar>();
            foreach (TypeMemberSpec mem in m.Members)
            {
                StructVar sv = new StructVar();
                sv.Name = mem.Name;
                sv.IsByte = mem.MemberType.Size == 1;
                sv.IsStruct = mem.MemberType.IsStruct;
                sv.Size = mem.MemberType.Size;
                sv.Type = sv.IsStruct ? mem.MemberType.Signature.ToString() : "";
                st.Vars.Add(sv);
            }
            EmitStruct(st);

        }



        public void EmitData(DataMember dm, MemberSpec v, bool constant = false)
        {
            if (!Variables.ContainsKey(v.Signature.ToString()))
            {
                v.Reference = ElementReference.New(v.Signature.ToString());
                if (constant)
                    ag.DefineConstantData(dm);
                else ag.DefineData(dm);

            }
        }



        public void MarkLabel(Label lb)
        {
            ag.MarkLabel(lb);
        }
        public Label DefineLabel(string name)
        {
            return ag.DefineLabel(name);
        }
        public Label DefineLabel()
        {
            return ag.DefineLabel(GenerateLabelName(LabelType.LABEL));
        }
        public Label DefineLabel(LabelType lbt, string suffix = null)
        {
            if (suffix == null)
                return ag.DefineLabel(GenerateLabelName(lbt));
            else return ag.DefineLabel(GenerateLabelName(lbt) + "_" + suffix);
        }


        public void Emit()
        {
            ag.Emit(ag.AssemblerWriter);
        }

        public void EmitDataWithConv(string name, object value, MemberSpec v, bool constant = false)
        {
            DataMember dm;
            if (value is string)
                dm = new DataMember(name, Encoding.ASCII.GetBytes(value.ToString()));
            else if (value is byte[])
                dm = new DataMember(name, (byte[])value);
            else if (value is bool)
                dm = new DataMember(name, ((bool)value) ? (new byte[1] { EmitContext.TRUE }) : (new byte[1] { 0 }));
            else if (value is byte)
                dm = new DataMember(name, new byte[1] { (byte)value });
            else if (value is ushort)
                dm = new DataMember(name, new ushort[1] { (ushort)value });
            else dm = new DataMember(name, new object[1] { value });

            EmitData(dm, v, constant);
        }
        public void EmitStruct(StructElement strct)
        {
            ag.DefineStruct(strct);
        }
        public bool AddInstanceOfStruct(string varname, TypeSpec st)
        {
            if (st.IsStruct && !ag.DeclaredStructVars.ContainsKey(varname))
                return ag.DefineStructInstance(varname, st.Signature.ToString());
            else return false;
        }
        public void DefineExtern(MethodSpec method)
        {
            ag.AddExtern(method.Signature.ToString());
        }
    }


    public class Conversion
    {
        /// <summary>
        /// Convert 8 bits to 16 bits Signed
        /// </summary>
        /// <param name="ec"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void EmitConvert8To16Signed(EmitContext ec, RegistersEnum src)
        {
            ec.EmitInstruction(new MoveSignExtend() { SourceReg = ec.GetLow(src), DestinationReg = src, Size = 80 });

        }
        /// <summary>
        /// Convert 8 bits to 16 bits
        /// </summary>
        /// <param name="ec"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void EmitConvert8To16Unsigned(EmitContext ec, RegistersEnum src)
        {
            ec.EmitInstruction(new MoveZeroExtend() { SourceReg = ec.GetLow(src), DestinationReg = src, Size = 80 });

        }
        /// <summary>
        /// Convert 16 to 8 
        /// </summary>
        /// <param name="ec"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void EmitConvert16To8(EmitContext ec, RegistersEnum src)
        {
            ec.EmitInstruction(new MoveZeroExtend() { SourceReg = ec.GetLow(src), DestinationReg = src, Size = 8 });

        }



    }
}