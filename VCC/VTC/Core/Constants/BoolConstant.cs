using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class BoolConstant : ConstantExpression
    {
        internal bool _value;
        public BoolConstant(bool value, Location loc)
            : base(BuiltinTypeSpec.Bool, loc)
        {
            _value = value;
        }
        public override string CommentString()
        {
            return _value.ToString();
        }

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
            EmitToStack(ec);
       
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
            return this;
        }

        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = _value ? (ushort)EmitContext.TRUE : (ushort)0, Size = 16 });
            return true;
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            if (_value == v)
                ec.EmitInstruction(new Jump() { DestinationLabel = truecase.Name });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = ec.GetLow(rg), SourceValue = _value ? (ushort)EmitContext.TRUE : (ushort)0, Size = 16 });
            return true;
        }
    }
    
	
	
	
}