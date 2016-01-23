using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    /// <summary>
    /// Variable Expr
    /// </summary>
    public class VariableExpression : Identifier
    {

        public MemberSpec variable;
        /// <summary>
        /// ByVal ccess operator
        /// </summary>
        /// <param name="ms"></param>
        public VariableExpression(MemberSpec ms)
            : base(ms.Name)
        {
            Type = ms.MemberType;
            variable = ms;
        }

        [Rule(@"<Var Expr>       ::= Id")]
        public VariableExpression(Identifier id)
            : base(id.Name)
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (variable == null)
            {

                variable = rc.Resolver.TryResolveVar(Name);
                if (variable == null)
                    variable = rc.Resolver.TryResolveEnumValue(Name);

                if (variable == null)
                    variable = rc.Resolver.ResolveParameter(Name);

                if (variable == null)
                    variable = rc.Resolver.TryResolveField(Name);

                if (variable != null)
                {
                    if (variable is VarSpec)
                        Type = ((VarSpec)variable).MemberType;
                    else if (variable is FieldSpec)
                        Type = ((FieldSpec)variable).MemberType;
                    else if (variable is EnumMemberSpec)
                        Type = ((EnumMemberSpec)variable).MemberType;
                    else if (variable is ParameterSpec)
                        Type = ((ParameterSpec)variable).MemberType;
                }

                if (variable == null && (rc.CurrentScope & ResolveScopes.AccessOperation) != ResolveScopes.AccessOperation)
                    ResolveContext.Report.Error(14, Location, "Unresolved variable '" + Name + "'");
            }
            base.DoResolve(rc);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            variable = rc.Resolver.TryResolveVar(_idName);
            return base.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            //TODO:Non builtin
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (variable is VarSpec)
                variable.EmitFromStack(ec);
            else if (variable is FieldSpec)
                variable.EmitFromStack(ec);
            else if (variable is ParameterSpec)
                variable.EmitFromStack(ec);
            else if (variable is RegisterSpec)
                variable.EmitFromStack(ec);
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {


            if (variable is VarSpec)
                variable.EmitToStack(ec);
            else if (variable is FieldSpec)
                variable.EmitToStack(ec);
            else if (variable is EnumMemberSpec)
                variable.EmitToStack(ec);
            else if (variable is ParameterSpec)
                variable.EmitToStack(ec);
            else if (variable is RegisterSpec)
                variable.EmitToStack(ec);
            return true;
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });

            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            return true;
        }
        public override string CommentString()
        {
            if (variable != null)
                return variable.Name;
            else return Name;
        }
    }
}
