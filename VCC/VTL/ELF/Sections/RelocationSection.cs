using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ELFSharp.ELF.Sections
{
    public class RelocationSection<T> : Section<T>, IRelocationSection<T> where T : struct
    {
      public List<IRelocationEntry<T>> Entries { get; private set; }
      internal RelocationSection(SectionHeader header, Class elfClass, Func<EndianBinaryReader> readerSource)
          : base(header, readerSource)
        {
            Entries = new List<IRelocationEntry<T>>();
            ReadEntries(readerSource, header.Offset, (int)(header.Size / (long)header.EntrySize));
        }
      void ReadEntries(Func<EndianBinaryReader> readerSource, long sectionOffset,int entriescount)
      {
          using (reader = readerSource())
          {
              reader.BaseStream.Seek(sectionOffset, SeekOrigin.Begin);
              for (int i = 0; i < entriescount; i++)
              {
                  dynamic off = reader.ReadUInt32();
                  dynamic info = reader.ReadUInt32();
                  dynamic z = 0u;
                  RelocationEntry<T> r = new RelocationEntry<T>(off, info, z);
                  Entries.Add(r);
              }
          }
      }
      private EndianBinaryReader reader;
    }
}
