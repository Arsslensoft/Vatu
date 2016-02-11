using ELFSharp.ELF.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTL
{
   public class BootloaderLinker : Linker
    {
       public BootloaderLinker(Settings opt)
           : base(opt)
       {
           
       }
       public override void WriteFooter()
       {
           if ( Writer.BaseStream.Position < 510)
           {
               FillWithByte((int)Writer.BaseStream.Position, 0, 510 - (int)Writer.BaseStream.Position);
               // bootloader signature
               Writer.Write((ushort)0xAA55);

           }
       }

       public override void Link()
       {
           WriteHeader();

           SymbolEntry<uint> entry = FindEntryPoint();
     
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
    
    }
}
