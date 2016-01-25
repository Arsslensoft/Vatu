using ELFSharp.ELF.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VTL
{
    public class Linker
    {
     
        public List<ObjectFile<uint>> ObjectFiles { get; set; }
        public List<LinkerSymbol<uint>> LinkerSymbols { get; set; }
        public BinaryWriter Writer { get; set; }
        public List<SymbolEntry<uint>> AllSymbols { get; set; }
        public uint Origin { get; protected set; }
        public string EntryPoint { get; set; }
        public uint Size { get; private set; }
        public uint Align { get; set; }
        public List<Relocation<uint>> Relocations { get; private set; }
        public List<SymbolEntry<uint>> Interrupts { get; private set; }

       protected SymbolEntry<uint> _entrysymbol;
       protected uint _entry_displacement = 0;
       protected bool isentry_infirst_sec = false;
       protected bool interrupt_enabled = false;
       protected List<string> reserved = new List<string>()
       {
           "PROGRAM_ORG",
           "PROGRAM_END"
       };
        public Linker(Settings opt)
        {
            if (File.Exists(opt.OutputBinary))
                File.Delete(opt.OutputBinary);
            interrupt_enabled = opt.IsInterrupt;
            ObjectFiles = new List<ObjectFile<uint>>();

            Writer = new BinaryWriter(File.Create(opt.OutputBinary), Encoding.BigEndianUnicode);
            LinkerSymbols = new List<LinkerSymbol<uint>>();
            AllSymbols = new List<SymbolEntry<uint>>();
            Relocations = new List<Relocation<uint>>();
            Interrupts = new List<SymbolEntry<uint>>();
            EntryPoint = opt.EntryPoint;
            Origin = opt.Origin;
          
            Align = opt.Align;
            // Map Object Files
            LoadObjectsAndMap(opt.Libraries);

            // Load Predefined symbols
            HandleDefaultLinkerSymbols();

            // Look for duplicates
            FindAllSymbolsAndCheckForDuplicate();
        }
        /// <summary>
        /// Loads all object files and sets their bounds with align & origins
        /// </summary>
        /// <param name="inobj">Input object files</param>
      protected virtual  void LoadObjectsAndMap(string[] inobj)
        {
            ReserveHeader();
            uint org = Origin;
            Size = 0;
            // load object files
            for (int i = 0; i < inobj.Length; i++)
            {
                ObjectFile<uint> obj = new ObjectFile<uint>(inobj[i], Align, org, EntryPoint, i == 0);
                if(obj.InterruptSymbol != null && interrupt_enabled) // interrupt
                         Interrupts.Add(obj.InterruptSymbol);

                org = obj.GetAlign();
                Size += obj.GetSize();
                ObjectFiles.Add(obj);
            }
            if (Interrupts.Count > 1)
                ObjectFiles[0].Start += (uint)(Interrupts.Count - 1) * Align;
        }

        /// <summary>
        /// Reserve first bytes for header size
        /// </summary>
       protected virtual void ReserveHeader()
        {
            //if (ExecutableHeader != null)
            //    Origin += ExecutableHeader.GetSize();

            return;
        }

        /// <summary>
        /// Used for predefined symbol lookup
        /// </summary>
       protected virtual void HandleDefaultLinkerSymbols()
        {
            LinkerSymbols.Add(new LinkerSymbol<uint>("PROGRAM_ORG", Origin));
            LinkerSymbols.Add(new LinkerSymbol<uint>("PROGRAM_END", Size));
        }

        /// <summary>
        /// Loads all symbols and check for duplicate
        /// </summary>
        public void FindAllSymbolsAndCheckForDuplicate()
        {
            int i = 0;
            foreach (ObjectFile<uint> obj in ObjectFiles)
            {
                if (obj.HasEntryPoint)
                {
                    _entrysymbol = obj.EntryPoint;
                    _entry_displacement = obj.Start;
                    isentry_infirst_sec = i == 0;
                }
                if (obj.InterruptSymbol != null)
                    obj.InterruptSymbol.Value += obj.Start;
                 

                foreach (var f in obj.Globals)
                {


                    if (reserved.Contains(f.Name))
                        throw new ArgumentException("VL0004:"+obj.FileName+":" + f.Name + " is a reserved name");
                    if (!AllSymbols.Contains(f))
                        AllSymbols.Add(f);
                    else throw new ArgumentException("VL0001:" + obj.FileName + ":Duplicate symbol definition of " + f.Name);

                } i++;
            }
            // No control on undefined symbols
            foreach (ObjectFile<uint> obj in ObjectFiles)
            {

                foreach (var f in obj.Undefined)
                    AllSymbols.Add(f);

            }
        }
        public SymbolEntry<uint> FindSymbolByName(string name)
        {
            foreach (SymbolEntry<uint> s in AllSymbols)
            {

                if (s.Name == name)
                    return s;
            }
            return null;
        }
        public SymbolEntry<uint> FindEntryPoint()
        {
            return _entrysymbol;
        }

        /// <summary>
        /// Link all
        /// </summary>
        /// <param name="links"></param>
        /// <param name="src"></param>
        /// <param name="sloc"></param>
        /// <returns></returns>
     protected virtual   bool LookForUndefinedSymbols(ref List<Link<uint>> links, SymbolEntry<uint> trg, ObjectFile<uint> tloc)
        {

            foreach (ObjectFile<uint> obj in ObjectFiles)
            {
                // add all undefined symbols
                foreach (SymbolEntry<uint> s in obj.Globals)
                    if (trg.Name == s.Name)
                    {
                        links.Add(new Link<uint>(s, trg, obj, tloc));
                        return true;
                    }
            }
            // Predefined links
            foreach (LinkerSymbol<uint> plnk in LinkerSymbols)
                if (plnk.Name == trg.Name)
                {
                    links.Add(new Link<uint>(trg, tloc, plnk));
                    return true;
                }
            return false;
        }
     protected virtual Link<uint> FindLink(List<Link<uint>> links, SymbolEntry<uint> target, ObjectFile<uint> obj)
        {
            foreach (Link<uint> l in links)
            {
                if (l.PredefinedLink != null && l.PredefinedLink.Name == target.Name)
                    return l;
                else if (l.PredefinedLink == null && l.Target.Name == target.Name && obj.FileName == l.TargetLocation.FileName)
                    return l;
            }
            return null;
        }

        /// <summary>
        /// Get all symbol links
        /// </summary>
        /// <returns>List of links</returns>
        public List<Link<uint>> DoMatchSymbols()
        {
            List<Link<uint>> links = new List<Link<uint>>();
            foreach (ObjectFile<uint> obj in ObjectFiles)
            {

                // look for undefined symbol and link them with globals
                foreach (SymbolEntry<uint> s in obj.Undefined)
                    if (s.Type != SymbolType.File && s.Type != SymbolType.Section && !LookForUndefinedSymbols(ref links, s, obj))
                        throw new ArgumentException("VL0002:"+obj.FileName+":Undefined symbol " + s.Name);



            }
            return links;
        }
        public void Relocate(Relocation<uint> rel)
        {
            uint newsym;
            uint ofs, symadr;
            if (rel.RelocationLink == null) // local relocations
            {
                ofs = rel.ReLocation.Offset + rel.RelocationFile.Start;
                symadr = rel.RelocationSymbol.Value + rel.RelocationFile.Start;

                newsym = rel.LocalCalculateAddress(ofs, symadr);


                rel.RelocationFile.WriteRelocation((int)(rel.ReLocation.Offset), (ushort)newsym);
                //  Console.WriteLine( " ofs=" + ofs + "  dd=" + rel.Value + " new dd=" + newsym );
            }
            else
            {
                if (rel.RelocationLink.PredefinedLink == null)
                {
                    ofs = rel.ReLocation.Offset + rel.RelocationLink.TargetLocation.Start;
                    symadr = rel.RelocationLink.Source.Value + rel.RelocationLink.SourceLocation.Start;
                    newsym = rel.CalculateAddress(ofs, symadr);
                    rel.RelocationLink.TargetLocation.WriteRelocation((int)(rel.ReLocation.Offset), (ushort)newsym);
                    //       Console.WriteLine(" ofs=" + ofs + "  dd=" + rel.Value + "  new dd=" + newsym);
                }
                else
                { // predefined symbol
                    ofs = rel.ReLocation.Offset + rel.RelocationLink.SourceLocation.Start;
                    symadr = rel.RelocationLink.PredefinedLink.Address;
                    newsym = rel.PredefCalculateAddress(ofs, symadr);
                    rel.RelocationLink.SourceLocation.WriteRelocation((int)(rel.ReLocation.Offset), (ushort)newsym);

                }

            }
        }
        public void RelocateAll(List<Link<uint>> links)
        {
            foreach (ObjectFile<uint> obj in ObjectFiles)
            {
                ISection rsect=null;

                if (!obj.ElfObject.TryGetSection(".rel.text", out rsect))
                    continue;
                var ssyms = ((RelocationSection<uint>)rsect).Entries;
                
                foreach (RelocationEntry<uint> r in ssyms)
                {
                    uint sym = r.Sym(r.Info);
                    SymbolEntry<uint> symval = obj.Symbols.ToList()[(int)sym];
                    if (symval.SpecialPointedSectionIndex == SpecialSectionIndex.Undefined) // external symbol
                    {

                        Link<uint> lnk = FindLink(links, symval, obj);
                        if (lnk == null)
                            throw new ArgumentException("VL0003:" + obj.FileName + ":Link not found " + symval.Name);
                        else
                        {
                            Relocation<uint> rel = new Relocation<uint>(lnk, r);
                            Relocations.Add(rel);
                        }
                    }
                    else
                    {


                        Relocation<uint> rel = new Relocation<uint>(obj, r, symval);
                        Relocations.Add(rel);

                    }
                }
            }
        }

        public virtual void Link()
        {
            WriteHeader();

            SymbolEntry<uint> entry = FindEntryPoint();
            // Install interrupts 
            foreach (SymbolEntry<uint> inter in Interrupts)
            {
                    Writer.Write((byte)0xE8);
                    Writer.Write((ushort)(inter.Value - Origin - 4));
                    Writer.Write((byte)0);             
            }
            // Jump to Entry Point
                Writer.Write((byte)0xE9);
                Writer.Write((ushort)(entry.Value + _entry_displacement - Origin - 3));
                Writer.Write((byte)0);
       

            // Relocate all
            foreach (Relocation<uint> rel in Relocations)
                Relocate(rel);



            // Emit All
            foreach (ObjectFile<uint> obj in ObjectFiles)
                obj.WriteCode(Writer);

            WriteFooter();
        }
        public virtual void WriteHeader()
        {
            return;
        }
        public virtual void WriteFooter()
        {
            return;
        }
        public void FillWithByte(int off, byte b, int size)
        {
            Writer.Seek(off, SeekOrigin.Begin);
            for (int i = 0; i < size; i++)
                Writer.Write((byte)b);

        }

        public void Close()
        {
            foreach (ObjectFile<uint> obj in ObjectFiles)
                obj.Close();
            Writer.Flush();
            Writer.Close();
        }

    }
}
