using System;
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

    public interface IEmitAddress
    {
        bool LoadEffectiveAddress(EmitContext ec);
      
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


        public const byte TRUE = 1;
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



        internal Vasm.AsmContext ag;
        List<string> _names;

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
        public void EmitPop(RegistersEnum rg, byte size = 80, bool adr = false, int off = 0)
        {
            if (Registers.Is8Bit(rg))
                rg = ag.GetHolder(rg);
            if (size == 8)
            {
                ag.SetAsUsed(rg);
                RegistersEnum drg = ag.GetNextRegister();
                ag.FreeRegister();
                ag.FreeRegister();

                EmitInstruction(new Pop() { DestinationReg = drg, Size = 16 });
                if (off != 0)
                    EmitInstruction(new Mov() { DestinationReg = rg, Size = 8, DestinationIsIndirect = adr, DestinationDisplacement = off, SourceReg = GetLow(drg) });
                else EmitInstruction(new Mov() { DestinationReg = rg, Size = 8, DestinationIsIndirect = adr, SourceReg = GetLow(drg) });
            }
            else
            {
                if (off != 0)
                    EmitInstruction(new Pop() { DestinationReg = rg, Size = 16, DestinationIsIndirect = adr, DestinationDisplacement = off });
                else EmitInstruction(new Pop() { DestinationReg = rg, Size = 16, DestinationIsIndirect = adr });
            }
        }
        public void EmitPush(bool v)
        {
            EmitInstruction(new Push() { DestinationValue = (v ? (ushort)EmitContext.TRUE : (ushort)0), Size = 16 });
        }
        public void EmitPush(byte v)
        {
            EmitInstruction(new Push() { DestinationValue = v, Size = 16 });
        }
        public void EmitPush(ushort v)
        {
            EmitInstruction(new Push() { DestinationValue = v, Size = 16 });
        }
        public void EmitPush(RegistersEnum rg, byte size = 80, bool adr = false, int off = 0)
        {
            if (Registers.Is8Bit(rg))
                rg = ag.GetHolder(rg);
            if (size == 8)
            {
                ag.SetAsUsed(rg);
                RegistersEnum drg = ag.GetNextRegister();
                ag.FreeRegister();
                ag.FreeRegister();
                EmitInstruction(new MoveZeroExtend() { DestinationReg = drg, Size = 8, SourceReg = rg, SourceDisplacement = off, SourceIsIndirect = adr });

                EmitInstruction(new Push() { DestinationReg = drg, Size = 16 });
            }
            else
                EmitInstruction(new Push() { DestinationReg = rg, Size = 16, DestinationIsIndirect = adr, DestinationDisplacement = off });
        }
        public void EmitPushSigned(RegistersEnum rg, byte size = 80, bool adr = false, int off = 0)
        {
            if (Registers.Is8Bit(rg))
                rg = ag.GetHolder(rg);
            if (size == 8)
            {
                ag.SetAsUsed(rg);
                RegistersEnum drg = ag.GetNextRegister();
                ag.FreeRegister();
                ag.FreeRegister();
                EmitInstruction(new MoveSignExtend() { DestinationReg = drg, Size = 8, SourceReg = rg, SourceDisplacement = off, SourceIsIndirect = adr });

                EmitInstruction(new Push() { DestinationReg = drg, Size = 16 });
            }
            else
                EmitInstruction(new Push() { DestinationReg = rg, Size = 16, DestinationIsIndirect = adr, DestinationDisplacement = off });
        }

        public void EmitLoadFloat(RegistersEnum rg, byte size = 32, bool adr = false, int off = 0)
        {
                EmitInstruction(new Vasm.x86.x87.FloatLoad() { DestinationReg = rg, Size = 16, DestinationIsIndirect = adr, DestinationDisplacement = off });
        }
        public void EmitStoreFloat(RegistersEnum rg, byte size = 32, bool adr = false, int off = 0)
        {
            EmitInstruction(new Vasm.x86.x87.FloatStoreAndPop() { DestinationReg = rg, Size = size, DestinationIsIndirect = adr, DestinationDisplacement = off });
        }


        public void EmitBooleanBranch(bool v, Label truecase, ConditionalTestEnum tr, ConditionalTestEnum fls)
        {
            if (v)
                EmitInstruction(new ConditionalJump() { Condition = tr, DestinationLabel = truecase.Name });
            else EmitInstruction(new ConditionalJump() { Condition = fls, DestinationLabel = truecase.Name });

        }
        public void EmitBoolean(RegistersEnum rg, ConditionalTestEnum tr, ConditionalTestEnum fls)
        {
            //  EmitInstruction(new Xor() { SourceReg = ag.GetHolder(rg), DestinationReg =  ag.GetHolder(rg), Size = 80 });
            EmitInstruction(new ConditionalSet() { Condition = tr, DestinationReg = GetLow(rg), Size = 80 });
            EmitInstruction(new MoveZeroExtend() { SourceReg = GetLow(rg), DestinationReg = ag.GetHolder(rg), Size = 80 });
            /*
              EmitInstruction(new ConditionalMove() { Condition = tr, DestinationReg = rg, Size = 80, SourceValue = TRUE });
              EmitInstruction(new ConditionalMove() { Condition = fls, DestinationReg = rg, Size = 80, SourceValue = 0 });
             * 
             * */
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
            EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (v ? (ushort)EmitContext.TRUE : (ushort)0), Size = 80 });
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

                sv.Size = mem.MemberType.IsPointer ? 2 : mem.MemberType.Size;
                if (!mem.MemberType.IsPointer)
                {
                    sv.IsStruct = mem.MemberType.IsStruct;
                    sv.Type = sv.IsStruct ? mem.MemberType.Signature.ToString() : "";
                }
                else sv.Type = "";

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
            if (ag.Externals.Contains(lb.Name))
                ag.Externals.Remove(lb.Name);
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
        public void EmitDataWithConv(string name, MemberSpec v, string value)
        {
            DataMember dm = new DataMember(name, value.ToString());
            EmitData(dm, v, false);
        }
        public void EmitDataWithConv(string name, object value, MemberSpec v, bool constant = false,bool verbatim = false)
        {
            DataMember dm;
            if (value is string)
            {
                if (constant)
                    dm = new DataMember(name, value.ToString(), true, verbatim);
                else dm = new DataMember(name, value.ToString(), false, verbatim);
            }
            else if (value is float)
                dm = new DataMember(name, BitConverter.GetBytes((float)value));
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
        public void EmitINT(ushort num, Label method)
        {
            InterruptDef idef = new InterruptDef();
            idef.Number = num;
            idef.Destination = method;
            ag.Interrupts.Add(idef);
        }
        public bool AddInstanceOfStruct(string varname, TypeSpec st)
        {
            if (st.IsStruct && !ag.DeclaredStructVars.ContainsKey(varname))
                return ag.DefineStructInstance(varname, st.Signature.ToString());
            else return false;
        }
        public void DefineExtern(MemberSpec method)
        {
            ag.AddExtern(method.Signature.ToString());
        }
        public void DefineGlobal(MemberSpec method)
        {
            ag.Globals.Add(method.Signature.ToString());
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


        public static void EmitConvert16ToFloat(EmitContext ec,VTC.Core.Expr lea)
        {
            lea.EmitToStack(ec);
            ec.EmitPop(RegistersEnum.SI);
            ec.EmitInstruction(new Vasm.x86.x87.IntLoad() { DestinationReg = EmitContext.SI, DestinationIsIndirect = true, Size = 16 });   
        }
        public static void EmitConvertFloatTo16(EmitContext ec, VTC.Core.Expr e)
        {
            e.EmitToStack(ec); // push float
            ec.EmitPush((ushort)0);// reserve word
            ec.EmitInstruction(new Mov() {  DestinationReg = EmitContext.SI, SourceReg = EmitContext.SP}); // mov si,sp
            ec.EmitInstruction(new Vasm.x86.x87.IntStoreAndPop() { DestinationReg = EmitContext.SI, Size = 16 ,DestinationIsIndirect = true }); // store int to [si]
            

        }
        public static void EmitConvert16ToFloatStack(EmitContext ec, VTC.Core.Expr e)
        {
            // int already in stack
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceReg = EmitContext.SP }); // mov si,sp
            ec.EmitInstruction(new Vasm.x86.x87.IntLoad() { DestinationReg = EmitContext.SI, DestinationIsIndirect = true, Size = 16 });
            // store float
            e.EmitFromStack(ec);
        }
        public static void EmitConvertFloatTo16Stack(EmitContext ec, VTC.Core.Expr e)
        {
            // float in stack
            ec.EmitPush((ushort)0);// reserve word
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.SI, SourceReg = EmitContext.SP }); // mov si,sp
            ec.EmitInstruction(new Vasm.x86.x87.IntStoreAndPop() { DestinationReg = EmitContext.SI, Size = 16, DestinationIsIndirect = true }); // store int to [si]
            e.EmitFromStack(ec);

        }
    }
}