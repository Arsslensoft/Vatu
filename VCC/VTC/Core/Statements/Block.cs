using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class Block : NormalStatment
    {
        public List<NormalStatment> Statements { get; set; }


        private NormalStatment _statements;
        [Rule("<Block>     ::= ~'{' <Stm List> ~'}' ")]
        public Block(NormalStatment stmt)
        {
            _statements = stmt;
            Statements = new List<NormalStatment>();
        }

       public override bool Resolve(ResolveContext rc)
        {
            if (_statements != null)

                return _statements.Resolve(rc);
            else return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _statements = (NormalStatment)_statements.DoResolve(rc);
       
                NormalStatment ns = _statements;
        
                while (ns != null)
                {
                    Statements.Add(ns);
                    ns = (NormalStatment)ns._next;
                }
           
            return this;
        }
     
        public override bool Emit(EmitContext ec)
        {
            bool ok = true;
            foreach (NormalStatment stmt in Statements)
                ok &= stmt.Emit(ec);
            
            return ok;
        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
         
            FlowState ok = FlowState.Valid;
            int i = 0;
            bool markedunreachable = false;
      
                foreach (var st in Statements)
                {
                    FlowState p = st.DoFlowAnalysis(fc);

                    if (markedunreachable && i < Statements.Count && Statements[i].current != null)
                    {
                        fc.ReportUnreachable(Statements[i].current.Location);

                        break;
                    }
                    else if (markedunreachable && fc.LookForUnreachableBrace && (i < Statements.Count && Statements[i].current == null))
                        return FlowState.Unreachable;
                    else if (!markedunreachable && fc.LookForUnreachableBrace && (i < Statements.Count && Statements[i].current == null))
                        return FlowState.Valid;
                    else ok.Reachable = new Reachability();
                    markedunreachable = p.Reachable.IsUnreachable;
                    i++;
                }
            
     
                return ok;
            }
       
    }
    
	
}