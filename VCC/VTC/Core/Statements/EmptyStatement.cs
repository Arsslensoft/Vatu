using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class EmptyStatement : NormalStatment
    {



        [Rule("<Normal Stm> ::= ~';'")]
        public EmptyStatement()
        {


        }

       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
             ec.EmitInstruction(new Vasm.x86.Noop());
             return true;
        }
         
      
    }
   
	
}