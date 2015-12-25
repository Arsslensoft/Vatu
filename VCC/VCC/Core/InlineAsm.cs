using bsn.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;

namespace VCC.Core
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
    public class AsmOpcode : Identifier
    {
        public OpCodeToken Id { get; set; }

        [Rule(@"<OPCODES> ::= mov")]
        [Rule(@"<OPCODES> ::= cmp")]
        [Rule(@"<OPCODES> ::= test")]
        [Rule(@"<OPCODES> ::= push")]
        [Rule(@"<OPCODES> ::= pop")]
        [Rule(@"<OPCODES> ::= idiv")]
        [Rule(@"<OPCODES> ::= inc")]
        [Rule(@"<OPCODES> ::= dec")]
        [Rule(@"<OPCODES> ::= neg")]
        [Rule(@"<OPCODES> ::= mul")]
        [Rule(@"<OPCODES> ::= div")]
        [Rule(@"<OPCODES> ::= imul")]
        [Rule(@"<OPCODES> ::= not")]
        [Rule(@"<OPCODES> ::= setpo")]
        [Rule(@"<OPCODES> ::= setae")]
        [Rule(@"<OPCODES> ::= setnle")]
        [Rule(@"<OPCODES> ::= setc")]
        [Rule(@"<OPCODES> ::= setno")]
        [Rule(@"<OPCODES> ::= setnb")]
        [Rule(@"<OPCODES> ::= setp")]
        [Rule(@"<OPCODES> ::= setnge")]
        [Rule(@"<OPCODES> ::= setl")]
        [Rule(@"<OPCODES> ::= setge")]
        [Rule(@"<OPCODES> ::= setpe")]
        [Rule(@"<OPCODES> ::= setnl")]
        [Rule(@"<OPCODES> ::= setnz")]
        [Rule(@"<OPCODES> ::= setne")]
        [Rule(@"<OPCODES> ::= setnc")]
        [Rule(@"<OPCODES> ::= setbe")]
        [Rule(@"<OPCODES> ::= setnp")]
        [Rule(@"<OPCODES> ::= setns")]
        [Rule(@"<OPCODES> ::= seto")]
        [Rule(@"<OPCODES> ::= setna")]
        [Rule(@"<OPCODES> ::= setnae")]
        [Rule(@"<OPCODES> ::= setz")]
        [Rule(@"<OPCODES> ::= setle")]
        [Rule(@"<OPCODES> ::= setnbe")]
        [Rule(@"<OPCODES> ::= sets")]
        [Rule(@"<OPCODES> ::= sete")]
        [Rule(@"<OPCODES> ::= setb")]
        [Rule(@"<OPCODES> ::= seta")]
        [Rule(@"<OPCODES> ::= setg")]
        [Rule(@"<OPCODES> ::= setng")]
        [Rule(@"<OPCODES> ::= xchg")]
        [Rule(@"<OPCODES> ::= popad")]
        [Rule(@"<OPCODES> ::= aaa")]
        [Rule(@"<OPCODES> ::= popa")]
        [Rule(@"<OPCODES> ::= popfd")]
        [Rule(@"<OPCODES> ::= cwd")]
        [Rule(@"<OPCODES> ::= lahf")]
        [Rule(@"<OPCODES> ::= pushad")]
        [Rule(@"<OPCODES> ::= pushf")]
        [Rule(@"<OPCODES> ::= aas")]
        [Rule(@"<OPCODES> ::= bswap")]
        [Rule(@"<OPCODES> ::= pushfd")]
        [Rule(@"<OPCODES> ::= cbw")]
        [Rule(@"<OPCODES> ::= cwde")]
        [Rule(@"<OPCODES> ::= xlat")]
        [Rule(@"<OPCODES> ::= aam")]
        [Rule(@"<OPCODES> ::= aad")]
        [Rule(@"<OPCODES> ::= cdq")]
        [Rule(@"<OPCODES> ::= daa")]
        [Rule(@"<OPCODES> ::= sahf")]
        [Rule(@"<OPCODES> ::= das")]
        [Rule(@"<OPCODES> ::= into")]
        [Rule(@"<OPCODES> ::= iret")]
        [Rule(@"<OPCODES> ::= clc")]
        [Rule(@"<OPCODES> ::= stc")]
        [Rule(@"<OPCODES> ::= cmc")]
        [Rule(@"<OPCODES> ::= cld")]
        [Rule(@"<OPCODES> ::= std")]
        [Rule(@"<OPCODES> ::= cli")]
        [Rule(@"<OPCODES> ::= sti")]
        [Rule(@"<OPCODES> ::= movsb")]
        [Rule(@"<OPCODES> ::= movsw")]
        [Rule(@"<OPCODES> ::= movsd")]
        [Rule(@"<OPCODES> ::= lods")]
        [Rule(@"<OPCODES> ::= lodsb")]
        [Rule(@"<OPCODES> ::= lodsw")]
        [Rule(@"<OPCODES> ::= lodsd")]
        [Rule(@"<OPCODES> ::= stos")]
        [Rule(@"<OPCODES> ::= stosb")]
        [Rule(@"<OPCODES> ::= stosw")]
        [Rule(@"<OPCODES> ::= sotsd")]
        [Rule(@"<OPCODES> ::= scas")]
        [Rule(@"<OPCODES> ::= scasb")]
        [Rule(@"<OPCODES> ::= scasw")]
        [Rule(@"<OPCODES> ::= scasd")]
        [Rule(@"<OPCODES> ::= cmps")]
        [Rule(@"<OPCODES> ::= cmpsb")]
        [Rule(@"<OPCODES> ::= cmpsw")]
        [Rule(@"<OPCODES> ::= cmpsd")]
        [Rule(@"<OPCODES> ::= insb")]
        [Rule(@"<OPCODES> ::= insw")]
        [Rule(@"<OPCODES> ::= insd")]
        [Rule(@"<OPCODES> ::= outsb")]
        [Rule(@"<OPCODES> ::= outsw")]
        [Rule(@"<OPCODES> ::= outsd")]
        [Rule(@"<OPCODES> ::= adc")]
        [Rule(@"<OPCODES> ::= add")]
        [Rule(@"<OPCODES> ::= sub")]
        [Rule(@"<OPCODES> ::= cbb")]
        [Rule(@"<OPCODES> ::= xor")]
        [Rule(@"<OPCODES> ::= or")]
        [Rule(@"<OPCODES> ::= jnbe")]
        [Rule(@"<OPCODES> ::= jnz")]
        [Rule(@"<OPCODES> ::= jpo")]
        [Rule(@"<OPCODES> ::= jz")]
        [Rule(@"<OPCODES> ::= js")]
        [Rule(@"<OPCODES> ::= loopnz")]
        [Rule(@"<OPCODES> ::= jge")]
        [Rule(@"<OPCODES> ::= jb")]
        [Rule(@"<OPCODES> ::= jnb")]
        [Rule(@"<OPCODES> ::= jo")]
        [Rule(@"<OPCODES> ::= jp")]
        [Rule(@"<OPCODES> ::= jno")]
        [Rule(@"<OPCODES> ::= jnl")]
        [Rule(@"<OPCODES> ::= jnae")]
        [Rule(@"<OPCODES> ::= loopz")]
        [Rule(@"<OPCODES> ::= jmp")]
        [Rule(@"<OPCODES> ::= jnp")]
        [Rule(@"<OPCODES> ::= loop")]
        [Rule(@"<OPCODES> ::= jl")]
        [Rule(@"<OPCODES> ::= jcxz")]
        [Rule(@"<OPCODES> ::= jae")]
        [Rule(@"<OPCODES> ::= jnge")]
        [Rule(@"<OPCODES> ::= ja")]
        [Rule(@"<OPCODES> ::= loopne")]
        [Rule(@"<OPCODES> ::= loope")]
        [Rule(@"<OPCODES> ::= jg")]
        [Rule(@"<OPCODES> ::= jnle")]
        [Rule(@"<OPCODES> ::= je")]
        [Rule(@"<OPCODES> ::= jnc")]
        [Rule(@"<OPCODES> ::= jc")]
        [Rule(@"<OPCODES> ::= jna")]
        [Rule(@"<OPCODES> ::= jbe")]
        [Rule(@"<OPCODES> ::= jle")]
        [Rule(@"<OPCODES> ::= jpe")]
        [Rule(@"<OPCODES> ::= jns")]
        [Rule(@"<OPCODES> ::= jecxz")]
        [Rule(@"<OPCODES> ::= jng")]
        [Rule(@"<OPCODES> ::= movzx")]
        [Rule(@"<OPCODES> ::= bsf")]
        [Rule(@"<OPCODES> ::= bsr")]
        [Rule(@"<OPCODES> ::= les")]
        [Rule(@"<OPCODES> ::= lea")]
        [Rule(@"<OPCODES> ::= lds")]
        [Rule(@"<OPCODES> ::= ins")]
        [Rule(@"<OPCODES> ::= outs")]
        [Rule(@"<OPCODES> ::= xadd")]
        [Rule(@"<OPCODES> ::= cmpxchg")]
        [Rule(@"<OPCODES> ::= shl")]
        [Rule(@"<OPCODES> ::= shr")]
        [Rule(@"<OPCODES> ::= ror")]
        [Rule(@"<OPCODES> ::= rol")]
        [Rule(@"<OPCODES> ::= rcl")]
        [Rule(@"<OPCODES> ::= sal")]
        [Rule(@"<OPCODES> ::= rcr")]
        [Rule(@"<OPCODES> ::= sar")]
        [Rule(@"<OPCODES> ::= btr")]
        [Rule(@"<OPCODES> ::= bt")]
        [Rule(@"<OPCODES> ::= btc")]
        [Rule(@"<OPCODES> ::= call")]
        [Rule(@"<OPCODES> ::= inter")]
        [Rule(@"<OPCODES> ::= retn")]
        [Rule(@"<OPCODES> ::= ret")]
        [Rule(@"<OPCODES> ::= retf")]
        [Rule(@"<OPCODES> ::= byt")]
        [Rule(@"<OPCODES> ::= sbyt")]
        [Rule(@"<OPCODES> ::= db")]
        [Rule(@"<OPCODES> ::= word")]
        [Rule(@"<OPCODES> ::= sword")]
        [Rule(@"<OPCODES> ::= dw")]
        [Rule(@"<OPCODES> ::= dword")]
        [Rule(@"<OPCODES> ::= sdword")]
        [Rule(@"<OPCODES> ::= dd")]
        [Rule(@"<OPCODES> ::= fword")]
        [Rule(@"<OPCODES> ::= df")]
        [Rule(@"<OPCODES> ::= qword")]
        [Rule(@"<OPCODES> ::= dq")]
        [Rule(@"<OPCODES> ::= tbyte")]
        [Rule(@"<OPCODES> ::= dt")]
        [Rule(@"<OPCODES> ::= real4")]
        [Rule(@"<OPCODES> ::= real8")]
        [Rule(@"<OPCODES> ::= real")]
        [Rule(@"<OPCODES> ::= far")]
        [Rule(@"<OPCODES> ::= near")]
        [Rule(@"<OPCODES> ::= proc")]
        public AsmOpcode(OpCodeToken id)
            : base(id.Name)
        {
           

        }




        public override SimpleToken DoResolve(ResolveContext rc)
        {

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {


            return true;
        }
    }
    public class AsmOperand : SimpleToken
    {
        public string Value { get; set; }

        [Rule(@"<OPERAND>    ::= <REGISTER>")]
        public AsmOperand(RegisterIdentifier rg)
        {
            loc = CompilerContext.TranslateLocation(position);
            Value = rg.Name;
        }
        [Rule(@"<OPERAND>    ::= DecLiteral")]
        public AsmOperand(DecLiteral dcl)
        {
            loc = CompilerContext.TranslateLocation(position);
            Value = dcl.Value.GetValue().ToString();
        }
        [Rule(@"<OPERAND>    ::= HexLiteral")]
        public AsmOperand(HexLiteral hl)
        {
            loc = CompilerContext.TranslateLocation(position);
            Value = hl.Value.GetValue().ToString();
        }
        [Rule(@"<OPERAND>    ::= StringLiteral")]
        public AsmOperand(StringLiteral hl)
        {
            loc = CompilerContext.TranslateLocation(position);
            Value = hl.Value.GetValue().ToString();
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
        [Rule(@"<INSTRUCTION> ::= <OPCODES> ~';'")]
        public AsmInstruction(AsmOpcode opc)
        {
            loc = CompilerContext.TranslateLocation(position);
            Value = opc.Name;
        }
        [Rule(@"<INSTRUCTION>    ::= <OPCODES> <OPERANDS> ~';'")]
        public AsmInstruction(AsmOpcode opc,AsmOperands opr)
        {
            loc = CompilerContext.TranslateLocation(position);
            Value = "";
            AsmOperands ap = opr;
            while (ap != null)
            {
                AsmOperand aop = ap.ins;
                if (aop != null && !string.IsNullOrEmpty(aop.Value))
                    Value += ", " + aop.Value;
                ap = ap.nxt;
            }
          
            if (Value.Length > 0)
                Value = opc.Name + "\t" + Value.Remove(0, 2);
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
    public class AsmOperands : SimpleToken
    {
       internal AsmOperand ins;
       internal AsmOperands nxt;

        [Rule(@"<OPERANDS>   ::= <OPERAND> ~',' <OPERANDS>")]
        public AsmOperands(AsmOperand a, AsmOperands ains)
        {
            loc = CompilerContext.TranslateLocation(position);
            nxt = ains;
            ins = a;
        }
        [Rule(@"<OPERANDS>   ::= <OPERAND>")]
        public AsmOperands(AsmOperand a)
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

    [Terminal("mov")]
    [Terminal("cmp")]
    [Terminal("test")]
    [Terminal("push")]
    [Terminal("pop")]
    [Terminal("idiv")]
    [Terminal("inc")]
    [Terminal("dec")]
    [Terminal("neg")]
    [Terminal("mul")]
    [Terminal("div")]
    [Terminal("imul")]
    [Terminal("not")]
    [Terminal("setpo")]
    [Terminal("setae")]
    [Terminal("setnle")]
    [Terminal("setc")]
    [Terminal("setno")]
    [Terminal("setnb")]
    [Terminal("setp")]
    [Terminal("setnge")]
    [Terminal("setl")]
    [Terminal("setge")]
    [Terminal("setpe")]
    [Terminal("setnl")]
    [Terminal("setnz")]
    [Terminal("setne")]
    [Terminal("setnc")]
    [Terminal("setbe")]
    [Terminal("setnp")]
    [Terminal("setns")]
    [Terminal("seto")]
    [Terminal("setna")]
    [Terminal("setnae")]
    [Terminal("setz")]
    [Terminal("setle")]
    [Terminal("setnbe")]
    [Terminal("sets")]
    [Terminal("sete")]
    [Terminal("setb")]
    [Terminal("seta")]
    [Terminal("setg")]
    [Terminal("setng")]
    [Terminal("xchg")]
    [Terminal("popad")]
    [Terminal("aaa")]
    [Terminal("popa")]
    [Terminal("popfd")]
    [Terminal("cwd")]
    [Terminal("lahf")]
    [Terminal("pushad")]
    [Terminal("pushf")]
    [Terminal("aas")]
    [Terminal("bswap")]
    [Terminal("pushfd")]
    [Terminal("cbw")]
    [Terminal("cwde")]
    [Terminal("xlat")]
    [Terminal("aam")]
    [Terminal("aad")]
    [Terminal("cdq")]
    [Terminal("daa")]
    [Terminal("sahf")]
    [Terminal("das")]
    [Terminal("into")]
    [Terminal("iret")]
    [Terminal("clc")]
    [Terminal("stc")]
    [Terminal("cmc")]
    [Terminal("cld")]
    [Terminal("std")]
    [Terminal("cli")]
    [Terminal("sti")]
    [Terminal("movsb")]
    [Terminal("movsw")]
    [Terminal("movsd")]
    [Terminal("lods")]
    [Terminal("lodsb")]
    [Terminal("lodsw")]
    [Terminal("lodsd")]
    [Terminal("stos")]
    [Terminal("stosb")]
    [Terminal("stosw")]
    [Terminal("sotsd")]
    [Terminal("scas")]
    [Terminal("scasb")]
    [Terminal("scasw")]
    [Terminal("scasd")]
    [Terminal("cmps")]
    [Terminal("cmpsb")]
    [Terminal("cmpsw")]
    [Terminal("cmpsd")]
    [Terminal("insb")]
    [Terminal("insw")]
    [Terminal("insd")]
    [Terminal("outsb")]
    [Terminal("outsw")]
    [Terminal("outsd")]
    [Terminal("adc")]
    [Terminal("add")]
    [Terminal("sub")]
    [Terminal("cbb")]
    [Terminal("xor")]
    [Terminal("or")]
    [Terminal("jnbe")]
    [Terminal("jnz")]
    [Terminal("jpo")]
    [Terminal("jz")]
    [Terminal("js")]
    [Terminal("loopnz")]
    [Terminal("jge")]
    [Terminal("jb")]
    [Terminal("jnb")]
    [Terminal("jo")]
    [Terminal("jp")]
    [Terminal("jno")]
    [Terminal("jnl")]
    [Terminal("jnae")]
    [Terminal("loopz")]
    [Terminal("jmp")]
    [Terminal("jnp")]
    [Terminal("loop")]
    [Terminal("jl")]
    [Terminal("jcxz")]
    [Terminal("jae")]
    [Terminal("jnge")]
    [Terminal("ja")]
    [Terminal("loopne")]
    [Terminal("loope")]
    [Terminal("jg")]
    [Terminal("jnle")]
    [Terminal("je")]
    [Terminal("jnc")]
    [Terminal("jc")]
    [Terminal("jna")]
    [Terminal("jbe")]
    [Terminal("jle")]
    [Terminal("jpe")]
    [Terminal("jns")]
    [Terminal("jecxz")]
    [Terminal("jng")]
    [Terminal("movzx")]
    [Terminal("bsf")]
    [Terminal("bsr")]
    [Terminal("les")]
    [Terminal("lea")]
    [Terminal("lds")]
    [Terminal("ins")]
    [Terminal("outs")]
    [Terminal("xadd")]
    [Terminal("cmpxchg")]
    [Terminal("shl")]
    [Terminal("shr")]
    [Terminal("ror")]
    [Terminal("rol")]
    [Terminal("rcl")]
    [Terminal("sal")]
    [Terminal("rcr")]
    [Terminal("sar")]
    [Terminal("btr")]
    [Terminal("bt")]
    [Terminal("btc")]
    [Terminal("call")]
    [Terminal("inter")]
    [Terminal("retn")]
    [Terminal("ret")]
    [Terminal("retf")]
    [Terminal("byt")]
    [Terminal("sbyt")]
    [Terminal("db")]
    [Terminal("word")]
    [Terminal("sword")]
    [Terminal("dw")]
    [Terminal("dword")]
    [Terminal("sdword")]
    [Terminal("dd")]
    [Terminal("fword")]
    [Terminal("df")]
    [Terminal("qword")]
    [Terminal("dq")]
    [Terminal("tbyte")]
    [Terminal("dt")]
    [Terminal("real4")]
    [Terminal("real8")]
    [Terminal("real")]
    [Terminal("far")]
    [Terminal("near")]
    [Terminal("proc")]
    public class OpCodeToken : SimpleToken
    {
    }
}
