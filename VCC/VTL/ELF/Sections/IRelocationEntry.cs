using System;
namespace ELFSharp.ELF.Sections
{
    public interface IRelocationEntry<T>
    {
        T Offset
        {
            get;
        }
        T Info { get; }
        T AddEnd { get; }
        RelocationType RelocationType { get; }
        PRelocationType RelocationKind { get; }
        T Sym(T i);


        T GetSymbolTableIndex(T SymAdr, T offset, uint adrend);
    }
}

