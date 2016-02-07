using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	public class TypeIdentifierListDefinition : Definition
    {
        public List<TypeSpec> Types { get; set; }
       public TypeIdentifier _id;
       public TypeIdentifierListDefinition _nextid;
        [Rule(@"<Types>      ::= <Type>  ~',' <Types>")]
        public TypeIdentifierListDefinition(TypeIdentifier ptr, TypeIdentifierListDefinition var)
        {
            _id = ptr;
            _nextid = var;
        }

        [Rule(@"<Types>      ::= <Type>")]
        public TypeIdentifierListDefinition(TypeIdentifier id)
            : this(id, null)
        {

        }

       public override bool Resolve(ResolveContext rc)
        {
            bool ok=            _id.Resolve(rc);
            if (_nextid != null)
           ok &= _nextid.Resolve(rc);
            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            Types = new List<TypeSpec>();

            _id = (TypeIdentifier)_id.DoResolve(rc);
            Types.Add(_id.Type);
            if (_nextid != null)
            {
                _nextid = (TypeIdentifierListDefinition)_nextid.DoResolve(rc);
                Types.AddRange(_nextid.Types);
            }
            return this;
        }
     
    
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }

}