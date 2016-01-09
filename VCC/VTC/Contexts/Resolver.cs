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
                        for(int j = 0; j < KnownTypes[i].ExtendedMethods.Count; j++)
                             if (KnownTypes[i].ExtendedMethods[j].Signature.ExtensionSignature == ms.Signature.ExtensionSignature)
                                return false;
                     
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
        public TypeSpec ResolveType(Namespace ns, string name)
        {
            foreach (TypeSpec kt in KnownTypes)
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
                        if (kt.Signature == msig)
                            return kt;
                    }
                }
                else
                {
                    foreach (MethodSpec kt in KnownMethods)
                    {
                        if (kt.NS.Name != ns.Name)
                            continue;
                        if (kt.Name == name)
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
                        if (kt.Signature.ExtensionSignature == msig.ExtensionSignature)
                            return kt;
                    }
                }
                else
                {
                    foreach (MethodSpec kt in ml)
                    {
                  
                        if (kt.Name == name)
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
        

    }
}
