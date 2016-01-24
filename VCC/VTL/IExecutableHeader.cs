using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VTL
{
    public interface IExecutableHeader
    {
        uint GetSize();
        void WriteHeader(BinaryWriter bw);
    }
}
