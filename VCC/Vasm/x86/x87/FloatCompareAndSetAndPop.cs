using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86.x87
{
    [Vasm.OpCode("fcomip")]
    public class FloatCompareAndSetAndPop : InstructionWithDestination
    {
    }
    [Vasm.OpCode("fcompp")]
    public class FloatCompareAnd2Pop : Instruction
    {


    }
    [Vasm.OpCode("fstsw")]
    public class FloatStoreStatus : InstructionWithDestination
    {


    }
    [Vasm.OpCode("sahf")]
    public class StoreAHToFlags : Instruction
    {


    }
    [Vasm.OpCode("fwait")]
    public class FloatWait : Instruction
    {


    }
   
}
