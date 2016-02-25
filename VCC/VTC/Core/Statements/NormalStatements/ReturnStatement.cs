using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ReturnStatement : NormalStatment
    {
        public Label ReturnLabel { get; set; }
        MethodSpec Method;
        private ParameterSequence<Expr> _expr;
        public List<Expr> Expressions { get; set; }
        
        [Rule("<Normal Stm> ::= ~return <PARAM EXPR> ~';' ")]
        public ReturnStatement(ParameterSequence<Expr> b)
        {
            _expr = b;

        }
      
       public override bool Resolve(ResolveContext rc)
        {
            if (_expr != null)
                return _expr.Resolve(rc);
            else return true;
        }


       public bool PopAllToRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
       {

           int s = size / 2;


           for (int i = 0; i < s; i++)
               ec.EmitInstruction(new Pop() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });
           if (size % 2 != 0)
           {
               ec.EmitPop(RegistersEnum.DX);
               ec.EmitInstruction(new Mov() { DestinationReg = rg, DestinationDisplacement = offset - 1 + size, DestinationIsIndirect = true, Size = 8, SourceReg = RegistersEnum.DL });

           }
           return true;
       }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            Method = rc.CurrentMethod;
            if (_expr != null)
            {
                Expressions = new List<Expr>();
                _expr = (ParameterSequence<Expr>)_expr.DoResolve(rc);
                foreach (Expr p in _expr)
                {
                    Expr e = (Expr)p.DoResolve(rc);
                    Expressions.Add(e);

                }
                

                if (Expressions.Count == 1 && !TypeChecker.CompatibleTypes(Expressions[0].Type, rc.CurrentMethod.MemberType))
                                  ResolveContext.Report.Error(0, Location, "Return expression type must be " + rc.CurrentMethod.MemberType.Name);
                else if(Expressions.Count == 0 && !rc.CurrentMethod.MemberType.Equals(BuiltinTypeSpec.Void))
                     ResolveContext.Report.Error(0, Location, "Empty returns are only used with void methods");
                else if (Expressions.Count > 1 && !rc.CurrentMethod.MemberType.IsTypeCollection)
                    ResolveContext.Report.Error(0, Location, "Group return are only used with type collections");
                else if (rc.CurrentMethod.MemberType.IsTypeCollection)
                {
                    StructTypeSpec st = rc.CurrentMethod.MemberType as StructTypeSpec;
                    int i = 0;
                    foreach (TypeMemberSpec tmp in st.Members)
                    {
                        if (!TypeChecker.CompatibleTypes(Expressions[i].Type, tmp.MemberType))
                            ResolveContext.Report.Error(0, Expressions[i].Location, "Group Return expression type must be " + tmp.MemberType.Name);
                        i++;
                    }
                }
               
            }
            
            if (rc.EnclosingTry == null)
                ReturnLabel = new Label(rc.CurrentMethod.Signature + "_ret");
            else ReturnLabel = rc.EnclosingTry.TryReturn;
            //// set exit loops
            //ILoop enc = rc.EnclosingLoop;
            //while (enc != null)
            //{

            //    enc.HasBreak = true;
            //    enc = enc.ParentLoop;

            //}
            return this;
        }
        public override bool Emit(EmitContext ec)
        {   bool ok=true;
            if (_expr != null )
            {
              for(int i = Expressions.Count-1; i >= 0; i--)
                  Expressions[i].EmitToStack(ec);
             
                if (!(Method.memberType.IsFloat && !Method.memberType.IsPointer))
                {
                    int ret_size = Method.memberType.GetSize(Method.memberType);
                    if (ret_size <= 2)
                        ec.EmitPop(EmitContext.A);
                    else
                        PopAllToRegister(ec, EmitContext.BP, ret_size,Method.LastParameterEndIdx);
                }
            }
            ec.EmitInstruction(new Vasm.x86.Jump() { DestinationLabel = this.ReturnLabel.Name });
            return ok;
        }
      
     
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
      
            if (_expr != null)
                _expr.DoFlowAnalysis(fc);
            return FlowState.Unreachable;
        }
    }
    
	
}