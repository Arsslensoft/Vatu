﻿using System;
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
        List<Namespace> childs;
        public List<Namespace> ChildNamespaces { get { return childs; } }

        string _name;
        bool _def;
        public string Name { get { return _name; } }
        public static Namespace Default = new Namespace("global");
        public bool IsDefault { get { return Name == "global"; } }
        public bool IsDefaultUse { get { return _def; } set { _def = value; } }
        public bool HasParent
        {
            get
            {
                if (_parent == null)
                    return false;
                return (Namespace)_parent != Namespace.Default;
            }
        }
        object _parent;
        public Namespace ParentNameSpace
        {
            get
            {

                return (Namespace)_parent;

            }
            set { _parent = value; }
        }
        public Namespace GetParent(Resolver r)
        {

            if (r.KnownNamespaces.Contains(ParentNameSpace))
                return r.KnownNamespaces[r.KnownNamespaces.IndexOf(ParentNameSpace)];
            else return ParentNameSpace;
        }
        Location l;
        public Location Location
        {
            get { return l; }
        }
        public Namespace(string name, bool def = false)
            : this(name, Location.Null, Namespace.Default, def)
        {

        }
        public Namespace(string name, Location loc, bool def = false)
            : this(name, loc, Namespace.Default, def)
        {



        }
        public Namespace(string name, Location loc, Namespace par, bool def = false)
        {
            _parent = par;
            childs = new List<Namespace>();
            _name = name;
            if (string.IsNullOrEmpty(name))
                _name = "global";

            if (loc == null)
                l = Location.Null;
            else
                l = loc;

            _def = def;
        }


        public string Normalize()
        {
            if (string.IsNullOrEmpty(_name))
                _name = "global";

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