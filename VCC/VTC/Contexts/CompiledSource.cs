using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VTC
{
    public class CompiledSource : IEquatable<CompiledSource>
    {
        public AssemblyWriter Asmw { get; set; }
        public DependencyParsing DefaultDependency { get; set; }
        public TimeSpan CompileTime { get; set; }
      
        public override string ToString()
        {
            return DefaultDependency.File;
        }
        public bool Equals(CompiledSource dep)
        {
            return dep.DefaultDependency.File == DefaultDependency.File;
        }
    }
}
