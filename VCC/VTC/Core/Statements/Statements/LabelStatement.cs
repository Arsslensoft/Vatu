using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class LabelStatement : BaseStatement
    {
        public Label Label { get; set; }

        Identifier _label;
        [Rule(@"<Statement> ::= Id ~':'")]
        public LabelStatement(Identifier id)
        {
            _label = id;
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
            Label = ec.DefineLabel(_label.Name);
            ec.MarkLabel(Label);
            return true;
        }
       
    }
    
	
}