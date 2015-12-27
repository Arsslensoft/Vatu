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

        [Rule(@"<REGISTER> ::= AX")]
        [Rule(@"<REGISTER> ::= BX")]
        [Rule(@"<REGISTER> ::= CX")]
        [Rule(@"<REGISTER> ::= DX")]
        [Rule(@"<REGISTER> ::= SP")]
        [Rule(@"<REGISTER> ::= BP")]
        [Rule(@"<REGISTER> ::= SI")]
        [Rule(@"<REGISTER> ::= DI")]

        [Rule(@"<REGISTER> ::= CS")]
        [Rule(@"<REGISTER> ::= DS")]
        [Rule(@"<REGISTER> ::= ES")]
        [Rule(@"<REGISTER> ::= FS")]
        [Rule(@"<REGISTER> ::= GS")]
        [Rule(@"<REGISTER> ::= SS")]

        [Rule(@"<REGISTER> ::= AH")]
        [Rule(@"<REGISTER> ::= BH")]
        [Rule(@"<REGISTER> ::= CH")]
        [Rule(@"<REGISTER> ::= DH")]
        [Rule(@"<REGISTER> ::= AL")]
        [Rule(@"<REGISTER> ::= BL")]
        [Rule(@"<REGISTER> ::= CL")]
        [Rule(@"<REGISTER> ::= DL")]
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
        [Rule(@"<INSTRUCTIONS>   ::= ")]
        public AsmInstructions()
        {
            loc = CompilerContext.TranslateLocation(position);
            ins = null;
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
