using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class StringConstant : ConstantExpression
    {
      

        public static int id = 0;
        public FieldSpec ConstVar { get; set; }
        string _value;
        public StringConstant(string value, Location loc)
            : base(BuiltinTypeSpec.String, loc)
        {
            _value = value;
           
        }
     
        bool decl = false;
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ConstVar != null)
            {
                ec.EmitData(new DataMember(ConstVar.Signature.ToString(), _value,true), ConstVar, true);
                ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(ConstVar.Signature.ToString()), Size = 16 });

            }
            return true;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
    
                ConstVar = new FieldSpec(rc.CurrentNamespace, "STRC_" + id,  Modifiers.Const, Type, loc);
          
            id++;
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (!decl)
                Emit(ec);
      
     
            return true;
        }
     
        public override string CommentString()
        {
            return _value.ToString();
        }
    }	
}