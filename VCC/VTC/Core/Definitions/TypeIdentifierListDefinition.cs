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

            _id = (TypeIdentifier)_id.DoResolve(rc);
            if (_nextid != null)
                _nextid = (TypeIdentifierListDefinition)_nextid.DoResolve(rc);
            return this;
        }
     
    
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }

}