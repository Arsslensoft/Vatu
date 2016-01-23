using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VTC.Core
{
    public class AsmDeclaration : Declaration
    {
        public List<string> Instructions { get; set; }
        public bool IsDefault;
        AsmStatement _stmt;
        [Rule(@"<ASM Decl>        ::= extern <ASM Statement>")]
        [Rule(@"<ASM Decl>        ::= default <ASM Statement>")]
        public AsmDeclaration(SimpleToken tok, AsmStatement stmt)
        {
            Instructions = new List<string>();
            IsDefault = tok.Name == "default";
            _stmt = stmt;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _stmt = (AsmStatement)_stmt.DoResolve(rc);
            Instructions.AddRange(_stmt.Instructions);
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (!IsDefault)
            {
                foreach (string ins in Instructions)
                    ec.EmitInstruction(new InlineInstruction(ins));
            }
            else
            {
                foreach (string ins in Instructions)
                    ec.ag.AddDefault(new InlineInstruction(ins));
            }
            return true;
        }
    }
}
