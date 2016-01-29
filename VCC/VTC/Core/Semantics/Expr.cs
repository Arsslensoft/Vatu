using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class Expr : SimpleToken, IEmit, IEmitExpr, IFlowAnalysis
    {
        public bool AcceptStatement = false;
        public Expr current;
        [Rule("<Expression> ::= <Op Assign>")]
        public Expr(Expr expr)
        {
            loc = expr.Location;
            current = expr;

        }



        protected TypeSpec type;

        public TypeSpec Type
        {
            get
            {
                if (type != null && type.IsTypeDef)
                    return type.GetTypeDefBase(type);
                else return type;
            }
            set
            {
                type = value;
            }
        }


        public Expr(TypeSpec tp, Location lc)
        {
            type = tp;
            loc = lc;
        }
        public Expr(Location lc)
        {
            type = null;
            loc = lc;
        }
        public Expr()
            : this(Location.Null)
        {

        }
        public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;
            if (current != null)
                ok &= current.Resolve(rc);


            return ok;
        }
        public virtual bool Emit(EmitContext ec)
        {
            if (current != null)
                current.Emit(ec);

            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (current != null)
            {
                current = (Expr)current.DoResolve(rc);
                Type = current.Type;


                return current;
            }

            return this;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {


            return current.EmitToStack(ec);

        }
        public virtual bool EmitFromStack(EmitContext ec)
        {


            return current.EmitFromStack(ec);
        }
        public virtual bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            return current.EmitToRegister(ec, rg);
        }
        public virtual bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return current.EmitBranchable(ec, truecase, v);
        }

        public virtual string CommentString()
        {
            return "";
        }
        public virtual bool DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (current != null)
                return current.DoFlowAnalysis(fc);
            return true;
        }
    }
}
