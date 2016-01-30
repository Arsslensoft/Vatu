using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ParameterListDefinition : Definition
    {
        public ParameterDefinition _id;
        public ParameterListDefinition _nextid;
        [Rule(@"<Params>     ::= <Param> ~',' <Params>")]
        public ParameterListDefinition(ParameterDefinition ptr, ParameterListDefinition var)
        {
            _id = ptr;
            _nextid = var;
        }

        [Rule(@"<Params>     ::= <Param>")]
        public ParameterListDefinition(ParameterDefinition id)
            : this(id, null)
        {

        }

       public override bool Resolve(ResolveContext rc)
        {
         bool ok =   _id.Resolve(rc);
            if(_nextid != null)
          ok &=  _nextid.Resolve(rc);
            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
          
            _id = (ParameterDefinition)_id.DoResolve(rc);
            if (_nextid != null)
            _nextid = (ParameterListDefinition)_nextid.DoResolve(rc);
            return this;
        }

        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }

	
}