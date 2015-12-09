using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
{
    [Terminal("Id")]
    public class Identifier : Expr
    {
        protected readonly string _idName;
        public string Name { get { return _idName; } }

        public Identifier(string idName)
        {
            _idName = idName;
        }
        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }

    }

    public class TypeIdentifier : TypeToken
    {
        BaseTypeIdentifier _base;
        TypePointer _pointers;
        [Rule(@"<Type>     ::= <Base> <Pointers>")]
        public TypeIdentifier(BaseTypeIdentifier tbase, TypePointer pointers)
        {
            _base = tbase;
            _pointers = pointers;
        }

 
        public override bool Resolve(ResolveContext rc)
        {
            _base.Resolve(rc);
            return base.Resolve(rc);
        }

    }
    public class MethodIdentifier : Identifier
    {
        public Identifier Id { get; set; }
        public TypeToken Type { get; set; }

        [Rule(@"<Func ID> ::= <Type> Id")]
        public MethodIdentifier(TypeToken type, Identifier id)
            : base(id.Name)
        {
            Id = id;
            Type = type;
        }

        [Rule(@"<Func ID> ::= Id")]
        public MethodIdentifier(Identifier id)
            : base(id.Name)
        {
            Id = id;
            Type = null;
        }

        public override bool Emit(EmitContext ec)
        {
            return base.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return base.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return base.EmitToStack(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }
}
