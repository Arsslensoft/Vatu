using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
	public class ValueOfOp : UnaryOp
    {
        MemberSpec ms;
        public ValueOfOp()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ValueOf;
        }
        TypeSpec MemberType;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!Right.Type.IsPointer)
                ResolveContext.Report.Error(53, Location, "Value of operator cannot be used with non pointer types");
            // VOF
            if (Right is AccessExpression)
            {
                //ResolveContext.Report.Error(53, Location, "Value of operator cannot be used with non variable expressions");
                ms = null;
            }
            else if (Right is VariableExpression)
                ms = (Right as VariableExpression).variable;
            MemberType = Right.Type;
            Right.Type = Right.Type.BaseType;
            if (Right.Type != null)
                CommonType = Right.Type;
            
            
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return Right.Resolve(rc) ;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ms != null)
            {
                if (ms is VarSpec)
                    ms.ValueOf(ec);
                else if (ms is FieldSpec)
                    ms.ValueOf(ec);
                else if (ms is ParameterSpec)
                    ms.ValueOf(ec);
            }
            else
            {
             
              
                ec.EmitComment("ValueOf @Var");
                Right.EmitToStack(ec);
                ec.EmitPop(EmitContext.SI);
                if  (MemberType.BaseType.Size <= 2)
                    ec.EmitPush(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
                else
                {

                    ec.EmitComment("Push ValueOf Var [TypeOf " + MemberType.Name + "] @Var");

                    PushAllFromRegister(ec, EmitContext.SI, MemberType.BaseType.Size, 0);
                }
            }
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
           
            return Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (ms != null)
            {
                if (ms is VarSpec)
                    ms.ValueOfStack(ec);
                else if (ms is FieldSpec)
                    ms.ValueOfStack(ec);
                else if (ms is ParameterSpec)
                    ms.ValueOfStack(ec);
            }
            else
            {
                

                 Right.Emit(ec);
                ec.EmitPop(EmitContext.SI); // pop @var 
                ec.EmitComment("ValueOf Stack @Var");

               
                 if (MemberType.BaseType.Size <= 2)
                    ec.EmitPop(EmitContext.SI, MemberType.BaseType.SizeInBits, true);
                else
                    PopAllToRegister(ec, EmitContext.SI, MemberType.BaseType.Size, 0);
         

            }
            return true;
        }
        public override string CommentString()
        {
            return "*" ;
        }

        public bool PushAllFromRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
        {
            int s = size / 2;

            if (size % 2 != 0)
            {
                ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.DL, SourceReg = rg, SourceDisplacement = offset - 1 + size, SourceIsIndirect = true, Size = 8 });
                ec.EmitPush(RegistersEnum.DX);
            }
            for (int i = s - 1; i >= 0; i--)
                ec.EmitInstruction(new Push() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });

            return true;
        }
        public bool PopAllToRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
        {

            int s = size / 2;


            for (int i = 0; i < s; i++)
                ec.EmitInstruction(new Pop() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });
            if (size % 2 != 0)
            {
                ec.EmitPop(RegistersEnum.DX);
                ec.EmitInstruction(new Mov() { DestinationReg = rg, DestinationDisplacement = offset - 1 + size, DestinationIsIndirect = true, Size = 8, SourceReg = RegistersEnum.DL });

            }
            return true;
        }
    }
    
	
}