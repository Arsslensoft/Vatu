using VTC.Base.GoldParser.Parser;
using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
	 [Terminal("??")]
    public class ZeroTestOperator : UnaryOp
    {
        public ZeroTestOperator()
         {
             FloatingPointSupported = true;
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ZeroTest;
        }

       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
          
            CommonType = BuiltinTypeSpec.Bool;
            UnaryCheck(rc);

       
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }


 bool EmitFloatOperationBranchable(EmitContext ec, Label truecase, bool v)
 {
     Right.EmitToStack(ec);
     ec.EmitComment(" ?? " + Right.CommentString());
     ec.EmitInstruction(new Vasm.x86.x87.FloatTest());
     ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() { DestinationReg = EmitContext.A });
     ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
     ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags());


     // jumps
     ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);

     return true;
 }
 bool EmitFloatOperation(EmitContext ec)
 {

     Right.EmitToStack(ec);
     ec.EmitComment(" ?? " + Right.CommentString());
     ec.EmitInstruction(new Vasm.x86.x87.FloatTest());
     ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() { DestinationReg = EmitContext.A });
     ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
     ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags());


     ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
     ec.EmitPush(Register.Value);


     return true;
 }


 public override bool Emit(EmitContext ec)
 {
     if (OvlrdOp != null)
         base.EmitOverrideOperator(ec);

 

     if (Right.Type.IsFloat && !Right.Type.IsPointer)
         return EmitFloatOperation(ec);


            Right.EmitToStack(ec);
            ec.EmitComment("??" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = 0, Size = 80 });
            ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
            ec.EmitPush(Register.Value);
       


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase, v, ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
            if (Right.Type.IsFloat && !Right.Type.IsPointer)
                return EmitFloatOperationBranchable(ec,truecase,v);
         
            Right.EmitToStack(ec);
            ec.EmitComment("??" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = 0, Size = 80 });
            // jumps
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
            return true;
        }

    }
    
	
}