﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm.x86 {
    [Vasm.OpCode("add")]
	public class Add: InstructionWithDestinationAndSourceAndSize {
        public Add() : base("add")
        {
            OptimizingBehaviour = OptimizationKind.None;
        }
	}
}