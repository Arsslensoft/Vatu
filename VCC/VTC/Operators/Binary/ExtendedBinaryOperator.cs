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
	public class ExtendedBinaryOperator : BinaryOp
    {
        public string SymbolName { get; set; }
 
        public ExtendedBinaryOperator(bsn.GoldParser.Grammar.Symbol l,string name)
        {
            SymbolName = name;
            symbol = l;
            Operator = BinaryOperator.UserDefine;
            LeftRegister = RegistersEnum.AX;
            RightRegister = RegistersEnum.CX;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
          

            CommonType = Left.Type;
            if (Right is RegisterExpression && Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Registers cannot be used with this kind of operators");
            else if (Right is RegisterExpression || Left is RegisterExpression)
                ResolveContext.Report.Error(28, Location, "Register expected, Left and Right must be registers");

            OperatorSpec oper = rc.Resolver.TryResolveOperator(SymbolName);
            if (oper == null)
                ResolveContext.Report.Error(0, Location, "Unknown operator");
            else
            {
                if (oper.IsLogic)
                    CommonType = BuiltinTypeSpec.Bool;

                 rc.Resolver.TryResolveMethod(CommonType.NormalizedName + "_" + oper.Name, ref OvlrdOp, new TypeSpec[2] { Left.Type, Right.Type });
                if (rc.CurrentMethod == OvlrdOp)
                    OvlrdOp = null;
                else if(OvlrdOp == null)
                    ResolveContext.Report.Error(0, Location, "No operator overloading for this operator");
            }
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            else return false;
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperator(ec);
            else return false;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorBranchable(ec, truecase,v, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            else return false;
        }

    }

	
}