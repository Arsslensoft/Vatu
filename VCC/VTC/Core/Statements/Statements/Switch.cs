using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class Switch : NormalStatment
    {
    
        bool HasDefault { get; set; }
        public Label SWITCH { get; set; }
        public Label SWITCH_EXIT { get; set; }
         public Label ResolveCase(int i,bool isdef)
        {
            Label lb = null;
            foreach (Case c in ResolvedCases)
                if ((lb = c.IdentifyCase(i, isdef)) != null)
                    return lb;

            return null;
        }
         public Label GetNextCaseLabel(int i)
         {
             if ((i+1) >= CaseLabels.Count)
                 return SWITCH_EXIT;
             else return CaseLabels[i+1];
             //if (ResolveCase(i + 1, false) == null)
             //    return ResolveCase(i + 1, true);
             //else return ResolveCase(i + 1, false);
         }
         public bool RedundantCaseValue(Label lb)
         {
             return CaseLabels.Contains(lb);    
         }
         public List<Label> CaseLabels { get; set; }
        private CaseSequence<Case> _cases;
        public List<Case> ResolvedCases { get; set; }
        private Expr _expr;

        [Rule("<Normal Stm> ::= ~switch ~'(' <Expression> ~')' ~'{' <Case Stms> ~'}'")]
        public Switch(Expr sw, CaseSequence<Case> cases)
        {
            ResolvedCases = new List<Case>();
            CaseLabels = new List<Label>();
            _cases = cases;
            _expr = sw;
        }
       public override bool Resolve(ResolveContext rc)
        {      // define switch
            SWITCH = rc.DefineLabel(LabelType.SW);
            SWITCH_EXIT = rc.DefineLabel(SWITCH.Name +"_EXIT");
            foreach (Case c in _cases)
                if (c != null)
                {
                    c.Resolve(rc);
                    ResolvedCases.Add(c);
                }
            HasDefault = false;
            int index = 0;
            if (ResolvedCases.Count > 0)
            {
            for(int i = 0; i < ResolvedCases.Count ;i++)
                HasDefault = HasDefault || ResolvedCases[i].SetSwitch(this,SWITCH, SWITCH_EXIT,ref index);
            }

            _expr.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {


           

            rc.EnclosingSwitch = this;

            _expr = (Expr)_expr.DoResolve(rc);

            for (int i = 0; i < ResolvedCases.Count; i++)
            {
                ResolvedCases[i] = (Case)ResolvedCases[i].DoResolve(rc);
                ResolvedCases[i].SetCompare(rc, _expr);

            }


            if (!_expr.Type.IsNumeric)
                ResolveContext.Report.Error(39, Location, "Could not use switch with non numeric types");
            else if(_expr is ConstantExpression)
                ResolveContext.Report.Error(41, Location, "Could not use switch with non constant values");

            rc.EnclosingSwitch = null;
            return this;
        }
       
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("Switch ");
          //  ec.MarkLabel(SWITCH);
            foreach (Case c in ResolvedCases)
                c.Emit(ec);
            ec.MarkLabel(SWITCH_EXIT);
            return true;
        }

   
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            CodePath cur = new CodePath(loc); // sub code path
        
            CodePath back = fc.CodePathReturn;
            fc.CodePathReturn = cur; // set current code path
            FlowState ok = FlowState.Valid;
            foreach (Case c in ResolvedCases)
             ok &= c.DoFlowAnalysis(fc);
            back.AddPath(cur);
            fc.CodePathReturn = back; // restore code path
            return ok;
        }
    }
    
	
}