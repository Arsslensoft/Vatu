using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Declarations
{
    public class ExtendDeclaration : Declaration
    {
        Expr _varexpr;
        TypeToken tt;
        [Rule("<Extension Decl> ::= <Value> ~extends <Type>  ~';'")]
        public ExtendDeclaration(Expr varexp, TypeToken t)
        {
            tt = t;
            _varexpr = varexp;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            tt = (TypeToken)tt.DoResolve(rc);
            _varexpr = (Expr)_varexpr.DoResolve(rc);
            if (_varexpr is VariableExpression)
            {
                VariableExpression v = _varexpr as VariableExpression;
                if (v.variable is FieldSpec)
                {
                    if (!rc.Extend(tt.Type, (FieldSpec)v.variable))
                        ResolveContext.Report.Error(0, Location, "Another field with same signature has already extended this type.");

                }
                else ResolveContext.Report.Error(0, Location, "Only fields can be extended");
            }
            else ResolveContext.Report.Error(0, Location, "Only fields can be extended");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }

    }
}
