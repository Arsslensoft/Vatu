using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC
{
   /// <summary>
   /// Type & members resolver
   /// </summary>
   public class Resolver
    {
       public Resolver Parent { get; set; }
       public Namespace CurrentNamespace { get; set; }
       public TypeSpec CurrentExtensionLookup { get; set; }
       public TypeSpec CurrentClassLookup{ get; set; }
       public bool IsExtensionStatic { get; set; }
       public List<Namespace> Imports { get; set; }
       public MethodSpec CurrentMethod { get; set; }

       public List<OperatorSpec> KnownOperators { get; set; }
        public List<TypeSpec> KnownTypes { get; set; }
        public List<FieldSpec> KnownGlobals { get; set; }
        public List<MethodSpec> KnownMethods { get; set; }
        public List<VarSpec> KnownLocalVars { get; set; }
        public List<VarSpec> GloballyKnownLocals { get; set; }
        public List<Namespace> KnownNamespaces { get; set; }
        public ResolveContext CurrentContext { get; set; }
        public Resolver(Resolver parent, Namespace ns, List<Namespace> imports,ResolveContext rc, MethodSpec mtd = null)
            : this(ns,imports,rc,mtd)
        {
            Parent = parent;
     
        }
        public Resolver(Namespace ns, List<Namespace> imports,ResolveContext rc, MethodSpec mtd = null)
        {
            CurrentContext = rc;
            CurrentMethod = mtd;
            Parent = null;
            CurrentNamespace = ns;
            Imports = imports;
            KnownGlobals = new List<FieldSpec>();
            KnownMethods = new List<MethodSpec>();
            KnownTypes = new List<TypeSpec>();
            KnownLocalVars = new List<VarSpec>();
            KnownOperators = new List<OperatorSpec>();
            GloballyKnownLocals = new List<VarSpec>();
            KnownNamespaces = new List<Namespace>();
        }

        public bool KnowOperator(OperatorSpec oper)
        {
            if (!KnownOperators.Contains(oper))
            {
                KnownOperators.Add(oper);
                return true;

            }
            return false;
        }
        public bool KnowType(TypeSpec tp)
        {
            if (!KnownTypes.Contains(tp))
            {
                KnownTypes.Add(tp);
                return true;

            }
            return false;
        }
        public bool KnowNamespace(Namespace tp)
        {
            if (!KnownNamespaces.Contains(tp))
            {
                KnownNamespaces.Add(tp);
                return true;

            }
            return false;
        }
        public bool KnowField(FieldSpec f)
        {
            if (!KnownGlobals.Contains(f))
            {
                KnownGlobals.Add(f);
                return true;

            }
            return false;
        }
        public bool KnowVar(VarSpec v)
        {
            if (!KnownLocalVars.Contains(v))
            {
                KnownLocalVars.Add(v);
                GloballyKnownLocals.Add(v);
                return true;

            }
            return false;
        }
        public bool KnowGVar(VarSpec v)
        {
            if (!this.GloballyKnownLocals.Contains(v))
            {
                GloballyKnownLocals.Add(v);
                return true;

            }
            return false;
        }
        public bool KnowMethod(MethodSpec m)
        {
            if (!KnownMethods.Contains(m))
            {
                KnownMethods.Add(m);
                return true;

            }
            return false;
        }


        public bool Extend(TypeSpec tp, MethodSpec ms)
        {
            if (KnownTypes.Contains(tp))
            {
                for(int i = 0; i < KnownTypes.Count;i++)
                    if (KnownTypes[i] == tp)
                    {
                        for (int j = 0; j < KnownTypes[i].ExtendedMethods.Count; j++)
                            if (KnownTypes[i].ExtendedMethods[j].Signature.ExtensionSignature == ms.Signature.ExtensionSignature && (KnownTypes[i].ExtendedMethods[j].Modifiers & Modifiers.Prototype) == 0)
                                return false;
                            else if (KnownTypes[i].ExtendedMethods[j].Signature.ExtensionSignature == ms.Signature.ExtensionSignature)
                                return true;
                            KnownTypes[i].ExtendedMethods.Add(ms);

                    }
            }
            return true;
        }
        public bool Extend(TypeSpec tp, FieldSpec ms)
        {
            if (KnownTypes.Contains(tp))
            {
                for (int i = 0; i < KnownTypes.Count; i++)
                    if (KnownTypes[i] == tp)
                    {

                        for (int j = 0; j < KnownTypes[i].ExtendedFields.Count; j++)
                            if (KnownTypes[i].ExtendedFields[j].Signature.ExtensionSignature == ms.Signature.ExtensionSignature)
                                return false;
                     
                            KnownTypes[i].ExtendedFields.Add(ms);

                    }
            }
            return true                ;
        }
        public bool ExtendStatic(TypeSpec tp, MethodSpec ms)
        {
            if (KnownTypes.Contains(tp))
            {
                for (int i = 0; i < KnownTypes.Count; i++)
                    if (KnownTypes[i] == tp)
                    {
                        for (int j = 0; j < KnownTypes[i].StaticExtendedMethods.Count; j++)
                            if (KnownTypes[i].StaticExtendedMethods[j].Signature.ExtensionSignature == ms.Signature.ExtensionSignature)
                                return false;

                        KnownTypes[i].StaticExtendedMethods.Add(ms);

                    }
            }
            return true;
        }


        #region Resolve Members
        protected bool CheckAccessModifier(MemberSpec ms)
        {
            if (CurrentContext.IsInClass)
            {
                if ((CurrentContext.CurrentScope & ResolveScopes.SuperAccess) == ResolveScopes.SuperAccess && ((ms.Modifiers & Modifiers.Private) == Modifiers.Private)) // private member of super
                    return false;
                else if ((CurrentContext.CurrentScope & ResolveScopes.ThisAcces) == ResolveScopes.ThisAcces)
                    return true;
                else return (((ms.Modifiers & Modifiers.Private) != Modifiers.Private) || CurrentContext.CurrentNamespace == ms.NS); // otherwise everything is accepted
            }
            else
            {
                // outside class
                if (CurrentContext.CurrentExtensionLookup != null) // class member
                {
                    if ((ms.Modifiers & Modifiers.Internal) == Modifiers.Internal && ms.NS == CurrentContext.CurrentNamespace)
                        return true;
                    else if ((ms.Modifiers & Modifiers.Public) == Modifiers.Public)
                        return true;
                    else
                        return false; // private & protected restricted
                }
              
                else // general
                {
                    // public or private
                    if ((ms.Modifiers & Modifiers.Public) == Modifiers.Public)
                        return true;
                    else if (CurrentClassLookup != null && ((ms.Modifiers & Modifiers.Protected) == Modifiers.Protected || (ms.Modifiers & Modifiers.Private) == Modifiers.Private))
                        return false;
                    else return ((((ms.Modifiers & Modifiers.Private) != Modifiers.Private) && ((ms.Modifiers & Modifiers.Protected) != Modifiers.Protected)) || CurrentContext.CurrentNamespace == ms.NS); // otherwise everything is accepted

                }
            }

        }

        public MemberSpec TryResolveName(Namespace ns, string name)
        {
            MemberSpec m = ResolveVar(ns, name);
            if (m == null)
            {
                m = ResolveParameter(name);
                if (m == null)
                {
                    m = ResolveField(ns, name);
                    if (m == null)
                        return null;

                    else return m;
                }
                else return m;
            }
            else return m;
        }
        public bool ResolveType(Namespace ns, string name,ref TypeSpec tp,string sig = null)
        {
            tp = null;
            for (int i = 0; i < KnownTypes.Count; i++)
            {
                if (KnownTypes[i].NS.Name != ns.Name)
                    continue;

                if (sig == null && KnownTypes[i].Name == name)
                {
                    tp = KnownTypes[i];
                    return true;

                }
                else if (KnownTypes[i].Signature.NoNamespaceSignature == sig)
                {
                    tp = KnownTypes[i];
                    return true;

                }

            }
           
            return false;
        }
       
        public FieldSpec ResolveField(Namespace ns, string name)
        {
            if (CurrentExtensionLookup == null)
            {
                foreach (FieldSpec kt in KnownGlobals)
                {

                    if (kt.NS.Name != ns.Name)
                        continue;
                    if (kt.Name == name)
                    {
                        if (!CheckAccessModifier(kt))
                            ResolveContext.Report.Error(0, kt.Signature.Location, kt.Signature.NormalSignature + " is inaccessible due to its protection level");
                        return kt;
                    }
                }
            }
            else
            {
                TypeSpec original = KnownTypes[KnownTypes.IndexOf(CurrentExtensionLookup)];
                List<FieldSpec> ml = original.ExtendedFields;
                foreach (FieldSpec kt in ml)
                {
                    if (kt.Name == name && !(kt.IsPrivate))
                    {
                        CheckAccessModifier(kt);
                        return kt;
                    }
                }
                

            }
            return null;
        }
        public void ResolveVariadicMethod(Namespace ns, string name, ref MethodSpec mtd, TypeSpec[] par = null)
        {
            if (CurrentExtensionLookup == null)
            {
               
                    MemberSignature msig = new MemberSignature(ns, name, par, Location.Null);
                    for (int i = 0; i < KnownMethods.Count; i++)
                    {
                        if (KnownMethods[i].NS.Name != ns.Name)
                            continue;
                        if (msig.Signature.StartsWith(KnownMethods[i].Signature.Signature))
                        {
                            // Variadic Signature Match
                            mtd = KnownMethods[i];
                            if (!CheckAccessModifier(mtd))
                                ResolveContext.Report.Error(0, mtd.Signature.Location, mtd.Signature.NormalSignature + " is inaccessible due to its protection level");
                            return;
                        }
                    }
                
            }
            else
            {
                TypeSpec original = KnownTypes[KnownTypes.IndexOf(CurrentExtensionLookup)];
                List<MethodSpec> ml = original.ExtendedMethods;
                //List<MethodSpec> ml = CurrentExtensionLookup.ExtendedMethods;
                if (IsExtensionStatic)
                    ml = CurrentExtensionLookup.StaticExtendedMethods;
                if (par != null && !IsExtensionStatic)
                {
                    List<TypeSpec> ts = new List<TypeSpec>();
                    ts.AddRange(par);
                    ts.Insert(0, CurrentExtensionLookup);
                    par = ts.ToArray();
                }
                else if (!IsExtensionStatic)
                    par = new TypeSpec[1] { CurrentExtensionLookup };

                if (par != null)
                {
                    MemberSignature msig = new MemberSignature(ns, name, par, Location.Null);
                    for (int i = 0; i < ml.Count; i++)
                    {
                        if (msig.ExtensionSignature.StartsWith(ml[i].Signature.ExtensionSignature) )
                        {
                            // Variadic Signature Match
                            mtd = ml[i];
                            if (!CheckAccessModifier(mtd))
                                ResolveContext.Report.Error(0, mtd.Signature.Location, mtd.Signature.NormalSignature + " is inaccessible due to its protection level");
                            return;

                        }
                    }
                }
                else
                {
                    MemberSignature msig = new MemberSignature(ns, name, par, Location.Null);
                    for (int i = 0; i < ml.Count; i++)
                    {

                        if (msig.ExtensionSignature.StartsWith(ml[i].Signature.ExtensionSignature))
                        {
                            // Variadic Signature Match
                            mtd = ml[i];
                            if (!CheckAccessModifier(mtd))
                                ResolveContext.Report.Error(0, mtd.Signature.Location, mtd.Signature.NormalSignature + " is inaccessible due to its protection level");
                            return;

                        }
                    }
                }
            }

        }
        void ResolveExtensionMethod(Namespace ns, string name,TypeSpec ext, ref MethodSpec mtd, TypeSpec[] par = null)
        {
            bool hastemplate = false;
    
                MemberSignature msig = new MemberSignature(ns,ext.Name + "$_"+ name, par, Location.Null);
                for (int i = 0; i < KnownMethods.Count; i++)
                {

                    if (KnownMethods[i].MatchExtSignature(msig, ext.Name + "$_" + name, par, ref hastemplate))
                    {
                        mtd = KnownMethods[i];
                        if (!CheckAccessModifier(mtd))
                            ResolveContext.Report.Error(0, mtd.Signature.Location, mtd.Signature.NormalSignature + " is inaccessible due to its protection level");
                        return;
                    }
                }
       
        }
        public void ResolveMethod(Namespace ns, string name, ref MethodSpec mtd,TypeSpec[] par=null)
        {
            bool hastemplate = false;
            if (CurrentExtensionLookup == null)
            {
         
                    MemberSignature msig = new MemberSignature(ns, name, par, Location.Null);
                    for (int i = 0; i < KnownMethods.Count; i++)
                    {
                        if (KnownMethods[i].NS.Name != ns.Name)
                            continue;
                      
                        if (KnownMethods[i].MatchSignature(msig, name, par,ref hastemplate))
                        {
                            mtd = KnownMethods[i];
                            if (!CheckAccessModifier(mtd))
                                ResolveContext.Report.Error(0, mtd.Signature.Location, mtd.Signature.NormalSignature + " is inaccessible due to its protection level");
                            return;

                        }
                    }
           
            }
            else
            {
                TypeSpec original = KnownTypes[KnownTypes.IndexOf(CurrentExtensionLookup)];
       
            
                if (par != null && !IsExtensionStatic)
                {
                    List<TypeSpec> ts = new List<TypeSpec>();
                    ts.AddRange(par);
                    ts.Insert(0, CurrentExtensionLookup);
                    par = ts.ToArray();
                }
                else if(!IsExtensionStatic)
                    par = new TypeSpec[1] { CurrentExtensionLookup };

                ResolveExtensionMethod( ns, name, CurrentExtensionLookup, ref mtd, par);
                if (mtd == null && original is StructTypeSpec && (original as StructTypeSpec).Primitive != null && original.Size == (original as StructTypeSpec).Primitive.Size) // implicit cast primitive
                {
                
                    original = KnownTypes[KnownTypes.IndexOf((original as StructTypeSpec).Primitive)];
        

                    if (par != null && !IsExtensionStatic)
                    {
                        List<TypeSpec> ts = new List<TypeSpec>();
                        ts.AddRange(par);
                        ts.RemoveAt(0);
                        ts.Insert(0, original);
                        par = ts.ToArray();
                    }
                    else if (!IsExtensionStatic)
                        par = new TypeSpec[1] { original  };

                    ResolveExtensionMethod( ns, name, original, ref mtd, par);
                }
            }
         
        }

        public VarSpec ResolveVar(Namespace ns, string name)
        {
            foreach (VarSpec kt in KnownLocalVars)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Name == name)
                {
                    if (!CheckAccessModifier(kt))
                        ResolveContext.Report.Error(0, kt.Signature.Location, kt.Signature.NormalSignature + " is inaccessible due to its protection level");
                    return kt;
                }
            }
            return null;
        }
        public Namespace ResolveNS(string name)
        {
            foreach (Namespace kt in KnownNamespaces)
            {
                if (kt.Name == name)
                    return kt;
             
            }
            return Namespace.Default;
        }
        public OperatorSpec ResolveOperator(Namespace ns, string symb)
        {
            foreach (OperatorSpec kt in KnownOperators)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Symbol == symb )
                {
                    if (!CheckAccessModifier(kt))
                        ResolveContext.Report.Error(0, kt.Signature.Location, kt.Signature.NormalSignature + " is inaccessible due to its protection level");
                    return kt;
                }
            }
            return null;
        }
        public ParameterSpec ResolveParameter(string name)
        {
            if (CurrentMethod == null)
                return null;
            foreach (ParameterSpec kt in CurrentMethod.Parameters)
            {

                if (kt.Name == name)
                {
                    if (!CheckAccessModifier(kt))
                        ResolveContext.Report.Error(0, kt.Signature.Location, kt.Signature.NormalSignature + " is inaccessible due to its protection level");
                    return kt;
                }
            }
            return null;
        }
        public EnumMemberSpec ResolveEnumValue(Namespace ns, string name)
        {
            foreach (TypeSpec kt in KnownTypes)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt is EnumTypeSpec)
                {

                    EnumTypeSpec ets = (EnumTypeSpec)kt;
                    foreach (EnumMemberSpec em in ets.Members)
                        if (em.Name == name)
                        {
                            if (!CheckAccessModifier(ets))
                                ResolveContext.Report.Error(0, ets.Signature.Location, ets.Signature.NormalSignature + " is inaccessible due to its protection level");
                            return em;
                        }
                }
            }

            return null;
        }

        public void TryResolveVariadicMethod(string name, ref MethodSpec ms, TypeSpec[] param = null)
        {
            ResolveVariadicMethod(CurrentNamespace, name, ref ms, param);
            if (ms == null)
            {
                ResolveVariadicMethod(Namespace.Default, name, ref ms, param);


                if (ms == null)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ResolveVariadicMethod(ns, name, ref ms, param);
                        if (ms != null)
                            return;
                    }
                }
            }
            return;
        }
        public void TryResolveMethod(string name,ref MethodSpec ms, TypeSpec[] param = null)
        {
            ResolveMethod(CurrentNamespace, name, ref ms,param);
            if (ms == null)
            {
            ResolveMethod(Namespace.Default, name, ref ms, param);


                if (ms == null)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ResolveMethod(ns, name, ref ms, param);
                        if (ms != null)
                            return;
                    }
                }
            }
         
           if(ms == null)
            TryResolveVariadicMethod(name, ref ms, param);
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
        public OperatorSpec TryResolveOperator(string sym)
        {
            OperatorSpec ms = ResolveOperator(CurrentNamespace, sym);
            if (ms == null)
            {
                ms = ResolveOperator(Namespace.Default, sym);
                if (ms == null)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ms = ResolveOperator(ns, sym);
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
            MemberSpec m = TryResolveVar(name);
            if (m == null)
            {
                m = ResolveParameter(name);
                if (m == null)
                {
                    m = TryResolveField(name);
                    if (m == null)
                        return null;

                    else return m;
                }
                else return m;
            }
            else return m;
        }
        public bool TryResolveType(string name, ref TypeSpec type,string sig = null)
        {
            bool ok = ResolveType(CurrentNamespace, name, ref type, sig);
            if (!ok)
            {
                ok = ResolveType(Namespace.Default, name, ref type,sig);
                if (!ok)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ok = ResolveType(ns, name, ref type, sig);
                        if (ok)
                            return true;
                    }
                }
            }
            return ok;
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
        

    }
}
