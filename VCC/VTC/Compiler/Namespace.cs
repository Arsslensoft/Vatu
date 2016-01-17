using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public struct Namespace : IEquatable<Namespace>
    {
        public static bool operator !=(Namespace a, Namespace b)
        {
            return a.Name != b.Name;
        }
        public static bool operator ==(Namespace a, Namespace b)
        {
            return a.Name == b.Name;
        }
        string _name;
        public string Name { get { return _name; } }
        public static Namespace Default = new Namespace("global");
        public bool IsDefault { get { return Name == "global"; } }
        Location l;
        public Location Location
        {
            get { return l; }
        }
        public Namespace(string name)
        {
            _name = name;
            l = Location.Null;
        }
        public Namespace(string name,Location loc)
        {
            _name = name;
            if(loc == null)
                l = Location.Null;
            else
            l = loc;
        }
        public string Normalize()
        {
            return Name.Replace("::", "_");
        }
        public override string ToString()
        {
            return "[Namespace: " + Name + "]";
        }
        public bool Equals(Namespace ns)
        {
            return ns.Name == Name;
        }
        public override bool Equals(object obj)
        {
            if (obj is Namespace)
                return Name == ((Namespace)obj).Name;
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
