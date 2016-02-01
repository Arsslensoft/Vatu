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
	

    public class NegationOperator : UnaryOp
    {
        public NegationOperator()
        {
            FloatingPointSupported = true;
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.UnaryNegation;
        }

       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if ((!Right.Type.IsSigned && !Right.Type.IsFloat)  || Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "Unary negate must be used signed & float types");
       
     CommonType = Right.Type;
     UnaryCheck(rc);
        
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
 
        bool EmitFloatOperation(EmitContext ec)
 {

     Right.EmitToStack(ec);
     ec.EmitComment(" - " + Right.CommentString());
     ec.EmitInstruction(new Vasm.x86.x87.FloatNegate());
     

     return true;
 }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);

            if (Right.Type.IsFloat && !Right.Type.IsPointer)
                return EmitFloatOperation(ec);


            Right.EmitToStack(ec);
            ec.EmitComment("-" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Neg() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
         


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }

    }

}