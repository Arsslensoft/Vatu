using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VTC.Core
{
    /// <summary>
    /// <Op If>
    /// </summary>
    public class IfExpression : Expr
    {
        private Expr _cond;
        private Expr _true;
        private Expr _false;

        [Rule(@"<Op If>      ::= <Op BinaryOpDef> ~'?' <Op If> ~':' <Op If>")]
        public IfExpression(Expr cnd, Expr tr, Expr fl)
        {
            _cond = cnd;
            _true = tr;
            _false = fl;
        }
#if IMPL
       [Rule(@"<Op If>      ::= <Op Or>")]
       public IfExpression(BinaryOperation cnd)
       {
           _cond = cnd;
           _true = null;
           _false = null;
       }
#endif
       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _cond.Resolve(rc);
            ok &= _true.Resolve(rc);
            ok &= _false.Resolve(rc);
            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _cond = (Expr)_cond.DoResolve(rc);
            _true = (Expr)_true.DoResolve(rc);
            _false = (Expr)_false.DoResolve(rc);
            Type = _true.Type;
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            FlowState ok = _cond.DoFlowAnalysis(fc);
            ok &= _true.DoFlowAnalysis(fc);
            ok &= _false.DoFlowAnalysis(fc);
            return ok;
        }
      

        public override bool Emit(EmitContext ec)
        {
            Label elselb = ec.DefineLabel(LabelType.IF_EXPR);
            Label iflb = ec.DefineLabel(elselb.Name + "_EXIT");
            _cond.Emit(ec);

            _true.Emit(ec);
            ec.MarkLabel(elselb);
            _false.Emit(ec);
            return true;
        }
    }
}
