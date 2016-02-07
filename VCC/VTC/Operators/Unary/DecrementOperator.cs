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
	[Terminal("--")]
    public class DecrementOperator : UnaryOp
    {
        public DecrementOperator()
        {
            FloatingPointSupported = true;
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.PrefixDecrement;
        }
        AssignExpression ae;
       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type.Equals( BuiltinTypeSpec.Bool))
                ResolveContext.Report.Error(25, Location, "Unary operator must be used with non boolean, pointer types");
          
            UnaryCheck(rc);
            ae = new AssignExpression(Right, new SimpleAssignOperator(), Right);
            ae = (AssignExpression)ae.DoResolve(rc);
            CommonType = Right.Type;
     
            rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + Operator.ToString(), ref OvlrdOp, new TypeSpec[1] { Right.Type });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            return this;
        }
         bool EmitFloatOperation(EmitContext ec)
 {
  
     Right.EmitToStack(ec);
     ec.EmitInstruction(new Vasm.x86.X86.x87.FloatPushOne());
     ec.EmitComment(" -- " + Right.CommentString());
     ec.EmitInstruction(new Vasm.x86.x87.FloatSubAndPop() { DestinationReg = RegistersEnum.ST1, SourceReg = RegistersEnum.ST0 });
    
     Right.EmitFromStack(ec);
     

     return true;
 }
         bool EmitFloatOperationToStack(EmitContext ec)
         {

             Right.EmitToStack(ec);
             if (Operator == UnaryOperator.PostfixDecrement)
                 ec.EmitInstruction(new Vasm.x86.x87.FloatLoad() { DestinationReg = RegistersEnum.ST0 });

             ec.EmitInstruction(new Vasm.x86.X86.x87.FloatPushOne());
             ec.EmitComment(" -- " + Right.CommentString());
             ec.EmitInstruction(new Vasm.x86.x87.FloatSubAndPop() { DestinationReg = RegistersEnum.ST1, SourceReg = RegistersEnum.ST0 });
        
             if (Operator == UnaryOperator.PrefixDecrement)
                 ec.EmitInstruction(new Vasm.x86.x87.FloatLoad() { DestinationReg = RegistersEnum.ST0 }); // second push 
           
             Right.EmitFromStack(ec);


             return true;
         }

        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                base.EmitOverrideOperator(ec);
                
                return Right.EmitFromStack(ec);
            }


            if (Right.Type.IsFloat && !Right.Type.IsPointer)
                return EmitFloatOperation(ec);



            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "-- ");
            ec.EmitPop(Register.Value);
   
            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Sub() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
                ec.EmitInstruction(new Dec() { DestinationReg = Register.Value, Size = 80 });
            EmitCheckOvf(ec, Register.Value, CommonType.IsSigned);
            ec.EmitPush(Register.Value);


            ae.EmitFromStack(ec);
         //   ec.EmitPush(ec.FirstRegister());
        


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {

            if (OvlrdOp != null)
            {
                base.EmitOverrideOperator(ec);

                return Right.EmitFromStack(ec);
            }


            if (Right.Type.IsFloat && !Right.Type.IsPointer)
                return EmitFloatOperationToStack(ec);

            


            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "-- ");
            ec.EmitPop(Register.Value);

            if (Operator == UnaryOperator.PostfixDecrement)
                ec.EmitPush(Register.Value); 


            if (Right.Type.IsPointer)
                ec.EmitInstruction(new Sub() { DestinationReg = Register.Value, SourceValue = (ushort)Right.Type.BaseType.Size });
            else
                ec.EmitInstruction(new Dec() { DestinationReg = Register.Value, Size = 80 });
            EmitCheckOvf(ec, Register.Value, CommonType.IsSigned);
            ec.EmitPush(Register.Value);
         
            if (Operator == UnaryOperator.PrefixDecrement)
                 ec.EmitPush(Register.Value);

            ae.EmitFromStack(ec);
            //   ec.EmitPush(ec.FirstRegister());



            return true;

        }
    }
    
	
}