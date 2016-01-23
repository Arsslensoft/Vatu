using bsn.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VTC.Core
{
    public class RegisterIdentifier : Identifier
    {
        public RegisterToken Id { get; set; }
        public RegistersEnum Register { get; set; }
        public bool Is16Bits { get; set; }

        [Rule(@"<REGISTER ID> ::= AX")]
        [Rule(@"<REGISTER ID> ::= BX")]
        [Rule(@"<REGISTER ID> ::= CX")]
        [Rule(@"<REGISTER ID> ::= DX")]
        [Rule(@"<REGISTER ID> ::= SP")]
        [Rule(@"<REGISTER ID> ::= BP")]
        [Rule(@"<REGISTER ID> ::= SI")]
        [Rule(@"<REGISTER ID> ::= DI")]

        [Rule(@"<REGISTER ID> ::= CS")]
        [Rule(@"<REGISTER ID> ::= DS")]
        [Rule(@"<REGISTER ID> ::= ES")]
        [Rule(@"<REGISTER ID> ::= FS")]
        [Rule(@"<REGISTER ID> ::= GS")]
        [Rule(@"<REGISTER ID> ::= SS")]

        [Rule(@"<REGISTER ID> ::= AH")]
        [Rule(@"<REGISTER ID> ::= BH")]
        [Rule(@"<REGISTER ID> ::= CH")]
        [Rule(@"<REGISTER ID> ::= DH")]
        [Rule(@"<REGISTER ID> ::= AL")]
        [Rule(@"<REGISTER ID> ::= BL")]
        [Rule(@"<REGISTER ID> ::= CL")]
        [Rule(@"<REGISTER ID> ::= DL")]
        public RegisterIdentifier(RegisterToken id)
            : base(id.Name)
        {
            Is16Bits = true;

        }




        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Register = Registers.GetRegister(Name).Value;
            Is16Bits = Registers.Is16Bit(Register);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {


            return true;
        }
    }

    public class AsmInstruction : SimpleToken
    {
        public string Value { get; set; }

        [Rule(@"<INSTRUCTION> ::= StringLiteral ~';'")]
        public AsmInstruction(StringLiteral opc)
        {
            loc = CompilerContext.TranslateLocation(position);
            Value = opc.Value.GetValue().ToString();
        }
  
  




    }
    public class AsmInstructions : SimpleToken
    {
      internal  AsmInstruction ins;
      internal AsmInstructions nxt;

        [Rule(@"<INSTRUCTIONS> ::= <INSTRUCTION> <INSTRUCTIONS>")]
        public AsmInstructions(AsmInstruction a, AsmInstructions ains)
        {
            loc = CompilerContext.TranslateLocation(position);
            nxt = ains;
            ins = a;
        }
        [Rule(@"<INSTRUCTIONS>  ::=  <INSTRUCTION> ")]
        public AsmInstructions(AsmInstruction a)
        {
            loc = CompilerContext.TranslateLocation(position);
            ins = a;
            nxt = null;
         
        }






    }


    [Terminal("AX")]
    [Terminal("BX")]
    [Terminal("CX")]
    [Terminal("DX")]
    [Terminal("BP")]
    [Terminal("SP")]
    [Terminal("SI")]
    [Terminal("DI")]
    [Terminal("AH")]
    [Terminal("AL")]
    [Terminal("BH")]
    [Terminal("BL")]
    [Terminal("CH")]
    [Terminal("CL")]
    [Terminal("DH")]
    [Terminal("DL")]

    [Terminal("CS")]
    [Terminal("DS")]
    [Terminal("ES")]
    [Terminal("FS")]
    [Terminal("GS")]
    [Terminal("SS")]
    public class RegisterToken : SimpleToken
    {
    }

  
}
