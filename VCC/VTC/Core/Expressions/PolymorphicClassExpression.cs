using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Base.GoldParser.Parser;

namespace VTC.Core
{
    public class PolymorphicClassExpression : VariableExpression
    {
        public int InheritanceIdx { get; set; }
        public TypeSpec Destination { get; set; }
       public PolymorphicClassExpression(MemberSpec ms, int index, TypeSpec dest, LineInfo pos)
           : base(ms)
        {
            position = pos;
            InheritanceIdx = index;
            Destination = dest;
       }

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           
            base.DoResolve(rc);
            Type = Destination;
            return this;
       }

  
       public override bool Resolve(ResolveContext rc)
       {
           variable = rc.Resolver.TryResolveVar("super");
           return base.Resolve(rc);
       }
       public override bool Emit(EmitContext ec)
       {

           base.EmitToStack(ec);
           if (InheritanceIdx > 0)
           {
               ec.EmitPop(Vasm.x86.RegistersEnum.AX);
               ec.EmitInstruction(new Add() { SourceValue = (ushort)InheritanceIdx, DestinationReg = RegistersEnum.AX });
               ec.EmitPush(RegistersEnum.AX);
           }
           return true;
       }
       public override bool EmitToStack(EmitContext ec)
       {
           return Emit(ec);
       }
       public override bool EmitBranchable(EmitContext ec, Vasm.Label truecase, bool v)
       {
           return base.EmitBranchable(ec, truecase, v);
       }
    }
}
