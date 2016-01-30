using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class AsmStatement : BaseStatement
    {
        public List<string> Instructions { get; set; }
        AsmInstructions _stmt;
        [Rule(@"<ASM Statement>        ::= ~asm ~'{' <INSTRUCTIONS>  ~'}'")]
        public AsmStatement(AsmInstructions stmt)
        {
            Instructions = new List<string>();

            _stmt = stmt;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            AsmInstructions st = _stmt;
            while (st != null)
            {
                AsmInstruction ins = st.ins;
                if (ins != null)
                    Instructions.Add(ins.Value);
                st = st.nxt;
            }
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            foreach (string ins in Instructions)
                ec.EmitInstruction(new InlineInstruction(ins));
            return true;
        }
    
    }
    
	
}