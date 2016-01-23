using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    [Terminal(":=")]
    [Terminal("define")]
    [Terminal("LOW")]
    [Terminal("HIGH")]
    [Terminal("setof")]
    [Terminal("global")]
    [Terminal("include")]
    [Terminal("implicit")]
    [Terminal("explicit")]
  
    [Terminal("exit")]
    [Terminal("pass")]
    
    [Terminal("nameof")]
    [Terminal("typeof")]
    [Terminal("addressof")]
    [Terminal("extends")]
    [Terminal("delegate")]
    [Terminal("public")]
    [Terminal("vfastcall")]
    [Terminal("ref")]
    [Terminal("isolated")]
    [Terminal("interrupt")]
    [Terminal("union")]
    [Terminal("pascal")]
    [Terminal("operator")]
    [Terminal("override")]
    [Terminal("loop")]
    [Terminal("asm")]
    [Terminal("break")]
    [Terminal("next")]
    [Terminal("case")]
    [Terminal("continue")]
    [Terminal("default")]
    [Terminal("do")]
    [Terminal("else")]
    [Terminal("for")]
    [Terminal("goto")]
    [Terminal("if")]
    [Terminal("return")]
    [Terminal("sizeof")]
    [Terminal("switch")]
    [Terminal("typedef")]
    [Terminal("while")]
    [Terminal("use")]
    [Terminal("namespace")]
    [Terminal("(EOF)")]
    [Terminal("(Error)")]
    [Terminal("(Whitespace)")]
    [Terminal("(Comment)")]
    [Terminal("(NewLine)")]
    [Terminal("(*/)")]
    [Terminal("(//)")]
    [Terminal("(/*)")]
    [Terminal("(")]
    [Terminal(")")]
    [Terminal("{")]
    [Terminal("}")]
    [Terminal("[")]
    [Terminal("]")]
    [Terminal(":")]
    [Terminal(";")]
    [Terminal("?")]
    [Terminal(",")]

    [Terminal("@")]
    [Terminal("enum")]
    [Terminal("struct")]
    [Terminal("extern")]
    [Terminal("static")]
    [Terminal("const")]
    [Terminal("entry")]
    [Terminal("stdcall")]
    [Terminal("fastcall")]
    [Terminal("cdecl")]
    [Terminal("private")]
    public class SimpleToken : SemanticToken, IResolve
    {
        public Location loc;
        public Location Location { get { return CompilerContext.TranslateLocation(position); } }

        public virtual string Name { get { return symbol.Name; } }

        public virtual bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    }
 
}
