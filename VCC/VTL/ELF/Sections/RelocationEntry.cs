using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ELFSharp.ELF.Sections
{
    public class RelocationEntry<T> : IRelocationEntry<T> where T : struct
    {
       public T Offset { get; private set; }
       public T Info { get; private set; }
       public T AddEnd { get; private set; }
       public RelocationType RelocationType { get { return GetRelocationType(Info); }  }
       public PRelocationType RelocationKind { get {
           dynamic inf = Info;
           dynamic sh = 0xFF;
           return (Sections.PRelocationType)(inf & sh); 
       } }

       public RelocationEntry(T off, T info, T addend)
       {
           Offset = off;
           Info = info;
           AddEnd = addend;
       }

       public T Sym(T inf)
       {
           dynamic i = inf;
           return i >> 8;
       }
       RelocationType GetRelocationType(T inf)
       {
           dynamic i = inf;
           return ( RelocationType)(i >> 8);
       }
      
   public  T GetSymbolTableIndex(T tSymAdr,T origin, uint adrend)
       {
        
       
           dynamic SymAdr = tSymAdr;
           dynamic offset = origin ;

           switch (RelocationKind)
           {
               case PRelocationType.R_P16:
                   return (SymAdr + adrend);
               case PRelocationType.R_PC16:
                   return (SymAdr + adrend - offset);
               case PRelocationType.R_P32:
                   return (SymAdr + adrend);
               case PRelocationType.R_PC32:
                   return (SymAdr + adrend - offset);

               default:
                   return Offset;

           }
       }
    }
}
