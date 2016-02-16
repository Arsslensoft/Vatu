using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
   public class ReferenceExpression : VariableExpression
    {
       public ReferenceExpression(MemberSpec ms)
           : base(ms)
       {
           Type = ms.memberType;
       }
       public override bool Emit(EmitContext ec)
       {
           return EmitToStack(ec);
       }
       public override bool EmitToStack(EmitContext ec)
       {
           StructEmitter st = null;
        
           if (variable is VarSpec)
               st = new StructEmitter(variable, (variable as VarSpec).VariableStackIndex, ReferenceKind.LocalVariable);
           else if (variable is ParameterSpec)
               st = new StructEmitter(variable, (variable as ParameterSpec).StackIdx, ReferenceKind.Parameter);
           else if (variable is FieldSpec)
               st = new StructEmitter(variable, (variable as FieldSpec).FieldOffset, ReferenceKind.Field);
      
           st.EmitToStack(ec);

           return true;
       }
    }
}
