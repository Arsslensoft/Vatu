using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class UseStatement : BaseStatement
    {
        
        Statement _stmt;
        Namespace ns;
        [Rule(@"<Statement>        ::= ~use <Name> <Statement>")]
        public UseStatement(NameIdentifier ni,Statement stmt)
        {
   
            ns = new Namespace(ni.Name);
            _stmt = stmt;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            int index = -1;
            // Set priority
            if (rc.Resolver.Imports.Contains(ns))
            {
                index = rc.Resolver.Imports.IndexOf(ns);
                rc.Resolver.Imports.RemoveAt(index);
                
            }
         
            // insert at 0
            rc.Resolver.Imports.Insert(0, ns);    


            _stmt = (Statement)_stmt.DoResolve(rc);
       
            // remove at 0
               rc.Resolver.Imports.RemoveAt(0);

               if (index != -1) // insert at index
                   rc.Resolver.Imports.Insert(index, ns);

           
          
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return _stmt.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            _stmt.Emit(ec);
            return true;
        }
        public override Reachability MarkReachable(Reachability rc)
        {
            base.MarkReachable(rc);
            if (_stmt is Block || _stmt is BlockStatement)
                return _stmt.MarkReachable(rc);
            else
                return rc;
        }


        public override bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            CodePath cur = new CodePath(_stmt.loc); // sub code path
   
            CodePath back = fc.CodePathReturn;
            fc.CodePathReturn = cur; // set current code path

            bool ok = _stmt.DoFlowAnalysis(fc);
            back.AddPath(cur);
            fc.CodePathReturn = back; // restore code path
            return ok;
        }
    }
    
	
}