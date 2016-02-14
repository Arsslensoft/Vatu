using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	
	public class Case : NormalStatment, IConditional
    {
        public Label CaseLabel { get; set; }
        public Label SwitchLabel { get; set; }
        public Label SwitchExit { get; set; }
        public int CaseIndex { get; set; }
        Label IdentifyChildCase(int id)
        {

            if (id == CaseIndex)
                return CodeLabel;

            return null;
        }
        public Label IdentifyCase(int id,bool def)
        {
            if (!def && IdentifyChildCase(id) != null)
                return IdentifyChildCase(id);
            else if (IsDefault && def)
                return CaseLabel;

            else return null;
        }
        Expr _expr;
        public List<BinaryOperation> CompareOperations { get; set; }
   

        public IConditional ParentIf { get; set; }
        public Label ExitIf { get; set; }
        public Label Else { get; set; }

        public bool IsDefault = false;
        private Statement _statements;
        private Block b;
   

        public int Index { get; set; }
        public Label CodeLabel { get; set; }
        public bool SetSwitch(Switch sw,Label lb,Label swexit,ref int i)
        {
            Index = i;

        
            if (_statements == null)
                return false;
            CodeLabel = new Label(lb.Name + "_CASE_BODY_" + i.ToString());
           if(!IsDefault)
              CaseIndex = sw.CaseLabels.Count;

            SwitchLabel = lb;

            if (!IsDefault)
            {

                CaseLabel = new Label(lb.Name + "_CASE_" + i.ToString());
                if (sw.RedundantCaseValue(CaseLabel))
                    ResolveContext.Report.Error(0, Location, "Multiple case definition with same value");
                i++;
                sw.CaseLabels.Add(CaseLabel);
            }
            else
            {
                CaseLabel = new Label(lb.Name + "_DEFAULT");
                if (sw.RedundantCaseValue(CaseLabel))
                    ResolveContext.Report.Error(0, Location, "Multiple case definition with same value");
                sw.CaseLabels.Add(CaseLabel);
                i++;
            }
            SwitchExit = swexit;

             return IsDefault;
        }
        public void SetCompare(ResolveContext rc,Expr expr)
        {
            if (!IsDefault)
            {
                CompareOperations.Add(new BinaryOperation(expr, new EqualOperator(), _expr));
                CompareOperations[CompareOperations.Count - 1] = (BinaryOperation)CompareOperations[CompareOperations.Count - 1].DoResolve(rc);
             
            }
        }
      
        [Rule("<Case Stm> ::= ~case <Expression>  ~':' <Stm List>")]
        public Case(Expr val, Statement stmt)
        {
            b = new Block((NormalStatment)stmt);
            _statements = stmt;

            _expr = val;
          
            CompareOperations = new List<BinaryOperation>();

        }

        // default
        [Rule("<Case Stm> ::= ~default ~':' <Stm List>   ")]
        public Case(Statement stmt)
        {
            b = new Block((NormalStatment)stmt);
            IsDefault = true;
            _statements = stmt;
            _expr = null;
      
        }

   
       public override bool Resolve(ResolveContext rc)
        {
            return _statements.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (b == null)
                return null;
     if(_expr != null)
            _expr = (Expr)_expr.DoResolve(rc);
            Label next_c = null;
            if (IsDefault)
                Else = SwitchExit;
            else
                Else = rc.EnclosingSwitch.GetNextCaseLabel(Index );
          
          
            ExitIf = SwitchExit;
            ParentIf = rc.EnclosingIf;
            rc.EnclosingIf = this;
            rc.CurrentScope |= ResolveScopes.Case;
            // enter case
          
            b = (Block)b.DoResolve(rc);
      

            // exit case
            rc.CurrentScope &= ~ResolveScopes.Case;
            rc.EnclosingIf = ParentIf;
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            if (_statements == null)
                return false;
          
            // compare if not default
            if (!IsDefault)
            {
      
                    ec.EmitComment("Case " + CaseIndex.ToString());
                    ec.MarkLabel(CaseLabel);// case lb
                    CompareOperations[0].EmitBranchable(ec, Else, false);
               
               

            }
            else
            {
                ec.EmitComment("Case " + CaseIndex.ToString());
                ec.MarkLabel(CaseLabel);// case lb
              
            }

            //case body
            ec.EmitComment("Case " + Index.ToString() + " body");
            ec.MarkLabel(CodeLabel);

                b.Emit(ec);
            
            //Exit switch
            if (!IsDefault)
            {
                ec.EmitComment("Exit switch");
                ec.EmitInstruction(new Jump() { DestinationLabel = SwitchExit.Name });
            }
          
            return true;
        }
    

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
          

            FlowState ok = b.DoFlowAnalysis(fc);
            if(_expr != null)
            _expr.DoFlowAnalysis(fc);
            return ok;
        }

    }

}