using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Member Signature [Types, member, variable]
    /// </summary>
    public struct MemberSignature : IEquatable<MemberSignature>
    {
     
        string _signature;
        public string Signature { get { return _signature; } }
        string _nsig;
        public string NormalSignature { get { return _nsig; } }
        Location _loc;
        public Location Location { get { return _loc; } }
        string _extsig;
        public string ExtensionSignature { get { return _extsig; } }

        string _nns;
        public string NoNamespaceSignature { get { return _nns; } }

        string _nnts;
        public string NoNamespaceTypeSignature { get { return _nnts; } }


        string types;
        public string TypesSignature { get { return types; } }
        public MemberSignature(Namespace ns, string name,TypeSpec[] param, Location loc)
        {
            _nsig = name;
            _signature = name;
            _extsig = name;
            _nns = name;
            _nnts = name;
            if (!ns.IsDefault)
            {
                _signature = ns.Normalize() + "_" + _signature;
                _nsig = ns.Normalize() + "." + _nsig;
            }
             types = "";
            
            if (param != null)
            {
                if (param.Length > 0)
                {
                    _nsig += "(";
                    types += "<";
                }
                foreach (TypeSpec p in param)
                {
                    if (p == null)
                    {

                    }

                    _extsig += "_" + p.GetTypeName(p).Replace("*", "P").Replace("&", "R").Replace("<", "$T_").Replace(">", "_T$");
                    _signature += "_" + p.GetTypeName(p).Replace("*", "P").Replace("&", "R").Replace("<", "$T_").Replace(">", "_T$");
                    _nsig +=  p.GetTypeName(p) + ",";
                    _nns += "_" + p.GetTypeName(p).Replace("*", "P").Replace("&", "R");
                    types += p.GetTypeName(p) + ",";
                }
                if (param.Length > 0)
                {
                    types = types.Remove(types.Length - 1, 1) + ">";
                    _nsig = _nsig.Remove(_nsig.Length - 1, 1) + ")";
                }
                _nnts += types;
            }
      
            _loc = loc;

        }
        public MemberSignature(Namespace ns, string name, Location loc)
        {
            _nsig = name;
            _signature = name;
            _extsig = name;
            _nns = name;
            _nnts = name;
            types = "";
            if (!ns.IsDefault)
            {
                _signature = ns.Normalize() + "_" + _signature;
                _nsig = ns.Normalize() + "." + _nsig;

            }
            _loc = loc;

        }
        public static bool operator !=(MemberSignature a, MemberSignature b)
        {
            return a.Signature != b.Signature;
        }
        public static bool operator ==(MemberSignature a, MemberSignature b)
        {
            return a.Signature == b.Signature;
        }

        public bool Equals(MemberSignature ns)
        {
            return ns.Signature == Signature;
        }
        public override bool Equals(object obj)
        {
            if (obj is MemberSignature)
                return Signature == ((MemberSignature)obj).Signature;
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return Signature;
        }

  
    }

	
	
}