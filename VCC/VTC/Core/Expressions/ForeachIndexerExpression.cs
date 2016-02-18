using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser.Parser;

namespace VTC.Core
{
    public class ForeachIndexerExpression : VariableExpression
    {
        public ForeachIndexerExpression(LineInfo pos)
            : base("FOREACH_INDEXER")
        {
            position = pos;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            variable = rc.Resolver.TryResolveVar(Name);
            if (variable == null)
                variable = rc.Resolver.TryResolveEnumValue(Name);

            if (variable == null)
                variable = rc.Resolver.ResolveParameter(Name);

            if (variable == null)
                variable = rc.Resolver.TryResolveField(Name);

            if (variable != null)
                Type = variable.memberType;

            if (variable == null)
            {
                variable = new VarSpec(rc.CurrentNamespace, Name, rc.CurrentMethod, BuiltinTypeSpec.UInt, Location, 0, Modifiers.NoModifier);
                rc.KnowVar((VarSpec)variable);
            }
            return base.DoResolve(rc);
        }

    }
}
