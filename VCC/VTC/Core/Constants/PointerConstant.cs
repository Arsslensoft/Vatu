using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	public class PointerConstant : UIntConstant
    {
       
        public PointerConstant(ushort value, Location loc)
            : base(value,loc)
        {
            Type = BuiltinTypeSpec.Pointer;
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
            ec.EmitInstruction(new Push() { DestinationValue = (ushort)_value, Size = 16 });
            return true;
        }

        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)_value, Size = 16 });
            return true;
        }
    }
    
	
	
}