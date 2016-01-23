using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	public class Modifier : SimpleToken
    {
        public Modifiers ModifierList { get; set; }

        /*<Mod>      ::= extern 
             | static
             | register
             | auto
             | volatile
             | const   */
       protected SimpleToken _mod;
        [Rule(@"<Modifier>      ::= extern")]
        [Rule(@"<Modifier>      ::= static")]
        [Rule(@"<Modifier>      ::= const")]
        [Rule(@"<Modifier>      ::= private")]
        [Rule(@"<Modifier>      ::= public")]
        public Modifier(SimpleToken mod)
        {
            _mod = mod;
           
        }

        Modifier nmod;
         [Rule(@"<Mod>      ::= <Modifier>")]
        public Modifier(Modifier mod)
        {
            nmod = mod;

        }

         Modifier nxt;
         [Rule(@"<Mod>      ::= <Modifier> <Mod>")]
         public Modifier(Modifier mod,Modifier next)
         {
             nxt = next;
             _mod = mod;

         }


        public override SimpleToken DoResolve(ResolveContext rc)
        {
            ModifierList = 0;
            if (nxt != null)
            {
                nxt = (Modifier)nxt.DoResolve(rc);
                ModifierList |= nxt.ModifierList;
            }
          
            if (_mod == null)
            {
                if (nmod != null)
                {
                    nmod = (Modifier)nmod.DoResolve(rc);
                    ModifierList |= nmod.ModifierList;

                }
                else                ModifierList |= Modifiers.Private;
            }
            else if (_mod.Name == "extern")
                ModifierList |= Modifiers.Extern;
            else if (_mod.Name == "static")
                ModifierList |= Modifiers.Static;
            else if (_mod.Name == "const")
                ModifierList |= Modifiers.Const;
            else if (_mod.Name == "private")
                ModifierList |= Modifiers.Private;
            else if (_mod.Name == "public")
                ModifierList |= Modifiers.Public;

            if ((ModifierList & Modifiers.Private) == Modifiers.Private && (ModifierList & Modifiers.Extern) == Modifiers.Extern)
                ResolveContext.Report.Error(0, Location, "A member cannot be private and extern at the same time");
            else if ((ModifierList & Modifiers.Private) == Modifiers.Private && (ModifierList & Modifiers.Public) == Modifiers.Public)
                ResolveContext.Report.Error(0, Location, "A member cannot be private and public at the same time");

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
    }

	
	
}