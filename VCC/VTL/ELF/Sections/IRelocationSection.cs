using System.Collections.Generic;
namespace ELFSharp.ELF.Sections
{
    public interface IRelocationSection<T> : ISection
    {
        List<IRelocationEntry<T>> Entries { get; }
    }
}

