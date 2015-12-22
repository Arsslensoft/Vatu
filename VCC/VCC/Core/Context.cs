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
        LABEL,
        BOOL_EXPR,
        SW,
        FOR

    }
    [Flags]
    public enum ResolveScopes
    {
        AccessOperation = 1 << 1,
        Normal = 1,
        Loop = 1 << 2,
        If = 1 << 3,
        Case = 1 << 4
    }
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

    public interface IResolve
    {
        bool Resolve(ResolveContext rc);
        SimpleToken DoResolve(ResolveContext rc);
    }

    public interface ILoop
    {
        ILoop ParentLoop { get; set; }
        Label EnterLoop { get; set; }
        Label ExitLoop { get; set; }
        Label LoopCondition { get; set; }
     
    }
    public interface IConditional
    {
        IConditional ParentIf { get; set; }
        Label Else { get; set; }
        Label ExitIf { get; set; }
    }
    /// <summary>
    /// Emit Context
    /// </summary>
    public class EmitContext
    {
        
#if _16BITS
        public const byte TRUE = 255;
        public const RegistersEnum A = RegistersEnum.AX;
        public const RegistersEnum B = RegistersEnum.BX;
        public const RegistersEnum C = RegistersEnum.CX;
        public const RegistersEnum D = RegistersEnum.DX;
        public const RegistersEnum SP = RegistersEnum.SP;
        public const RegistersEnum BP = RegistersEnum.BP;
        public const RegistersEnum DI = RegistersEnum.DI;
        public const RegistersEnum SI = RegistersEnum.SI;
#elif _32BITS
        public const RegistersEnum A = RegistersEnum.EAX;
        public const RegistersEnum B = RegistersEnum.EBX;
        public const RegistersEnum C = RegistersEnum.ECX;
        public const RegistersEnum D = RegistersEnum.EDX;
        public const RegistersEnum SP = RegistersEnum.ESP;
        public const RegistersEnum BP = RegistersEnum.EBP;
        public const RegistersEnum DI = RegistersEnum.EDI;
        public const RegistersEnum SI = RegistersEnum.ESI;
#endif


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
        public void EmitPush(RegistersEnum rg, byte size = 80,bool adr=false)
        {
            EmitInstruction(new Push() { DestinationReg = rg, Size = size,DestinationIsIndirect = adr });
        }
      
        
   

       


        public void EmitBoolean(RegistersEnum rg, ConditionalTestEnum tr, ConditionalTestEnum fls)
        {
            EmitInstruction(new ConditionalMove() {  Condition = tr, DestinationReg = rg, Size = 80, SourceValue = TRUE });
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



        public void EmitData(DataMember dm, MemberSpec v, bool constant=false)
        {
            if (!Variables.ContainsKey(v.Signature.ToString()))
            {
                v.Reference =  ElementReference.New(v.Signature.ToString());
                if(constant)
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
        public Label DefineLabel(LabelType lbt,string suffix = null)
        {
          if(suffix == null)
            return ag.DefineLabel(GenerateLabelName(lbt));
          else return ag.DefineLabel(GenerateLabelName(lbt) + "_"+suffix);
        }
       

        public void Emit()
        {
            ag.Emit(ag.AssemblerWriter);
        }

        public void EmitDataWithConv(string name, object value, MemberSpec v, bool constant=false)
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
        public static void EmitConvert8To16Unsigned(EmitContext ec,RegistersEnum src)
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

    public class ResolveContext : IDisposable
    {
      
        public IConditional EnclosingIf { get; set; }
        public ILoop EnclosingLoop { get; set; }
        public Switch EnclosingSwitch { get; set; }
        public Label DefineLabel(string name)
        {
        
                return new Label(name);
          
        }
        public Label DefineLabel(LabelType lbt, string suffix = null)
        {
            if (suffix == null)
                return new Label(EmitContext.GenerateLabelName(lbt));
            else return new Label(EmitContext.GenerateLabelName(lbt) + "_" + suffix);
        }

        Block current_block;
        static Report rp;
        public static Report Report { get { return rp; } set { rp = value; } }

        public Stack<object> ResolverStack { get; set; }

       public List<ResolveContext> ChildContexts { get; set; }
       public bool IsInTypeDef { get; set; }
       public bool IsInStruct { get; set; }
       public bool IsInEnum { get; set; }
       public bool IsInVarDeclaration { get; set; }

       public Namespace CurrentNamespace { get; set; }
       public List<Namespace> Imports { get; set; }

       public ResolveScopes CurrentScope { get; set; }

       public int LocalStackIndex { get; set; }

        MethodSpec current_member;
        public MethodSpec CurrentMethod { get { return current_member; } set { current_member = value; } }

        TypeSpec current_type;
        public TypeSpec CurrentType { get { return current_type; } set { current_type = value; } }

       public Known _known;
        void FillKnown()
        {
            CurrentScope = ResolveScopes.Normal;
            ResolverStack = new Stack<object>();
            _known.KnownTypes.Add(BuiltinTypeSpec.Bool);
            _known.KnownTypes.Add(BuiltinTypeSpec.Byte);
            _known.KnownTypes.Add(BuiltinTypeSpec.SByte);
            _known.KnownTypes.Add(BuiltinTypeSpec.Int);
            _known.KnownTypes.Add(BuiltinTypeSpec.UInt);
            _known.KnownTypes.Add(BuiltinTypeSpec.String);
            _known.KnownTypes.Add(BuiltinTypeSpec.Void);
        }
         void Init(){
       
        IsInVarDeclaration = false;
        IsInTypeDef = false;
        IsInStruct = false;
        IsInEnum = false;
        LocalStackIndex = 0;
        Report = new ConsoleReporter();
    }
         public ResolveContext(List<Namespace> imp,Namespace ns, Block b, MethodSpec cm, Known known)
        {
            Imports = imp;
            CurrentNamespace = ns;
            _known = known;
            current_block = b;
            current_member = cm;
            Init();
        }
         public ResolveContext(List<Namespace> imp, Namespace ns, Block b, MethodSpec cm)
         {
             Imports = imp;
            CurrentNamespace = ns;
            _known = new Known();
            current_block = b;
            current_member = cm;
            FillKnown();
            Init();
        }
         public ResolveContext(List<Namespace> imp, Namespace ns, DeclarationSequence<Declaration> decl)
         {
             Imports = imp;
             CurrentNamespace = ns;
            Init();
            _known = new Known();
            current_member = new MethodSpec(CurrentNamespace, "<root-decl-list>", Modifiers.NoModifier, null, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
    
        }
         public ResolveContext(List<Namespace> imp, Namespace ns, MethodDeclaration decl)
         {
             Imports = imp;
            CurrentNamespace = ns;
            Init();
            _known = new Known();
            current_member = new MethodSpec(CurrentNamespace, decl.Identifier.Name, Modifiers.NoModifier, null, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
   
         
        }
         public ResolveContext(List<Namespace> imp, Namespace ns, StructDeclaration decl)
         {
             Imports = imp;
            CurrentNamespace = ns;
            Init();
            _known = new Known();
            current_member = new MethodSpec(CurrentNamespace, "<struct-decl>", Modifiers.NoModifier, null, Location.Null);
            current_type = new TypeSpec(CurrentNamespace, decl.Identifier.Name, 0, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
       
            IsInStruct = true;
        }
         public ResolveContext(List<Namespace> imp, Namespace ns, EnumDeclaration decl)
         {
             Imports = imp;
            CurrentNamespace = ns;
            Init();
            _known = new Known();
            current_member = new MethodSpec(CurrentNamespace, "<enum-decl>", Modifiers.NoModifier, null, Location.Null);
            current_type = new TypeSpec(CurrentNamespace, decl.Identifier.Name, 0, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();

            IsInEnum = true;
        }

        public List<VarSpec> GetLocals()
        {
            return _known.KnownLocalVars;
        }

        public void FillKnownByKnown(Known kn)
        {
            foreach (FieldSpec fs in kn.KnownGlobals)
                KnowField(fs);

            foreach (MethodSpec ms in kn.KnownMethods)
                KnowMethod(ms);

            foreach (TypeSpec ts in kn.KnownTypes)
                KnowType(ts);

      
        }
        public bool IsInGlobal()
        {
            return current_member.Name == "<root-decl-list>";
        }

        public ResolveContext CreateAsChild(List<Namespace> imp,Namespace ns, MethodDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp,ns,md);
                rc.FillKnownByKnown(_known);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, StructDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp,ns,md);
                rc.FillKnownByKnown(_known);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, EnumDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp,ns,md);
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
        public static ResolveContext CreateContextForMethod(List<Namespace> imp, Namespace ns, MethodSpec mtd, Block b)
        {
            return new ResolveContext(imp,ns,b, mtd);
        }
        public static ResolveContext CreateRootContext(List<Namespace> imp, Namespace ns, DeclarationSequence<Declaration> dcl)
        {
            return new ResolveContext(imp,ns,dcl);
        }


        #region Resolve Members
        public MemberSpec TryResolveName(Namespace ns,string name)
        {
            MemberSpec m = ResolveVar(ns,name);
            if (m == null)
            {
                m = ResolveParameter(name);
                if (m == null)
                {
                    m = ResolveField(ns,name);
                    if (m == null)
                        return null;
                  
                    else return m;
                }
                else return m;
            }
            else return m;
        }
        public TypeSpec ResolveType(Namespace ns, string name)
        {
            foreach (TypeSpec kt in _known.KnownTypes)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Name == name)
                    return kt;
            }

            return null;
        }
        public FieldSpec ResolveField(Namespace ns, string name)
        {
            foreach (FieldSpec kt in _known.KnownGlobals)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Name == name)
                    return kt;
            }

            return null;
        }
        public MethodSpec ResolveMethod(Namespace ns, string name)
        {
            foreach (MethodSpec kt in _known.KnownMethods)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Name == name)
                    return kt;
            }

            return null;
        }
        public VarSpec ResolveVar(Namespace ns, string name)
        {
            foreach (VarSpec kt in _known.KnownLocalVars)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Name == name)
                    return kt;
            }
            return null;
        }
        public ParameterSpec ResolveParameter( string name)
        {
            foreach (ParameterSpec kt in CurrentMethod.Parameters)
            {
        
                if (kt.Name == name)
                    return kt;
            }
            return null;
        }
        public EnumMemberSpec ResolveEnumValue(Namespace ns, string name)
        {
            foreach (TypeSpec kt in _known.KnownTypes)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt is EnumTypeSpec)
                {
                   
                    EnumTypeSpec ets = (EnumTypeSpec)kt;
                    foreach (EnumMemberSpec em in ets.Members)
                        if (em.Name == name)
                            return em;
                }
            }    

            return null;
        }


        public MethodSpec TryResolveMethod(string name)
        {
            MethodSpec ms = ResolveMethod(CurrentNamespace, name);
            if (ms == null)
            {
                ms = ResolveMethod(Namespace.Default, name);


                if (ms == null)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ms = ResolveMethod(ns, name);
                        if (ms != null)
                            return ms;
                    }
                }
            }
            return ms;
        }
        public VarSpec TryResolveVar(string name)
        {
            VarSpec ms = ResolveVar(CurrentNamespace, name);
              if (ms == null)
              {
                  ms = ResolveVar(Namespace.Default, name);
                  if (ms == null)
                  {
                      foreach (Namespace ns in Imports)
                      {
                          ms = ResolveVar(ns, name);
                          if (ms != null)
                              return ms;
                      }
                  }
              }
            return ms;
        }
        public FieldSpec TryResolveField(string name)
        {
            FieldSpec ms = ResolveField(CurrentNamespace, name);
           if (ms == null)
           {
               ms = ResolveField(Namespace.Default, name);
               if (ms == null)
               {
                   foreach (Namespace ns in Imports)
                   {
                       ms = ResolveField(ns, name);
                       if (ms != null)
                           return ms;
                   }
               }
           }
            return ms;
        }
        public MemberSpec TryResolveName(string name)
        {
            MemberSpec m = TryResolveVar( name);
            if (m == null)
            {
                m = ResolveParameter( name);
                if (m == null)
                {
                    m = TryResolveField( name);
                    if (m == null)
                        return null;

                    else return m;
                }
                else return m;
            }
            else return m;
        }
        public TypeSpec TryResolveType(string name)
        {
            TypeSpec ms = ResolveType(CurrentNamespace, name);
            if (ms == null)
            {
                ms = ResolveType(Namespace.Default, name);
                if (ms == null)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ms = ResolveType(ns, name);
                        if (ms != null)
                            return ms;
                    }
                }
            }
            return ms;
        }
        public EnumMemberSpec TryResolveEnumValue(string name)
        {
            EnumMemberSpec ms = ResolveEnumValue(CurrentNamespace, name);
           if (ms == null)
           {
               ms = ResolveEnumValue(Namespace.Default, name);
               if (ms == null)
               {
                   foreach (Namespace ns in Imports)
                   {
                       ms = ResolveEnumValue(ns, name);
                       if (ms != null)
                           return ms;
                   }
               }
           }
            return ms;
        }

        #endregion

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
                LocalStackIndex -= mtd.MemberType.Size;
                mtd.StackIdx = LocalStackIndex;
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
    public class CompilerContext
    {
        public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
        {
            return new Location( li.Line, li.Column);
        }
    }
}