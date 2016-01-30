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
    /// Method Expression
    /// </summary>
    public class MethodExpression : Expr
    {
        public MethodSpec Method;
        public List<Expr> Parameters { get; set; }
        bool isdelegate = false;
        protected Identifier _id;
        protected ParameterSequence<Expr> _param;
        [Rule(@"<Method Expr>       ::= Id ~'(' <PARAM EXPR> ~')'")]
        public MethodExpression(Identifier id, ParameterSequence<Expr> expr)
        {
            _id = id;
            _param = expr;
        }

        MemberSpec DelegateVar;
        bool ResolveDelegate(ResolveContext rc)
        {
            MemberSpec r = rc.Resolver.TryResolveName(_id.Name);
            if (r is MethodSpec)
                return false;
            else if (r != null)
            {
                if (!r.MemberType.IsDelegate)
                    ResolveContext.Report.Error(0, Location, "Only delegate can be called with parameters");
                else
                {
                    isdelegate = true;
                    if (MatchParameterTypes(r.MemberType as DelegateTypeSpec))
                    {
                        DelegateVar = r;
                        return true;
                    }
                    else { ResolveContext.Report.Error(0, Location, "Delegate parameters mismatch"); return false; }
                }
            }
            return false;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (Method != null)
                fc.MarkAsUsed(Method);
       
            if(Parameters != null)
            {        FlowState ok = FlowState.Valid;
                foreach(Expr p in Parameters)
                     ok &= p.DoFlowAnalysis(fc);

                return ok & base.DoFlowAnalysis(fc);
            }

            if (DelegateVar != null)
                fc.MarkAsUsed(DelegateVar);
            return base.DoFlowAnalysis(fc);
        }
       
       public override bool Resolve(ResolveContext rc)
        {

            bool ok = true;
            if (_param != null)
                ok &= _param.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            ccvh = new CallingConventionsHandler();
            List<TypeSpec> tp = new List<TypeSpec>();
            Parameters = new List<Expr>();
            AcceptStatement = true;
            if (_param != null)
            {
                foreach (Expr p in _param)
                {
                    Expr e = (Expr)p.DoResolve(rc);
                    Parameters.Add(e);
                    tp.Add(e.Type);

                }

            }

            MemberSignature msig = new MemberSignature();
            if (tp.Count > 0)
            {
                if (!ResolveDelegate(rc))
                    rc.Resolver.TryResolveMethod(_id.Name,ref Method, tp.ToArray());
                else
                {
                    Type = (DelegateVar.MemberType as DelegateTypeSpec).ReturnType;
                    return this;
                }

                if (Method == null)
                    msig = new MemberSignature(rc.CurrentNamespace, _id.Name, tp.ToArray(), loc);

            }
            else
            {
                if (!ResolveDelegate(rc))
                 rc.Resolver.TryResolveMethod(_id.Name,ref Method);
                else
                {
                    Type = (DelegateVar.MemberType as DelegateTypeSpec).ReturnType;
                    return this;
                }
                if (Method == null)
                    msig = new MemberSignature(rc.CurrentNamespace, _id.Name, tp.ToArray(), loc);
            }
            if ((rc.CurrentScope & ResolveScopes.AccessOperation) == ResolveScopes.AccessOperation && rc.CurrentExtensionLookup != null)
            {
                if (Method == null)
                    ResolveContext.Report.Error(46, Location, "Unresolved extension method");
                else if (rc.ExtensionVar != null && Parameters.Count < Method.Parameters.Count)
                    Parameters.Insert(0,rc.ExtensionVar);
                else if (!rc.StaticExtensionLookup)
                    ResolveContext.Report.Error(46, Location, "Extension methods must be called with less parameters by 1, the first is reserved for the extended type");
            }
      
                if (Method == null)
                    ResolveContext.Report.Error(46, Location, "Unknown method " + msig.NormalSignature + " ");
                else if (Method.Parameters.Count != Parameters.Count)
                    ResolveContext.Report.Error(46, Location, "the method " + Method.Name + " has different parameters");
                else if (!MatchParameterTypes(rc))
                    ResolveContext.Report.Error(46, Location, "the method " + Method.Name + " has different parameters types. try casting");
            
            if (Method != null)
                Type = Method.MemberType;
            return this;
        }
        bool MatchParameterTypes(ResolveContext rc)
        {
            for (int i = 0; i < Method.Parameters.Count; i++)
                if (!TypeChecker.CompatibleTypes(Parameters[i].Type, Method.Parameters[i].MemberType))
                    return false;
                else if (Method.Parameters[i].IsReference)
                {
                    LoadEffectiveAddressOp lea = new LoadEffectiveAddressOp();
                    lea.loc = Parameters[i].loc;
                    Parameters[i] = new UnaryOperation(Parameters[i], lea);
                    Parameters[i].loc = lea.loc;
                    Parameters[i] = (Expr)Parameters[i].DoResolve(rc);

                }

            return true;
        }
        bool MatchParameterTypes(DelegateTypeSpec t)
        {
            for (int i = 0; i < t.Parameters.Count; i++)
                if (!TypeChecker.CompatibleTypes(Parameters[i].Type, t.Parameters[i]))
                    return false;

            return true;
        }
        CallingConventionsHandler ccvh;
        public override bool Emit(EmitContext ec)
        {
            if (isdelegate && DelegateVar != null)
            {
                DelegateVar.EmitToStack(ec);
                ec.EmitPop(RegistersEnum.BX);
                ccvh.EmitCall(ec, Parameters, DelegateVar, RegistersEnum.BX, (DelegateVar.MemberType as DelegateTypeSpec).CCV);
            }
            else
                ccvh.EmitCall(ec, Parameters, Method);


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            Emit(ec);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            throw new NotImplementedException();
            //     return base.EmitFromStack(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            Emit(ec);
            ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });
            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            return true;
        }
        public override string CommentString()
        {
            return _id.Name + ((_param == null) ? "()" : "(" + ")");
        }
    }
}
