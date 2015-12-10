using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
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

        public static Dictionary<LabelType, int> Labels = new Dictionary<LabelType, int>();
        EmitContext()
        {
            _names = new List<string>();
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

        public EmitContext(Vasm.AssemblyWriter asmw)
        {
            ag = new Vasm.AsmContext(asmw);
        }

        public EmitContext(Vasm.AsmContext ac)
        {
            ag = ac;
        }

        public void EmitInstruction(Instruction ins)
        {
            ag.Emit(ins);
        }

        public void EmitData(DataMember dm)
        {
            ag.DefineData(dm);
        }
        public void EmitData(string name, byte[] data)
        {
            ag.DefineData(DefineData(name, data));
        }
        public DataMember DefineData(string name, byte[] data)
        {
            return ag.Declare(name, data);
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
    }

    public class ResolveContext : IDisposable
    {
        Block current_block;
       public Report Report { get; set; }
       public List<ResolveContext> ChildContexts { get; set; }
        MethodSpec current_member;
        public MethodSpec CurrentMethod { get { return current_member; } }
        Known _known;
        void FillKnown()
        {
            _known.KnownTypes.Add(BuiltinTypeSpec.Bool);
            _known.KnownTypes.Add(BuiltinTypeSpec.Char);
            _known.KnownTypes.Add(BuiltinTypeSpec.Byte);
            _known.KnownTypes.Add(BuiltinTypeSpec.Short);
            _known.KnownTypes.Add(BuiltinTypeSpec.UShort);
            _known.KnownTypes.Add(BuiltinTypeSpec.Int);
            _known.KnownTypes.Add(BuiltinTypeSpec.UInt);
            _known.KnownTypes.Add(BuiltinTypeSpec.Long);
            _known.KnownTypes.Add(BuiltinTypeSpec.ULong);
            _known.KnownTypes.Add(BuiltinTypeSpec.Float);
            _known.KnownTypes.Add(BuiltinTypeSpec.Double);
            _known.KnownTypes.Add(BuiltinTypeSpec.String);
            _known.KnownTypes.Add(BuiltinTypeSpec.Void);
        }
        public ResolveContext(Block b, MethodSpec cm, Known known)
        {
            _known = known;
            current_block = b;
            current_member = cm;
       

        }
        public ResolveContext(Block b, MethodSpec cm)
        {
            _known = new Known();
            current_block = b;
            current_member = cm;
            FillKnown();
        }
        public ResolveContext(DeclarationSequence<Declaration> decl)
        {
            _known = new Known();
            current_member = new MethodSpec("<root-decl-list>", Modifiers.NoModifier, null, null, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
        }
        public ResolveContext(MethodDeclaration decl)
        {
            _known = new Known();
            current_member = new MethodSpec(decl.Identifier.Name, Modifiers.NoModifier, null, null, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();
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

        public void KnowMethod(MethodSpec mtd)
        {
            _known.KnownMethods.Add(mtd);
        }
        public void KnowVar(VarSpec mtd)
        {
            _known.KnownLocalVars.Add(mtd);
        }
        public void KnowType(TypeSpec mtd)
        {
            _known.KnownTypes.Add(mtd);
        }
        public void KnowField(FieldSpec mtd)
        {
            _known.KnownGlobals.Add(mtd);
        }
        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

    }

    public class CompilerContext
    {
        public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
        {
            return new Location(new SourceFile("this", "null", 0), li.Line, li.Column);
        }
    }
}