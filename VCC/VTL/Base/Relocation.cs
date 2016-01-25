using ELFSharp.ELF.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VTL
{
    public class Relocation<T> where T : struct
    {
        public T Value { get; private set; }
        public Link<T> RelocationLink { get; private set; }
        public RelocationEntry<T> ReLocation { get; private set; }
        public ObjectFile<T> RelocationFile { get; private set; }
        public SymbolEntry<T> RelocationSymbol { get; private set; }
        /// <summary>
        /// Global relocations
        /// </summary>
        /// <param name="lnk"></param>
        /// <param name="reloc"></param>
        public Relocation(Link<T> lnk, RelocationEntry<T> reloc)
        {
            RelocationLink = lnk;
            ReLocation = reloc;
        }


        public Relocation(ObjectFile<T> lnk, RelocationEntry<T> reloc, SymbolEntry<T> relocsym)
        {
            RelocationFile = lnk;
            ReLocation = reloc;
            RelocationSymbol = relocsym;
        }
        public T CalculateAddress(T ofs, T symadr)
        {
            //dynamic shifted = ReLocation.Sym(ReLocation.Info);
            dynamic org = ofs;
            //SymbolEntry<T> symval = RelocationLink.Source;
            dynamic offset = ReLocation.Offset;
            RelocationLink.TargetLocation.CodeReader.BaseStream.Seek((int)offset, SeekOrigin.Begin);
            uint value = (uint)RelocationLink.TargetLocation.CodeReader.ReadUInt16();
            dynamic v = value;
            Value = v;
            dynamic NewSymbol = ReLocation.GetSymbolTableIndex(symadr, org, (uint)value);
            return NewSymbol;
        }
        public T LocalCalculateAddress(T ofs, T symadr)
        {
            dynamic org = ofs;
            //  dynamic shifted = ReLocation.Sym(ReLocation.Info);

            //SymbolEntry<T> symval = RelocationSymbol;
            dynamic offset = ReLocation.Offset;
            RelocationFile.CodeReader.BaseStream.Seek((int)offset, SeekOrigin.Begin);
            uint value = (uint)RelocationFile.CodeReader.ReadUInt16();
            dynamic v = value;
            Value = v;
            dynamic NewSymbol = ReLocation.GetSymbolTableIndex(symadr, org, (uint)value);
            return NewSymbol;
        }
        public T PredefCalculateAddress(T ofs, T symadr)
        {
            //dynamic shifted = ReLocation.Sym(ReLocation.Info);
            dynamic org = ofs;
            //SymbolEntry<T> symval = RelocationLink.Source;
            dynamic offset = ReLocation.Offset;
            RelocationLink.SourceLocation.CodeReader.BaseStream.Seek((int)offset, SeekOrigin.Begin);
            uint value = (uint)RelocationLink.SourceLocation.CodeReader.ReadUInt16();
            dynamic v = value;
            Value = v;
            dynamic NewSymbol = ReLocation.GetSymbolTableIndex(symadr, org, (uint)value);
            return NewSymbol;
        }

    }
}
