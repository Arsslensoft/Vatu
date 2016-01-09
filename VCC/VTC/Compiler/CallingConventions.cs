using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC
{
    public enum CallingConventions 
{
			Cdecl = 1,
			StdCall = 2,
			FastCall = 3,
            Pascal  = 4,
            VeryFastCall = 5,
            Default = 0
	} 
}
