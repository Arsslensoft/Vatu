﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using VTC.Core;

namespace VTC
{

    public interface ILoop
    {
        ILoop ParentLoop { get; set; }
        Label EnterLoop { get; set; }
        Label ExitLoop { get; set; }
        Label LoopCondition { get; set; }
       // bool HasInterrupt { get; set; }

    }
    public interface IConditional
    {
        IConditional ParentIf { get; set; }
        Label Else { get; set; }
        Label ExitIf { get; set; }
    }
    public interface IResolve
    {
        FlowState DoFlowAnalysis(FlowAnalysisContext fc); 
        SimpleToken DoResolve(ResolveContext rc);
        bool Resolve(ResolveContext rc);
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
        FOR,
        IF_EXPR

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
        static ResolveContext()
        {
            Report = new ConsoleReporter();
        }
        public Stack<object> ResolverStack { get; set; }

        public List<ResolveContext> ChildContexts { get; set; }
        public bool IsInTypeDef { get; set; }
        public bool IsInStruct { get; set; }
        public bool IsInUnion { get; set; }
        public bool IsInEnum { get; set; }
        public bool IsInVarDeclaration { get; set; }
        public TypeSpec CurrentExtensionLookup
        {
            get { return Resolver.CurrentExtensionLookup; }
            set { Resolver.CurrentExtensionLookup = value; }
        }
        public bool StaticExtensionLookup
        {
            get { return Resolver.IsExtensionStatic; }
            set { Resolver.IsExtensionStatic = value; }
        }
        public Expr ExtensionVar { get; set; }
        public Namespace CurrentNamespace  { get { return Resolver.CurrentNamespace; }
            set { Resolver.CurrentNamespace = value; } }
        public List<Namespace> Imports { get { return Resolver.Imports; } set { Resolver.Imports = value; } }

        public ResolveScopes CurrentScope { get; set; }

        public int LocalStackIndex { get; set; }

      
        public MethodSpec CurrentMethod { get { return Resolver.CurrentMethod; } set { Resolver.CurrentMethod = value; } }

        TypeSpec current_type;
        public TypeSpec CurrentType { get { return current_type; } set { current_type = value; } }

        public Resolver Resolver{get;set;}
        void FillKnown()
        {
            CurrentScope = ResolveScopes.Normal;
            ResolverStack = new Stack<object>();
            Resolver.KnowType(BuiltinTypeSpec.Bool);
            Resolver.KnowType(BuiltinTypeSpec.Byte);
            Resolver.KnowType(BuiltinTypeSpec.SByte);
            Resolver.KnowType(BuiltinTypeSpec.Int);
            Resolver.KnowType(BuiltinTypeSpec.UInt);
            Resolver.KnowType(BuiltinTypeSpec.String);
            Resolver.KnowType(BuiltinTypeSpec.Pointer);
            Resolver.KnowType(BuiltinTypeSpec.Void);
            Resolver.KnowType(BuiltinTypeSpec.Float);
        }
        void Init()
        {

            IsInVarDeclaration = false;
            IsInTypeDef = false;
            IsInStruct = false;
            IsInUnion = false;
            IsInEnum = false;
            LocalStackIndex = 0;
      
        }
        public ResolveContext(List<Namespace> imp, Namespace ns, Block b, MethodSpec cm, Resolver known)
        {
          
      
            Resolver = known;
            CurrentNamespace = ns;
            current_block = b;
 
            Init();
        }
        public ResolveContext(List<Namespace> imp, Namespace ns, Block b, MethodSpec cm)
        {
     
       
            Resolver = new Resolver(ns, imp,cm);
            current_block = b;
            CurrentNamespace = ns;
            FillKnown();
            Init();
        }
        public ResolveContext(List<Namespace> imp, Namespace ns, DeclarationSequence<Declaration> decl)
        {
       

            Init();
            Resolver = new Resolver(ns,imp,new MethodSpec(ns, "<root-decl-list>", Modifiers.NoModifier, null, CallingConventions.StdCall,null, Location.Null));
          
            FillKnown();
            ChildContexts = new List<ResolveContext>();

        }
        public ResolveContext(List<Namespace> imp, Namespace ns, InterruptDeclaration decl)
        {
      

            Init();
            Resolver = new Resolver(ns, imp, new MethodSpec(ns, decl.ItName, Modifiers.NoModifier, null, CallingConventions.StdCall, null, Location.Null));

            FillKnown();
            ChildContexts = new List<ResolveContext>();


        }
        public ResolveContext(List<Namespace> imp, Namespace ns, OperatorDeclaration decl)
        {
      

            Init();
            Resolver = new Resolver(ns, imp, new MethodSpec(ns, decl.OpName, Modifiers.NoModifier, null, CallingConventions.StdCall, null, Location.Null));

            FillKnown();
            ChildContexts = new List<ResolveContext>();


        }
        public ResolveContext(List<Namespace> imp, Namespace ns, MethodDeclaration decl)
        {
         
 
            Init();
            Resolver = new Resolver(ns, imp, new MethodSpec(ns, decl.Identifier.Name, Modifiers.NoModifier, null, CallingConventions.StdCall,null, Location.Null));
     
            FillKnown();
            ChildContexts = new List<ResolveContext>();


        }
        public ResolveContext(List<Namespace> imp, Namespace ns, StructDeclaration decl)
        {
          
            
            Init();
            Resolver = new Resolver(ns, imp, new MethodSpec(ns, "<struct-decl>", Modifiers.NoModifier, null, CallingConventions.StdCall,null, Location.Null));
            current_type = new TypeSpec(CurrentNamespace, decl.Identifier.Name, 0, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();

            IsInStruct = true;
        }
        public ResolveContext(List<Namespace> imp, Namespace ns, UnionDeclaration decl)
        {
       

            Init();
            Resolver = new Resolver(ns, imp, new MethodSpec(ns, "<union-decl>", Modifiers.NoModifier, null, CallingConventions.StdCall, null, Location.Null));
            current_type = new TypeSpec(CurrentNamespace, decl.Identifier.Name, 0, BuiltinTypes.Unknown, TypeFlags.Union, Modifiers.NoModifier, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();

            IsInUnion = true;
        }
        public ResolveContext(List<Namespace> imp, Namespace ns, EnumDeclaration decl)
        {
          
        
            Init();
            Resolver = new Resolver(ns, imp,new MethodSpec(ns, "<enum-decl>", Modifiers.NoModifier, null,CallingConventions.StdCall,null, Location.Null));
       
            current_type = new TypeSpec(CurrentNamespace, decl.Identifier.Name, 0, BuiltinTypes.Unknown, TypeFlags.Struct, Modifiers.NoModifier, Location.Null);
            FillKnown();
            ChildContexts = new List<ResolveContext>();

            IsInEnum = true;
        }

        public List<VarSpec> GetLocals()
        {
            return Resolver.KnownLocalVars;
        }

        public void FillKnownByKnown(Resolver kn)
        {
            foreach (FieldSpec fs in kn.KnownGlobals)
                KnowField(fs);

            foreach (MethodSpec ms in kn.KnownMethods)
                KnowMethod(ms);

            foreach (TypeSpec ts in kn.KnownTypes)
                KnowType(ts);

            foreach (OperatorSpec op in kn.KnownOperators)
                Resolver.KnowOperator(op);

            foreach (Namespace ns in kn.KnownNamespaces)
                Resolver.KnowNamespace(ns);

            foreach (VarSpec ms in kn.KnownLocalVars)
                Resolver.KnowGVar(ms);
        }
        public bool IsInGlobal()
        {
            return Resolver.CurrentMethod.Name == "<root-decl-list>";
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, InterruptDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp, ns, md);
                rc.FillKnownByKnown(Resolver);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, OperatorDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp, ns, md);
                rc.FillKnownByKnown(Resolver);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, MethodDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp, ns, md);
                rc.FillKnownByKnown(Resolver);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, StructDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp, ns, md);
                rc.FillKnownByKnown(Resolver);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, UnionDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp, ns, md);
                rc.FillKnownByKnown(Resolver);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }
        public ResolveContext CreateAsChild(List<Namespace> imp, Namespace ns, EnumDeclaration md)
        {
            if (ChildContexts != null)
            {
                ResolveContext rc = new ResolveContext(imp, ns, md);
                rc.FillKnownByKnown(Resolver);
                ChildContexts.Add(rc);
                return rc;

            }
            else return null;
        }

        public void UpdateFather(ResolveContext rc)
        {
            foreach (MethodSpec m in rc.Resolver.KnownMethods)
                KnowMethod(m);
            foreach (TypeSpec m in rc.Resolver.KnownTypes)
                KnowType(m);
            foreach (FieldSpec m in rc.Resolver.KnownGlobals)
                KnowField(m);

            foreach (OperatorSpec op in rc.Resolver.KnownOperators)
                Resolver.KnowOperator(op);

            foreach (VarSpec op in rc.Resolver.KnownLocalVars)
                Resolver.KnowGVar(op);
        }
        public void UpdateChildContext(string name, ResolveContext crc)
        {
            int idx = 0;
            foreach (ResolveContext rc in ChildContexts)
            {
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
            return new ResolveContext(imp, ns, b, mtd);
        }
        public static ResolveContext CreateRootContext(List<Namespace> imp, Namespace ns, DeclarationSequence<Declaration> dcl)
        {
            return new ResolveContext(imp, ns, dcl);
        }


       

        public bool Exist(MemberSpec m, List<MemberSpec> l)
        {
            foreach (MemberSpec ms in l)
                if (m.Signature.ToString() == ms.Signature.ToString())
                    return true;

            return false;
        }
        public bool Extend(TypeSpec tp, FieldSpec mtd, bool stat = false)
        {
            return Resolver.Extend(tp, mtd);
        }
        public bool Extend(TypeSpec tp,MethodSpec mtd, bool stat=false)
        {
            if(!stat)
           return Resolver.Extend(tp, mtd);
            else return Resolver.ExtendStatic(tp, mtd);
        }
        public void KnowMethod(MethodSpec mtd)
        {
            if (!Exist((MemberSpec)mtd, Resolver.KnownMethods.Cast<MemberSpec>().ToList<MemberSpec>()))
            {
                Resolver.KnownMethods.Add(mtd);
            }
        }
        public bool KnowVar(VarSpec mtd)
        {
            if (!Exist((MemberSpec)mtd, Resolver.KnownLocalVars.Cast<MemberSpec>().ToList<MemberSpec>()))
            {
                LocalStackIndex -= mtd.memberType.IsArray ? mtd.memberType.GetSize(mtd.memberType) : mtd.MemberType.Size;
                mtd.StackIdx = LocalStackIndex;
                Resolver.KnowVar(mtd);
                return true;
            }
            else return false;
        }
        public void UpdateType(TypeSpec old, TypeSpec ne)
        {
            if (Exist((MemberSpec)old, Resolver.KnownTypes.Cast<MemberSpec>().ToList<MemberSpec>()))
            {
                Resolver.KnownTypes[Resolver.KnownTypes.IndexOf(old)] = ne;
            
            }
        }
        public void UpdatField(FieldSpec old, FieldSpec ne)
        {
            if (Exist((MemberSpec)old, Resolver.KnownGlobals.Cast<MemberSpec>().ToList<MemberSpec>()))
                Resolver.KnownGlobals[Resolver.KnownGlobals.IndexOf(old)] = ne;

         
        }
        public void UpdateMethod(MethodSpec old, MethodSpec ne)
        {
            if (Exist((MemberSpec)old, Resolver.KnownTypes.Cast<MemberSpec>().ToList<MemberSpec>()))
                Resolver.KnownMethods[Resolver.KnownMethods.IndexOf(old)] = ne;
        }


        public void KnowType(TypeSpec mtd)
        {
            if (!Exist((MemberSpec)mtd, Resolver.KnownTypes.Cast<MemberSpec>().ToList<MemberSpec>()))
                Resolver.KnownTypes.Add(mtd);
        }
        public void KnowField(FieldSpec mtd)
        {
            if (!Exist((MemberSpec)mtd, Resolver.KnownGlobals.Cast<MemberSpec>().ToList<MemberSpec>()))
                Resolver.KnownGlobals.Add(mtd);
        }


        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

    }
}
