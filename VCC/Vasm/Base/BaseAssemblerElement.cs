using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vasm
{
    public abstract class BaseAssemblerElement
    {
        /// Gets/sets the address at which the element could start emitting data. Note that if
        /// the element needs any alignment, start address is unaligned, and the element should 
        /// do the alignment itself. ActualAddress is used for referencing the actual address.
        public virtual ulong? StartAddress
        {
            get
            {
#if BINARY_COMPILE
                return startAddress;
#else
                return null;
#endif

            }
            set
            {
#if BINARY_COMPILE
                startAddress = value;
#endif

            }
        }

        public virtual ulong? ActualAddress
        {
            get
            {
#if BINARY_COMPILE
                return actualAddress;
#else
                return null;
#endif

            }
        }



#if BINARY_COMPILE
        ulong? startAddress;
        ulong actualAddress;
#endif


        public virtual void UpdateAddress(AsmContext ec, ref ulong aAddress)
        {
            StartAddress = aAddress;
        }

        public abstract bool IsComplete(AsmContext ec);

        public abstract void WriteData(AsmContext ec, Stream aOutput);

        public abstract void WriteText( AsmContext ec, AssemblyWriter aOutput );

        public sealed override string ToString()
        {
            return base.ToString();
        }
    }
}