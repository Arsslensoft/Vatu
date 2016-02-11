using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class EnumDeclaration : Declaration
    {
        public EnumTypeSpec TypeName { get; set; }
        public int Size { get; set; }
        public bool IsFlags = false;
        Modifier _mod;
        EnumDefinition _def;
        [Rule(@"<Enum Decl>    ::= <Mod> ~enum Id ~'{' <Enum Def> ~'}'  ~';'")]
        public EnumDeclaration(Modifier mod, Identifier id, EnumDefinition edef)
        {
            _mod = mod;
            _name = id;
            _def = edef;

        }
        [Rule(@"<Enum Decl>    ::= <Mod> ~typedef ~enum ~'{' <Enum Def> ~'}' Id ~';'")]
        public EnumDeclaration(Modifier mod, EnumDefinition edef, Identifier id)
        {
            _mod = mod;
            _name = id;
            _def = edef;

        }

        [Rule(@"<Enum Decl>    ::= <Mod> setof ~enum Id ~'{' <Enum Def> ~'}'  ~';'")]
        public EnumDeclaration(Modifier mod, SimpleToken t, Identifier id, EnumDefinition edef)
        {
            IsFlags = true;
            _mod = mod;
            _name = id;
            _def = edef;

        }
        [Rule(@"<Enum Decl>    ::= <Mod> ~typedef setof ~enum ~'{' <Enum Def> ~'}' Id ~';'")]
        public EnumDeclaration(Modifier mod, SimpleToken t, EnumDefinition edef, Identifier id)
        {
            IsFlags = true;
            _mod = mod;
            _name = id;
            _def = edef;

        }
        void UpdateTypes(List<EnumMemberSpec> mem,TypeSpec tp)
        {
            foreach (EnumMemberSpec em in _def.Members)
                em.memberType = tp;
        }
        void GetValues(List<ushort> UsedValues, List<EnumMemberSpec> mem)
        {
            // Get Values
            foreach (EnumMemberSpec em in _def.Members)
                if (em.IsAssigned)
                {
                    if (UsedValues.Contains(em.Value))
                        ResolveContext.Report.Error(10, Location, "Each enum member must have a unique value");
                    UsedValues.Add(em.Value);
                }

            // Auto-Assign

            foreach (EnumMemberSpec em in _def.Members)
            {
                if (!em.IsAssigned)
                {
                    for (ushort v = 0; v < ushort.MaxValue; v++)
                    {
                        if (!UsedValues.Contains(v))
                        {
                            em.Value = v;
                            UsedValues.Add(v);
                            break;
                        }
                    }
                }

                mem.Add(em);
            }
        }
        void GetValuesFlags(List<ushort> UsedValues, List<EnumMemberSpec> mem)
        {
            ushort f = 1;
            // generate flags
            List<ushort> allowed = new List<ushort>();

            for (int i = 0; i < 16; i++)
            {
                allowed.Add(f);
                f = (ushort)(f << 1);
            }

            // Get Values
            foreach (EnumMemberSpec em in _def.Members)
                if (em.IsAssigned)
                {
                    if (UsedValues.Contains(em.Value))
                        ResolveContext.Report.Error(10, Location, "Each enum member must have a unique value");
                    else if (!allowed.Contains(em.Value))
                        ResolveContext.Report.Error(10, Location, "Each setof member must have a power of 2 value");

                    UsedValues.Add(em.Value);
                }

            // Auto-Assign

            foreach (EnumMemberSpec em in _def.Members)
            {
                if (!em.IsAssigned)
                {
                    foreach (ushort v in allowed)
                    {
                        if (!UsedValues.Contains(v))
                        {
                            em.Value = v;
                            UsedValues.Add(v);
                            break;
                        }
                    }
                }

                mem.Add(em);
            }
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);

            List<ushort> UsedValues = new List<ushort>();
            List<EnumMemberSpec> mem = new List<EnumMemberSpec>();
            _def = (EnumDefinition)_def.DoResolve(rc);

            if (_def != null)
                Size = _def.Size;
            if (IsFlags && _def.Members.Count > 8)
                Size = 2;
            else if (IsFlags && _def.Members.Count > 16)
                ResolveContext.Report.Error(10, Location, "Flags based enum cannot hold more than 16 values");
            if (IsFlags)
                GetValuesFlags(UsedValues, mem);
            else
                GetValues(UsedValues, mem);
            UpdateTypes(mem, Size == 2 ? BuiltinTypeSpec.UInt : BuiltinTypeSpec.Byte);
            TypeName = new EnumTypeSpec(rc.CurrentNamespace, _name.Name, Size, mem, Location);
            TypeName.IsFlags = IsFlags;
            TypeName.Modifiers = _mod.ModifierList;
            if (TypeName.Members.Count >= 65536)
                ResolveContext.Report.Error(11, Location, "Max enum values exceeded, only 65536 values are allowed");
            rc.KnowType(TypeName);
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
           
            return base.DoFlowAnalysis(fc);
        }
    }

}
