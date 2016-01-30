using bsn.GoldParser.Semantic;
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
            if (ResolvedConstants == null)
                return null;

            int i = 0;
            foreach (ConstantExpression ic in ResolvedConstants)
            {
                if (int.Parse(ic.GetValue().ToString()) == id)
                    return CodeLabel;

                i++;
            }
            return null;
        }
        public Label IdentifyCase(int id,bool def)
        {
            if (!def && IdentifyChildCase(id) != null)
                return IdentifyChildCase(id);
            else if (IsDefault && def)
                return CaseLabel;

            else return CaseLabel;
        }
      
        public List<BinaryOperation> CompareOperations { get; set; }
   

        public IConditional ParentIf { get; set; }
        public Label ExitIf { get; set; }
        public Label Else { get; set; }

        public bool IsDefault = false;
        private Statement _statements;
        private Block b;
        private IntegralConstSequence<IntegralConst> _val;
        public List<ConstantExpression> ResolvedConstants { get; set; }
        public List<Label> ChildCases { get; set; }

        public int Index { get; set; }
        public Label CodeLabel { get; set; }
        public bool SetSwitch(Switch sw,Label lb,Label swexit,ref int i)
        {
            Index = i;

        
            if (_statements == null)
                return false;
            CodeLabel = new Label(lb.Name + "_CASE_BODY_" + i.ToString());
           if(!IsDefault)
              CaseIndex = int.Parse( ResolvedConstants[0].GetValue().ToString());

            SwitchLabel = lb;

            if (!IsDefault)
            {

                foreach (ConstantExpression ic in ResolvedConstants)
                {
                    CaseLabel = new Label(lb.Name + "_CASE_" + int.Parse(ic.GetValue().ToString()).ToString());
                    ChildCases.Add(CaseLabel);
                    if (sw.RedundantCaseValue(CaseLabel))
                                     ResolveContext.Report.Error(0, Location, "Multiple case definition with same value");
                    
                    sw.CaseLabels.Add(CaseLabel);
                }
                CaseLabel = ChildCases[0];
                i = i + ChildCases.Count;
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
                foreach (ConstantExpression ct in ResolvedConstants)
                {
                    if (ct != null)
                    {
                        CompareOperations.Add(new BinaryOperation(expr, new EqualOperator(), ct));
                        CompareOperations[CompareOperations.Count - 1] = (BinaryOperation)CompareOperations[CompareOperations.Count - 1].DoResolve(rc);
                    }
                   
                }
             
            }
        }
        [Rule("<Case Stm> ::= ~case <Integral Const List>  ~':' <Stm List>")]
        public Case(IntegralConstSequence<IntegralConst> val, Statement stmt)
        {
            b = new Block((NormalStatment)stmt);
            _statements = stmt;
            _val = val;
            ChildCases = new List<Label>();
            ResolvedConstants = new List<ConstantExpression>();
            foreach (IntegralConst ct in _val)
            {
                if (ct != null)
                    ResolvedConstants.Add(ct.Value);
            }
            CompareOperations = new List<BinaryOperation>();
  
        }

        // default
        [Rule("<Case Stm> ::= ~default ~':' <Stm List>   ")]
        public Case(Statement stmt)
        {
            b = new Block((NormalStatment)stmt);
            IsDefault = true;
            _statements = stmt;
            _val = null;
      
        }

   
       public override bool Resolve(ResolveContext rc)
        {
            return _statements.Resolve(rc);
        } public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (b == null)
                return null;

            Label next_c = null;
            if (IsDefault)
                Else = SwitchExit;
            else
                Else = rc.EnclosingSwitch.GetNextCaseLabel(Index + ChildCases.Count - 1);
          
          
            ExitIf = SwitchExit;
            ParentIf = rc.EnclosingIf;
            rc.EnclosingIf = this;
            rc.CurrentScope |= ResolveScopes.Case;
            // enter case
            if (!IsDefault)
            {
                for (int i = 0; i < ResolvedConstants.Count; i++)
                {


                    ResolvedConstants[i] = (ConstantExpression)ResolvedConstants[i].DoResolve(rc);
                    if (!ResolvedConstants[i].Type.IsNumeric)
                        ResolveContext.Report.Error(39, Location, "Could not use switch with non numeric types");




                }
            }
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
                if (CompareOperations.Count == 1)
                {
                    ec.EmitComment("Case " + CaseIndex.ToString());
                    ec.MarkLabel(CaseLabel);// case lb
                    CompareOperations[0].EmitBranchable(ec, Else, false);
                }
                else
                {

                    for (int i = 0; i < ChildCases.Count - 1; i++)
                    {
                        ec.EmitComment("Case " + ChildCases[i].ToString());
                        ec.MarkLabel(ChildCases[i]);// case lb
                        CompareOperations[i].EmitBranchable(ec, ChildCases[i + 1], false);
                        ec.EmitInstruction(new ConditionalJump() { Condition = ConditionalTestEnum.Equal, DestinationLabel = CodeLabel.Name });
                    }
                    ec.EmitComment("Case " + ChildCases[ChildCases.Count - 1].ToString());
                    ec.MarkLabel(ChildCases[ChildCases.Count - 1]);// case lb
                    CompareOperations[CompareOperations.Count - 1].EmitBranchable(ec, Else, false);
                }

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
            CodePath cur = new CodePath(loc); // sub code path
         
            CodePath back = fc.CodePathReturn;
            fc.CodePathReturn = cur; // set current code path

            FlowState ok = b.DoFlowAnalysis(fc);
            back.AddPath(cur);
            fc.CodePathReturn = back; // restore code path
            return ok;
        }
    }

}