﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser;
using VTC.Core;

namespace VTC
{
    public class DependencyParsing : IEquatable<DependencyParsing>
    {
        public Report Report { get; set; }
        public DependencyParsing()
        {
            Report = new ConsoleReporter();
            RessourcesSpecs = new Dictionary<FieldSpec, byte[]>(new MemberSpecEqualityComparer());
        }
        public ResolveContext RootCtx { get; set; }
 
        public Dictionary<FieldSpec,byte[]> RessourcesSpecs { get; set; }

        public List<ResolveContext> ResolveCtx { get; set; }
        public List<Declaration> Declarations { get; set; }
        public bool Parsed { get; set; }
        public string File { get; set; }
        public List<DependencyParsing> DependsOn = new List<DependencyParsing>();
        public ParserReader InputStream { get; set; }
        public override string ToString()
        {
            return File;
        }
        public bool Equals(DependencyParsing dep)
        {
            return dep.File == File;
        }
    }

}
