using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;


namespace VTC.Core
{

 

    #region Access Operators

    [Terminal("->")]
    public class ByAddressOperator : AccessOp
    {
        public ByAddressOperator()
        {
            _op = AccessOperator.ByAddress;
        }

    }

 
    



#endregion

  
}
