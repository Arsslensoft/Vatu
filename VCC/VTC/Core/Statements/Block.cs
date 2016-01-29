using bsn.GoldParser.Semantic;
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
        public override bool Resolve(ResolveContext rc)
        {
            if (_statements != null)

                return _statements.Resolve(rc);
            else return true;
        }
        public override bool Emit(EmitContext ec)
        {
            bool ok = true;
            foreach (NormalStatment stmt in Statements)
                ok &= stmt.Emit(ec);
            
            return ok;
        }
        public override Reachability MarkReachable(Reachability rc)
        {
            if (rc.IsUnreachable)
                return rc;

            base.MarkReachable(rc);
            int i = 0;
            foreach (var s in Statements)
            {
                i++;
                rc = s.MarkReachable(rc);

                if (rc.IsUnreachable && i < Statements.Count && Statements[i].current != null)
                {
                    rc.Loc = Statements[i].current.Location;
                    return rc;
                }
                else rc = new Reachability();
                
            }

    

            return rc;
         
        }
        public override bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            CodePath cur = new CodePath(loc); // sub code path
        
            CodePath back = fc.CodePathReturn;
            fc.CodePathReturn = cur; // set current code path
            bool ok = true;
           
            foreach(var st in Statements)
             ok &= st.DoFlowAnalysis(fc);
            back.AddPath(cur);
            fc.CodePathReturn = back; // restore code path
            return ok;
        }
    }
    
	
}