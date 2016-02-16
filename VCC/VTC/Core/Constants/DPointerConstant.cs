using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	public class DPointerConstant : ConstantExpression
    {
        uint _value;
        public DPointerConstant(uint value, Location loc)
            : base(BuiltinTypeSpec.DPointer, loc)
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
            ushort low = (ushort)_value;
            ushort high = (ushort)(_value >> 16);
            ec.EmitInstruction(new Push() { DestinationValue = low, Size = 16 });
            ec.EmitInstruction(new Push() { DestinationValue = high, Size = 16 });
            return true;
        }

        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
 
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)_value, Size = 16 });
            return true;
        }
    }
    
	
	
}