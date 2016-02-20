using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{

    public class MethodIdentifier : Identifier
    {
        public Identifier Id { get; set; }
        public TypeToken TType { get; set; }
        public Modifiers Mods { get; set; }
        internal CallingCV CCV { get; set; }
        public CallingConventions CV
        {
            get
            {
                if (CCV != null)
                    return CCV.CallingConvention;
                else return CallingConventions.Default;
            }
        }
        public Specifiers Specs { get; set; }
        Modifier _mod;
        FunctionSpecifierSequence _fsp;
        [Rule(@"<Func ID> ::= <CallCV> <Type> Id")]
        public MethodIdentifier(CallingCV ccv, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = ccv;
        }
        [Rule(@"<Func ID> ::= <Type> Id")]
        public MethodIdentifier(TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = null;
        }

        [Rule(@"<Func ID> ::= <Mod> <CallCV> <Type> Id")]
        public MethodIdentifier(Modifier mod, CallingCV ccv, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = ccv;
            _mod = mod;
        }
        [Rule(@"<Func ID> ::= <Mod> <Type> Id")]
        public MethodIdentifier(Modifier mod, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = null;
            _mod = mod;
        }

        // Function Specs
        [Rule(@"<Func ID> ::= <Func Specs> <CallCV> <Type> Id")]
        public MethodIdentifier(FunctionSpecifierSequence fs, CallingCV ccv, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = ccv;
            _fsp = fs;
        }
        [Rule(@"<Func ID> ::= <Func Specs> <Type> Id")]
        public MethodIdentifier(FunctionSpecifierSequence fs, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = null;
            _fsp = fs;
        }

        [Rule(@"<Func ID> ::= <Mod> <Func Specs> <CallCV> <Type> Id")]
        public MethodIdentifier(Modifier mod, FunctionSpecifierSequence fs, CallingCV ccv, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = ccv;
            _mod = mod;
            _fsp = fs;
        }
        [Rule(@"<Func ID> ::= <Mod> <Func Specs> <Type> Id")]
        public MethodIdentifier(Modifier mod, FunctionSpecifierSequence fs, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = null;
            _mod = mod;
            _fsp = fs;
        }

        public override bool Resolve(ResolveContext rc)
        {
            TType.Resolve(rc);

            return base.Resolve(rc);
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_mod != null)
            {
                _mod.AllowSealedModifier = rc.IsInClass;
                _mod = (Modifier)_mod.DoResolve(rc);
                Mods = _mod.ModifierList;
            }
            else Mods = Modifiers.Private;

            if (_fsp != null)
            {
                _fsp = (FunctionSpecifierSequence)_fsp.DoResolve(rc);
                Specs = _fsp.FunctionSpecs;
            }
            else Specs = Specifiers.NoSpec;
            TType = (TypeToken)TType.DoResolve(rc);
            base.Type = TType.Type;
            if (CCV != null)
                CCV = (CallingCV)CCV.DoResolve(rc);

            return this;
        }

    }



}