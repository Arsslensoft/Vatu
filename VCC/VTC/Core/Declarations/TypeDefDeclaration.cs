﻿using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class TypeDefDeclaration : Declaration
    {

        public TypeSpec TypeName
        {
            get;
            set;

        }
        Modifier _mod;
        TypeIdentifier _typedef;
        [Rule(@"<Typedef Decl> ::= <Mod> ~typedef <Type> Id ~';'")]
        public TypeDefDeclaration(Modifier mod, TypeIdentifier type, Identifier id)
        {
            _mod = mod;
            _name = id;
            _typedef = type;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            _typedef = (TypeIdentifier)_typedef.DoResolve(rc);
            TypeName = new TypeSpec(rc.CurrentNamespace, _name.Name, _typedef.Type.Size, BuiltinTypes.Unknown, TypeFlags.TypeDef, Modifiers.NoModifier, loc, _typedef.Type);
            TypeName.Modifiers = _mod.ModifierList;
            rc.KnowType(TypeName);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {


            return _typedef.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }

    }
}