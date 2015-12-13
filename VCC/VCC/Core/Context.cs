using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VCC.Core;

namespace VCC
{
    public class Known
    {
        public List<TypeSpec> KnownTypes { get; set; }
        public List<FieldSpec> KnownGlobals { get; set; }
        public List<MethodSpec> KnownMethods { get; set; }
        public List<VarSpec> KnownLocalVars { get; set; }

        public Known()
        {
            KnownGlobals = new List<FieldSpec>();
            KnownMethods = new List<MethodSpec>();
            KnownTypes = new List<TypeSpec>();
            KnownLocalVars = new List<VarSpec>();
          
      
        }
    }
    public enum LabelType
    {
        LOOP,
        IF,
        ELSE,
        WHILE,
        DO,
        DO_WHILE,
        CASE,
        LABEL

    }


    public interface IEmitExpr
    {
        bool EmitToStack(EmitContext ec);
        bool EmitFromStack(EmitContext ec);
        bool EmitToRegister(EmitContext ec, RegistersEnum rg);
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

    public interface IResolve
    {
        bool Resolve(ResolveContext rc);
        SimpleToken DoResolve(ResolveContext rc);
    }

    /// <summary>
    /// Emit Context
    /// </summary>
    public class EmitContext
    {

        Vasm.AsmContext ag;
        List<string> _names;
        Label enterLoop, exitLoop;
        Dictionary<string, VarSpec> Variables = new Dictionary<string, VarSpec>();
    
       public RegistersEnum GetNextRegister()
        {
            RegistersEnum acc = ag.GetNextRegister();
            if (acc == RegistersEnum.SP)
                throw new ArgumentException("All Registers used");
            return acc;
        }
       public void FreeRegister()
       {
           ag.FreeRegister();
       }
       public RegistersEnum FirstRegister()
       {
       return    ag.PeekRegister();
       }
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
        string GenerateLabelName(LabelType lb)
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
        public void EmitPop(RegistersEnum rg)
        {
            EmitInstruction(new Pop() { DestinationReg = rg, Size = 80 });
        }
        public void EmitPush(RegistersEnum rg)
        {
            EmitInstruction(new Push() { DestinationReg = rg, Size = 80 });
        }
        public void EmitPush(VarSpec v)
        {
            EmitInstruction(new Mov() { DestinationReg = RegistersEnum.AX, Size = 80, SourceReg = RegistersEnum.BP, SourceDisplacement = v.StackIndex, SourceIsIndirect = true });
            EmitInstruction(new Push() { DestinationReg = RegistersEnum.AX, Size = 80 });
        }
        public void EmitPop(VarSpec v)
        {
            EmitInstruction(new Pop() { DestinationReg = FirstRegister(), Size = 80 });
            EmitInstruction(new Mov() { SourceReg = FirstRegister(), Size = 80, DestinationReg = RegistersEnum.BP, DestinationDisplacement = v.StackIndex, DestinationIsIndirect = true });
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



        public void EmitData(DataMember dm, MemberSpec v)
        {
            if (!Variables.ContainsKey(v.Signature.ToString()))
            {
                v.Reference =  ElementReference.New(v.Signature.ToString());
                ag.DefineData(dm);

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
        public Label DefineLabel(LabelType lbt)
        {
            return ag.DefineLabel(GenerateLabelName(lbt));
        }
        public void MarkLoopEnter()
        {
            enterLoop = DefineLabel(LabelType.LOOP);
            MarkLabel(enterLoop);

        }
        public void MarkLoopExit()
        {
            exitLoop = DefineLabel("E_" + enterLoop.Name);
            MarkLabel(exitLoop);
        }

        public void Emit()
        {
            ag.Emit(ag.AssemblerWriter);
        }

        public void EmitDataWithConv(string name, object value, MemberSpec v)
        {
            DataMember dm;
            if (value is string)
                dm = new DataMember(name, Encoding.ASCII.GetBytes(value.ToString()));
            else if (value is bool)
                dm = new DataMember(name, ((bool)value) ? (new byte[1] { 255 }) : (new byte[1] { 0 }));
            else if (value is byte)
                dm = new DataMember(name, new byte[1] { (byte)value });
            else if (value is ushort)
                dm = new DataMember(name, new ushort[1] { (ushort)value });
            else dm = new DataMember(name, new object[1] { value });

            EmitData(dm,v);
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

    public class ResolveContext : IDisposable
    {
        Block current_block;
       public Report Report { get; set; }
       public List<ResolveContext> ChildContexts { get; set; }
       public bool IsInTypeDef { get; set; }
       public bool IsInStruct { get; set; }
       public bool IsInEnum { get; set; }

       public int LocalStackIndex { get; set; }

        MethodSpec current_member;
        public MethodSpec CurrentMethod { get { return current_member; } set { current_member = value; } }

        TypeSpec current_type;
        public TypeSpec CurrentType { get { return current_type; } }

       public Known _known;
        void FillKnown()
        {
            _known.KnownTypes.Add(BuiltinTypeSpec.Bool);
            _known.KnownTypes.Add(BuiltinTypeSpec.Byte);
            _known.KnownTypes.Add(BuiltinTypeSpec.SByte);
            _known.KnownTypes.Add(BuiltinTypeSpec.Int);
            _known.KnownTypes.Add(BuiltinTypeSpec.UInt);
            _known.KnownTypes.Add(BuiltinTypeSpec.String);
            _known.KnownTypes.Add(BuiltinTypeSpec.Void);
        }
    
        public ResolveContext(Block b, MethodSpec cm, Known known)
        {
            _known = known;
            current_block = b;
            current_member = cm;
            IsInTypeDef = false;
            IsInStruct = false;
            IsInEnum = false;
        }
        public ResolveContext(Block b, MethodSpec cm)
        {
            _known = new Known();
            current_block = b;
            current_member = cm;
            FillKnown();
            IsInTypeDef = false;
            IsInStruct = false;
            IsInEnum = false;
        }
        public ResolveContext(DeclarationSequence<Declaration> decl)
        {
            _known = new Known();
            current_member = new MethodSpec("<root-decl-list>", Modifiers.NoModifier, null, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
            IsInTypeDef = false;
            IsInStruct = false;
            IsInEnum = false;
        }
        public ResolveContext(MethodDeclaration decl)
        {
            _known = new Known();
            current_member = new MethodSpec(decl.Identifier.Name, Modifiers.NoModifier, null, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
            IsInTypeDef = false;
            IsInStruct = false;
            IsInEnum = false;
            LocalStackIndex = 0;
        }
        public ResolveContext(StructDeclaration decl)
        {
            _known = new Known();
            current_member = new MethodSpec("<struct-decl>", Modifiers.NoModifier, null, Location.Null);
            current_type = new TypeSpec(decl.Identifier.Name, 0, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
            IsInTypeDef = false;
            IsInStruct = true;
        }

        public List<VarSpec> GetLocals()
        {
            return _known.KnownLocalVars;
        }

        public void FillKnownByKnown(Known kn)
        {
            foreach (FieldSpec fs in kn.KnownGlobals)
                _known.KnownGlobals.Add(fs);

            foreach (MethodSpec ms in kn.KnownMethods)
                _known.KnownMethods.Add(ms);

            foreach (TypeSpec ts in kn.KnownTypes)
                _known.KnownTypes.Add(ts);

      
        }
        public bool IsInGlobal()
        {
            return current_member.Name == "<root-decl-list>";
        }

        public ResolveContext CreateAsChild(MethodDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(md);
                rc.FillKnownByKnown(_known);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(StructDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(md);
                rc.FillKnownByKnown(_known);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public void UpdateFather(ResolveContext rc)
        {
            foreach (MethodSpec m in rc._known.KnownMethods)
                KnowMethod(m);
            foreach (TypeSpec m in rc._known.KnownTypes)
                KnowType(m);
            foreach (FieldSpec m in rc._known.KnownGlobals)
                KnowField(m);
        }
        public void UpdateChildContext(string name, ResolveContext crc)
        {
            int idx = 0;
            foreach(ResolveContext rc in ChildContexts){
                if (rc.CurrentMethod.Name == name)
                    break;
                idx++;
                }

            if (idx < ChildContexts.Count)
                ChildContexts.RemoveAt(idx);

            ChildContexts.Add(crc);
        }
        public static ResolveContext CreateContextForMethod(MethodSpec mtd, Block b)
        {
            return new ResolveContext(b, mtd);
        }
        public static ResolveContext CreateRootContext(DeclarationSequence<Declaration> dcl)
        {
            return new ResolveContext(dcl);
        }

        public TypeSpec ResolveType(string name)
        {
            foreach (TypeSpec kt in _known.KnownTypes)
                if (kt.Name == name)
                    return kt;

            return null;
        }
        public FieldSpec ResolveField(string name)
        {
            foreach (FieldSpec kt in _known.KnownGlobals)
                if (kt.Name == name)
                    return kt;

            return null;
        }
        public MethodSpec ResolveMethod(string name)
        {
            foreach (MethodSpec kt in _known.KnownMethods)
                if (kt.Name == name)
                    return kt;

            return null;
        }
        public VarSpec ResolveVar(string name)
        {
            foreach (VarSpec kt in _known.KnownLocalVars)
                if (kt.Name == name)
                    return kt;

            return null;
        }

        public bool Exist(MemberSpec m, List<MemberSpec> l)
        {
            foreach (MemberSpec ms in l)
                if (m.Signature.ToString() == ms.Signature.ToString())
                    return true;

            return false;
        }


        public void KnowMethod(MethodSpec mtd)
        {
            if (!Exist((MemberSpec)mtd, _known.KnownMethods.Cast<MemberSpec>().ToList<MemberSpec>()))
            {
                _known.KnownMethods.Add(mtd);
            }
        }
        public void KnowVar(VarSpec mtd)
        {
            if (!Exist((MemberSpec)mtd,_known.KnownLocalVars.Cast<MemberSpec>().ToList<MemberSpec>() ))
            {
                LocalStackIndex -= 2;
                mtd.StackIndex = LocalStackIndex;
                _known.KnownLocalVars.Add(mtd);
            }
        }
        public void KnowType(TypeSpec mtd)
        {
            if (!Exist((MemberSpec)mtd, _known.KnownTypes.Cast<MemberSpec>().ToList<MemberSpec>()))
              _known.KnownTypes.Add(mtd);
        }
        public void KnowField(FieldSpec mtd)
        {
            if (!Exist((MemberSpec)mtd, _known.KnownGlobals.Cast<MemberSpec>().ToList<MemberSpec>()))
            _known.KnownGlobals.Add(mtd);
        }

    
        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

    }

    public class AtomicContext
    {
        public Dictionary<RegistersEnum, Expr> RegistersInUse { get; set; }
        public AtomicContext()
        {
         RegistersInUse = new Dictionary<RegistersEnum,Expr>();

        }

      
    }

    public class CompilerContext
    {
        public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
        {
            return new Location(new SourceFile("this", "null", 0), li.Line, li.Column);
        }
    }
}