﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
  [Vasm.OpCode("mov")]
  public class Mov : InstructionWithDestinationAndSourceAndSize {
      public Mov():base("mov")
      {
      }
  }
}
