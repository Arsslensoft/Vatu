using Vasm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestAssembler
{
    class Program
    {
        static void Main(string[] args)
        {
            using (AssemblyWriter str = new AssemblyWriter(@"D:\Research\Vatu\Vatu\VCC\Tests\Test.asm"))
            {
                Vasm.AsmContext asm = new Vasm.AsmContext(str);
                asm.Add(new Instruction[6] {
            new Vasm.x86.Mov { SourceValue = 0x4F07, DestinationReg = Vasm.x86.Registers.AX },
            new Vasm.x86.Mov { SourceValue = 0, DestinationReg = Vasm.x86.Registers.BL },
            new Vasm.x86.Mov { SourceValue = 0, DestinationReg = Vasm.x86.Registers.CX },
            new Vasm.x86.Mov { SourceValue = 0x20, DestinationReg = Vasm.x86.Registers.DX },
            new Vasm.x86.INT { DestinationValue = 0x10 },
                new Vasm.Label("Test")
            });

                asm.DefineData(new DataMember("a", new byte[2] { 65, 89 }));
                asm.DefineData(new DataMember("b", new uint[1] { 9 }));
                asm.Emit(asm.AssemblerWriter);

            }
        }
    }
}
