using VTC.Base.GoldParser.Semantic;
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
       [Rule(@"<Modifier>      ::= protected")]
       [Rule(@"<Modifier>      ::= internal")]
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


       public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }

       bool DuplicateModifiers(Modifiers mod, bool setm)
       {
           if (setm)
           {
               if (((mod & Modifiers.Private) == Modifiers.Private) || ((mod & Modifiers.Protected) == Modifiers.Protected) || ((mod & Modifiers.Public) == Modifiers.Public) || ((mod & Modifiers.Internal) == Modifiers.Internal))
                   return true;
               else return false;
           }
           else if ((mod & Modifiers.Private) == Modifiers.Private)
               return DuplicateModifiers(mod & ~Modifiers.Private, true);
           else if ((mod & Modifiers.Public) == Modifiers.Public)
               return DuplicateModifiers(mod & ~Modifiers.Public, true);
           else if ((mod & Modifiers.Internal) == Modifiers.Internal)
               return DuplicateModifiers(mod & ~Modifiers.Internal, true);
           else if ((mod & Modifiers.Protected) == Modifiers.Protected)
               return DuplicateModifiers(mod & ~Modifiers.Protected, true);
           else return false;
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
            else if (_mod.Name == "Modifier")
            {
                nmod = (Modifier)_mod.DoResolve(rc);
                ModifierList |= nmod.ModifierList;
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
            else if (_mod.Name == "protected")
                ModifierList |= Modifiers.Protected;
            else if (_mod.Name == "internal")
                ModifierList |= Modifiers.Internal;


            if ((ModifierList & Modifiers.Private) == Modifiers.Private && (ModifierList & Modifiers.Extern) == Modifiers.Extern)
                ResolveContext.Report.Error(0, Location, "A member cannot be private and extern at the same time");
            else if (DuplicateModifiers(ModifierList,false))
                ResolveContext.Report.Error(0, Location, "A member cannot have multiple access modifiers at the same time");

            return this;
        }
       
    }

	
	
}