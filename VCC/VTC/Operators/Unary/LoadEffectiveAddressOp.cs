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
	public class LoadEffectiveAddressOp : UnaryOp
    {
        MemberSpec ms;
        public LoadEffectiveAddressOp()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.AddressOf;
        }
        TypeSpec MemberType;
       public override bool Resolve(ResolveContext rc)
        {
           return Right.Resolve(rc) ;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
       
                // LEA
            //if (Right is AccessExpression)
            //{
            //    ResolveContext.Report.Error(53, Right.Location, "Address of operator cannot be used with non variable expressions");
            //    ms = null;
            //}
            //else
         if (!(Right is VariableExpression))
              //  ms = (Right as VariableExpression).variable;
              //else
                ResolveContext.Report.Error(54, Location, "Address Of Operator does not support non variable nor access expressions");
                CommonType = Right.Type.MakePointer();
                return this;
         
        }
  
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return Right.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            //if (ms != null)
            //{
            //    if (ms is VarSpec)
            //        ms.LoadEffectiveAddress(ec);
            //    else if (ms is FieldSpec)
            //        ms.LoadEffectiveAddress(ec);
            //    else if (ms is ParameterSpec)
            //        ms.LoadEffectiveAddress(ec);
            //}
            if(Right is AccessExpression)
                return (Right as AccessExpression).LoadEffectiveAddress(ec);
            else
                return (Right as VariableExpression).LoadEffectiveAddress(ec);
        
        }
        public override bool EmitToStack(EmitContext ec)
        {
          return Emit(ec);
 
        }
        public override string CommentString()
        {
            return  "&";
        }
    }
    
	
}