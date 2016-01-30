using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	public class EnumValue : Definition
    {
        public Identifier _id;
        private Literal _value;
        public ConstantExpression Value;
        [Rule(@"<Enum Val> ::= Id ~'=' <Integral Const>")]
        public EnumValue(Identifier id, Literal value)
        {
            _id = id;
            _value = value;
        }
    

        [Rule(@"<Enum Val>     ::= Id")]
        public EnumValue(Identifier id)
        {
            _id = id;
            _value = null;
        }


       public override bool Resolve(ResolveContext rc)
        {

            if (_value != null)

                return _value.Resolve(rc);
            else return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _id = (Identifier)_id.DoResolve(rc);
            if (_value != null)
                Value = (ConstantExpression)_value.DoResolve(rc);
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            // TODO:EMIT ENUM
            return true;
        }
    }
   
}