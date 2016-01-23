using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	 public class VariableListDefinition : Definition
    {
        public bool IsAbstract = false;
        public VariableItemDefinition _vardef;
        public VariableListDefinition _nextvars;
        [Rule(@"<Var List> ::=  ~',' <Var Item> <Var List>")]
        public VariableListDefinition(VariableItemDefinition ptr, VariableListDefinition var)
        {
            _vardef = ptr;
            _nextvars = var;
        }

        [Rule(@"<Var List> ::=  ")]
        public VariableListDefinition()
        {

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_nextvars != null)
            {
                _nextvars.IsAbstract = IsAbstract;
                _nextvars = (VariableListDefinition)_nextvars.DoResolve(rc);
              
                _vardef.IsAbstract = IsAbstract;
                _vardef = (VariableItemDefinition)_vardef.DoResolve(rc);
            
                return this;
            }
            else return null;
       
        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;
      
            if (_vardef != null)
                ok &= _vardef.Resolve(rc);
            if (_nextvars != null)
               ok &= _nextvars.Resolve(rc);
            return ok;
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    
}