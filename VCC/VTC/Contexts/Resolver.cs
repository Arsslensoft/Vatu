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
       public bool IsExtensionStatic { get; set; }
       public List<Namespace> Imports { get; set; }
       public MethodSpec CurrentMethod { get; set; }

       public List<OperatorSpec> KnownOperators { get; set; }
        public List<TypeSpec> KnownTypes { get; set; }
        public List<FieldSpec> KnownGlobals { get; set; }
        public List<MethodSpec> KnownMethods { get; set; }
        public List<VarSpec> KnownLocalVars { get; set; }

        public Resolver(Resolver parent, Namespace ns, List<Namespace> imports, MethodSpec mtd = null)
            : this(ns,imports,mtd)
        {
            Parent = parent;
     
        }
        public Resolver(Namespace ns, List<Namespace> imports, MethodSpec mtd = null)
        {
            CurrentMethod = mtd;
            Parent = null;
            CurrentNamespace = ns;
            Imports = imports;
            KnownGlobals = new List<FieldSpec>();
            KnownMethods = new List<MethodSpec>();
            KnownTypes = new List<TypeSpec>();
            KnownLocalVars = new List<VarSpec>();
            KnownOperators = new List<OperatorSpec>();
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
        public bool ResolveType(Namespace ns, string name,ref TypeSpec tp)
        {
            tp = null;
            for (int i = 0; i < KnownTypes.Count; i++)
            {
                if (KnownTypes[i].NS.Name != ns.Name)
                    continue;
                if (KnownTypes[i].Name == name && ((!KnownTypes[i].IsPrivate || CurrentNamespace == ns)))
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
                    if (kt.Name == name && !(kt.IsPrivate && kt.NS != CurrentNamespace))
                        return kt;
                }
            }
            else
            {
                foreach (FieldSpec kt in CurrentExtensionLookup.ExtendedFields)
                {
                    if (kt.Name == name && !(kt.IsPrivate))
                        return kt;
                }
            }
            return null;
        }
        public MethodSpec ResolveMethod(Namespace ns, string name, TypeSpec[] par=null)
        {
            if (CurrentExtensionLookup == null)
            {
                if (par != null)
                {
                    MemberSignature msig = new MemberSignature(ns, name, par, Location.Null);
                    foreach (MethodSpec kt in KnownMethods)
                    {
                        if (kt.NS.Name != ns.Name)
                            continue;
                        if (kt.Signature == msig && ((!kt.IsPrivate || CurrentNamespace == ns)))
                            return kt;
                    }
                }
                else
                {
                    foreach (MethodSpec kt in KnownMethods)
                    {
                        if (kt.NS.Name != ns.Name)
                            continue;
                        if (kt.Name == name && ((!kt.IsPrivate || CurrentNamespace == ns)))
                            return kt;
                    }
                }
            }
            else
            {
                List<MethodSpec> ml = CurrentExtensionLookup.ExtendedMethods;
                if(IsExtensionStatic)
                    ml = CurrentExtensionLookup.StaticExtendedMethods;
                if (par != null && !IsExtensionStatic)
                {
                    List<TypeSpec> ts = new List<TypeSpec>();
                    ts.AddRange(par);
                    ts.Insert(0, CurrentExtensionLookup);
                    par = ts.ToArray();
                }
                else if(!IsExtensionStatic)
                    par = new TypeSpec[1] { CurrentExtensionLookup };

                if (par != null)
                {
                    MemberSignature msig = new MemberSignature(ns, name, par, Location.Null);
                    foreach (MethodSpec kt in ml)
                    {
                        if (kt.Signature.ExtensionSignature == msig.ExtensionSignature && ((!kt.IsPrivate || CurrentNamespace == ns)))
                            return kt;
                    }
                }
                else
                {
                    foreach (MethodSpec kt in ml)
                    {

                        if (kt.Name == name && ((!kt.IsPrivate || CurrentNamespace == ns)))
                            return kt;
                    }
                }
            }
            return null;
        }
        public VarSpec ResolveVar(Namespace ns, string name)
        {
            foreach (VarSpec kt in KnownLocalVars)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Name == name)
                    return kt;
            }
            return null;
        }
        public OperatorSpec ResolveOperator(Namespace ns, string symb)
        {
            foreach (OperatorSpec kt in KnownOperators)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt.Symbol == symb && ((!kt.IsPrivate || CurrentNamespace == ns)))
                    return kt;
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
                    return kt;
            }
            return null;
        }
        public EnumMemberSpec ResolveEnumValue(Namespace ns, string name)
        {
            foreach (TypeSpec kt in KnownTypes)
            {
                if (kt.NS.Name != ns.Name)
                    continue;
                if (kt is EnumTypeSpec &&  ((!kt.IsPrivate || CurrentNamespace == ns)))
                {

                    EnumTypeSpec ets = (EnumTypeSpec)kt;
                    foreach (EnumMemberSpec em in ets.Members)
                        if (em.Name == name)
                            return em;
                }
            }

            return null;
        }


        public MethodSpec TryResolveMethod(string name, TypeSpec[] param = null)
        {
            MethodSpec ms = ResolveMethod(CurrentNamespace, name, param);
            if (ms == null)
            {
                ms = ResolveMethod(Namespace.Default, name, param);


                if (ms == null)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ms = ResolveMethod(ns, name, param);
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
        public bool TryResolveType(string name, ref TypeSpec type)
        {
            bool ok = ResolveType(CurrentNamespace, name,ref type);
            if (!ok)
            {
                ok = ResolveType(Namespace.Default, name, ref type);
                if (!ok)
                {
                    foreach (Namespace ns in Imports)
                    {
                        ok = ResolveType(ns, name, ref type);
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
