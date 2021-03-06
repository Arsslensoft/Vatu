﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("jmp")]
    public class JumpToSegment : Instruction {
        public Vasm.ElementReference DestinationRef {
            get;
            set;
        }

        public ushort Segment {
            get;
            set;
        }

        public override void WriteText( Vasm.AsmContext aAssembler, AssemblyWriter aOutput )
        {
                if (DestinationRef != null) {
                    aOutput.Write("jmp ");
                    aOutput.Write(Segment);
                    aOutput.Write(":");
                    aOutput.Write(DestinationRef.ToString());
                } else {
                    aOutput.Write("jmp ");
                    aOutput.Write(Segment);
                    aOutput.Write(":0x0");
                }
        }

        public string DestinationLabel {
            get {
                if (DestinationRef != null) {
                    return DestinationRef.Name;
                }
                return String.Empty;
            }
            set {
                DestinationRef = Vasm.ElementReference.New(value);
            }
        }

        public override bool IsComplete( Vasm.AsmContext aAssembler )
        {
            ulong xAddress;
            return DestinationRef == null || DestinationRef.Resolve(aAssembler, out xAddress);
        }

        public override void UpdateAddress(Vasm.AsmContext aAssembler, ref ulong aAddress) {
            base.UpdateAddress(aAssembler, ref aAddress);
            aAddress += 7;
        }

        //public override byte[] GetData(Assembler aAssembler) {
        public override void WriteData( Vasm.AsmContext aAssembler, System.IO.Stream aOutput )
        {
            aOutput.WriteByte(0xEA);
            ulong xAddress = 0;
            if (DestinationRef != null && DestinationRef.Resolve(aAssembler, out xAddress)) {
                xAddress = (ulong)(((long)xAddress) + DestinationRef.Offset);
            }
            aOutput.Write(BitConverter.GetBytes((uint)(xAddress)), 0, 4);
            aOutput.Write(BitConverter.GetBytes(Segment), 0, 2);
        }
    }
}