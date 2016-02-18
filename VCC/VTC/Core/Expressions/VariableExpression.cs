using VTC.Base.GoldParser.Semantic;
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
    public class VariableExpression : Identifier, IEmitAddress
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

        public VariableExpression(string name)
            : base(name)
        {

        }
       public override bool Resolve(ResolveContext rc)
        {
            variable = rc.Resolver.TryResolveVar(_idName);
            return base.Resolve(rc);
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
                    Type = variable.memberType;
                                bool isaccess = ((rc.CurrentGlobalScope & ResolveScopes.VariableExtensionAccess) == ResolveScopes.VariableExtensionAccess) ;
                                bool isanonymous = ((rc.CurrentGlobalScope & ResolveScopes.AnonymousMethod) == ResolveScopes.AnonymousMethod);

                if (variable == null && !isaccess)
                    ResolveContext.Report.Error(14, Location, "Unresolved variable '" + Name + "'");

                if (variable != null && isanonymous)
                {
                    if (variable is VarSpec && (variable as VarSpec).MethodHost.Signature != rc.CurrentMethod.Signature)
                    {
                        ParameterSpec p = new ParameterSpec(rc.CurrentNamespace, variable.Name, rc.CurrentMethod, variable.MemberType, Location, 0, Modifiers.NoModifier);
                        rc.KnowVar(p);
                        rc.AnonymousParameters.Add(this);
                        VariableExpression v= new VariableExpression(p);
                        v.Type = p.memberType;
                        return v;
                    }
                }
                  //  ResolveContext.Report.Error(0, Location, variable.Signature.NormalSignature + " is inaccessible due to its protection level");
            }
            else Type = variable.memberType;
            base.DoResolve(rc);
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (variable != null)
            {
                if (!fc.IsAssigned(variable))
                    fc.ReportUseOfUnassigned(Location, variable.Name);
                fc.MarkAsUsed(variable);
            }
            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            //TODO:Non builtin
            return true;
        }
        public virtual bool LoadEffectiveAddress(EmitContext ec)
        {
                return variable.LoadEffectiveAddress(ec);
         
        }


        public override bool EmitFromStack(EmitContext ec)
        {
                return variable.EmitFromStack(ec);
   
        }
        public override bool EmitToStack(EmitContext ec)
        {


   
                return variable.EmitToStack(ec);
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
