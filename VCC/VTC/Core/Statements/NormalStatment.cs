using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class NormalStatment : Statement
    {


        public Statement _next;
        public Statement current;


        [Rule("<Stm List>  ::=  <Statement> <Stm List> ")]
        public NormalStatment(Statement stm, Statement next)
        {
            current = stm;
            _next = next;

        }
        [Rule("<Stm List>  ::=  ")]
        public NormalStatment()
            :
            this(null, null)
        {


        }

       public override bool Resolve(ResolveContext rc)
        {
            if (current != null)
                current.Resolve(rc);
            if (_next != null)
                _next.Resolve(rc);
            return base.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (current != null)
                current = (Statement)current.DoResolve(rc);
            if (_next != null)
                _next = (Statement)_next.DoResolve(rc);
           
            return this;
        }
     
       
        public override bool Emit(EmitContext ec)
        {
            if (current != null)
                current.Emit(ec);

         
            return base.Emit(ec);
        }
     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (current != null)
                return current.DoFlowAnalysis(fc);
            return base.DoFlowAnalysis(fc);
        }
    }
   
	
}