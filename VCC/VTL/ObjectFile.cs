using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VTL
{
    public class ObjectFile<T> where T : struct
    {
        public string FileName { get; private set; }
        public T Start { get;  set; }
        public T End { get; private set; }
        public bool HasEntryPoint { get; private set; }
        public SymbolEntry<T> EntryPoint { get; private set; }

        public T Align { get; set; }
        public IELF ElfObject { get; private set; }
        public List<SymbolEntry<T>> Symbols { get; set; }
        public List<SymbolEntry<T>> Globals { get; set; }
        public List<SymbolEntry<T>> Undefined { get; set; }
        public Section<T> CodeSection { get; private set; }
        public MemoryStream CodeReaderStream { get; private set; }
        public MemoryStream CodeWriterStream { get; private set; }
        public BinaryReader CodeReader { get; private set; }
        public SymbolEntry<T> InterruptSymbol { get; private set; }


        public MiscUtil.IO.EndianBinaryWriter CodeWriter { get; private set; }
        dynamic Zero = 0u;
        string entry;

        public ObjectFile(string file, T align, T org, string entry, bool isfirst)
        {

            ElfObject = ELFReader.Load(file);
            Symbols = new List<SymbolEntry<T>>();
            Globals = new List<SymbolEntry<T>>();
            Undefined = new List<SymbolEntry<T>>();
            this.entry = entry;
            LoadAllSymbols();
            FileName = file;
            Start = Zero;
            End = Zero;
            Align = align;
            dynamic al = 0u;
            CodeSection = (Section<T>)ElfObject.GetSection(".text");
            if (InterruptSymbol != null)
                al += Align;

            if (!HasEntryPoint && isfirst)// check only first
            {
                al += Align;
                SetOrigin(org + al); // reserved for entry jump
            }
            else
                SetOrigin(org);

            CodeReaderStream = new MemoryStream(CodeSection.GetContents());
            CodeReader = new BinaryReader(CodeReaderStream);
            CodeWriterStream = new MemoryStream(CreateNopBasedBuffer());
            CodeWriter = new MiscUtil.IO.EndianBinaryWriter(CodeSection.readerSourceSourceSource().BitConverter, CodeWriterStream);
        }
        public void LoadAllSymbols()
        {
            var syms = ((ISymbolTable)ElfObject.GetSection(".symtab")).Entries;
            foreach (SymbolEntry<T> s in syms)
            {
                if (s.Name == entry)
                {
                    HasEntryPoint = true;
                    EntryPoint = s;
                }
                if (s.Name.StartsWith("______INSTALL_INTERRUPTS"))
                    InterruptSymbol = s;

                if (s.SpecialPointedSectionIndex == SpecialSectionIndex.Undefined)
                    Undefined.Add(s);
                else
                    Globals.Add(s);

                Symbols.Add(s);
            }

        }

        public void SetOrigin(T origin)
        {
            Start = origin;
            dynamic start = Start;
            dynamic size = CodeSection.Size;

            End = start + size;
        }
        public T FindSymbolAddress(string name)
        {

            T rel = Zero;
            foreach (SymbolEntry<T> sym in Symbols)
            {
                if (sym.Name == name)
                {
                    rel = sym.Value;
                    break;
                }
            }

            dynamic shift = Start;
            return rel + shift;
        }
        public T FindSymbolAddress(int idx)
        {
            if (idx >= Symbols.Count)
                throw new ArgumentException("Error:VL0001:Symbol Index is out of range ");


            T rel = Symbols[idx].Value;
            dynamic shift = Start;
            return rel + shift;
        }
        public SymbolEntry<T> PerformTranslationFix(SymbolEntry<T> sym)
        {
            dynamic shift = Start;
            sym.Value += shift;
            return sym;
        }

        public T GetAlign()
        {
            dynamic align = Align;
            return (End + align - 1u) / align * align;
        }

        public T GetSize()
        {
            dynamic align = Align;
            dynamic st = Start;
            return ((End - st) + align - 1u) / align * align;
        }
        public void WriteRelocation(int offset, ushort address)
        {
            CodeWriter.Seek(offset, SeekOrigin.Begin);
            CodeWriter.Write(address);
        }
        public byte[] CreateNopBasedBuffer()
        {
            dynamic sz = GetSize();
            dynamic codesz = CodeSection.Size;
            byte[] b = new byte[sz];
            Buffer.BlockCopy(CodeSection.GetContents(), 0, b, 0, (int)codesz);
            for (dynamic i = End; i < GetAlign(); i++)
                b[i - Start] = 0;

            return b;
        }


        public void WriteCode(BinaryWriter bw)
        {


            byte[] data = CodeWriterStream.ToArray();
            bw.Write(data, 0, data.Length);

        }
        public void Close()
        {
            CodeWriter.Close();
            CodeReader.Close();
        }

    }
}
