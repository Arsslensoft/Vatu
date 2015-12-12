using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
{
   public abstract class IndexedValue : IEmit
    {
       public TypeSpec ValueType { get; set; }
       public VarSpec Variable { get; set; }
       public int Index { get; set; }

       public virtual bool Emit(EmitContext ec)
       {
           return true;
       }

    }
   public class IndexedArray : IndexedValue
   {
       public IndexedArray(VarSpec v, int idx)
       {
           Index = idx;
           ValueType = v.MemberType;
           Variable = v;
       }
       public override bool Emit(EmitContext ec)
       {
           return base.Emit(ec);
       }

      
   }
   public class IndexedStruct : IndexedValue
   {
       public bool PointerAccess { get; set; }
   }

}
