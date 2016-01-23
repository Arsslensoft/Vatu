using bsn.GoldParser.Semantic;
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
        private CallingCV CCV { get; set; }
        public CallingConventions CV
        {
            get
            {
                if (CCV != null)
                    return CCV.CallingConvention;
                else return CallingConventions.Default;
            }
        }
        Modifier _mod;
        [Rule(@"<Func ID> ::= <CallCV> <Type> Id")]
        public MethodIdentifier(CallingCV ccv, TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = ccv;
        }
        [Rule(@"<Func ID> ::= <Type> Id")]
        public MethodIdentifier( TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            TType = type;
            CCV = null;
        }

        [Rule(@"<Func ID> ::= <Mod> <CallCV> <Type> Id")]
        public MethodIdentifier(Modifier mod,CallingCV ccv, TypeToken type, Identifier id)
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_mod != null)
            {
                _mod = (Modifier)_mod.DoResolve(rc);
                Mods = _mod.ModifierList;
            }
            else Mods = Modifiers.Private;
            TType = (TypeToken)TType.DoResolve(rc);
            base.Type = TType.Type;
            if (CCV != null)
                CCV = (CallingCV)CCV.DoResolve(rc);
         
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            TType.Resolve(rc);

            return base.Resolve(rc);
        }
    }

	
	
}