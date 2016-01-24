using ELFSharp.ELF.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
    public class Link<T> where T : struct
    {
        public LinkerSymbol<T> PredefinedLink { get; private set; }

        public ObjectFile<T> SourceLocation { get; private set; }
        public ObjectFile<T> TargetLocation { get; private set; }

        public SymbolEntry<T> Source { get; private set; }
        public SymbolEntry<T> Target { get; private set; }

        public Link(SymbolEntry<T> src, SymbolEntry<T> trg, ObjectFile<T> sloc, ObjectFile<T> tloc)
        {
            SourceLocation = sloc;
            TargetLocation = tloc;
            Source = src;
            Target = trg;
        }

        public Link(SymbolEntry<T> src, ObjectFile<T> sloc, LinkerSymbol<T> pre)
        {
            SourceLocation = sloc;
            Source = src;
            PredefinedLink = pre;
        }
    }
}
