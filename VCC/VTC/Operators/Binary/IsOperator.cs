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
	
	[Terminal("is")]
    public class IsOperator : BinaryOp
    {
        public IsOperator()
        {
            Operator = BinaryOperator.Equality;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.BX;
        }
        bool isval = false;
        bool IsExtendedByTypeDef(TypeSpec tp,TypeSpec deriv)
        {
            return (tp.GetTypeDefBase(tp).Equals(deriv.GetTypeDefBase(deriv)));
        }
        bool IsInherited(TypeSpec deriv, TypeSpec inherited)
        {
            if (deriv is StructTypeSpec)
            {
                foreach (TypeSpec t in (deriv as StructTypeSpec).Inherited)
                {
                    if (t.Equals(inherited))
                        return true;
                    else if (IsInherited(t, inherited))
                        return true;

                }


                
            }
            return false;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            isval = IsInherited(Left.Type, RightType.Type) || IsExtendedByTypeDef(Left.Type,RightType.Type);

            CommonType = BuiltinTypeSpec.Bool;

            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment(Left.CommentString() + " is " + RightType.Name);
            ec.EmitPush(isval);
            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase,bool v)
        {
          
            ec.EmitComment(Left.CommentString() + " is " + RightType.Name);
            if (isval == v)
                ec.EmitInstruction(new Jump() { DestinationLabel = truecase.Name });

            return true;
        }
    }
   
}