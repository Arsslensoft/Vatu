using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public abstract class Statement : SimpleToken, IEmit, IResolve
    {


        public Statement()
        {
          
        }

   


      
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }


        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return FlowState.Valid;
        }
       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
    } 
}
