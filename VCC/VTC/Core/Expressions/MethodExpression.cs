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
        bool isclass = false;
        protected Identifier _id;
        bool issuper = false;
        bool isthis = false;
        protected ParameterSequence<Expr> _param;
        [Rule(@"<Method Expr>       ::= Id ~'(' <PARAM EXPR> ~')'")]
        public MethodExpression(Identifier id, ParameterSequence<Expr> expr)
        {
            _id = id;
            _param = expr;
        }


        [Rule(@"<Method Expr>       ::= this ~'(' <PARAM EXPR> ~')'")]
        [Rule(@"<Method Expr>       ::= super ~'(' <PARAM EXPR> ~')'")]
        public MethodExpression(SimpleToken id, ParameterSequence<Expr> expr)
        {
            issuper = (id.Name == "super");
            isthis = (id.Name == "this");
            _param = expr;
        }


        MemberSpec DelegateVar;
        TypeMemberSpec tmp = null;
        int index = 0;
        AccessExpression classae = null;
        public bool ResolveClassMember(ResolveContext rc, MemberSpec mem, ref TypeMemberSpec tmp,ref int index)
        {
      

            if (!mem.MemberType.IsPointer)
            {
                if (!mem.MemberType.IsClass)
                    return false;
                else
                {
                    ClassTypeSpec stp = (ClassTypeSpec)mem.MemberType;
                    tmp = stp.ResolveMember(_id.Name);
                    if (tmp == null)
                        return false;
                    else
                    {
                        // Resolve
                        index = tmp.Index;
                        return true;
                    }

                }

            }
            else return false;


        }
        bool ResolveClassMethod(ResolveContext rc)
        {
            if (!(rc.CurrentExtensionLookup != null && rc.CurrentExtensionLookup is ClassTypeSpec && rc.ExtensionVar != null && !( rc.ExtensionVar is PolymorphicClassExpression)))
                return false;
            Expr left = rc.ExtensionVar;
            if (left is VariableExpression)
            {
                VariableExpression lv = (VariableExpression)left;
                bool ok = ResolveClassMember(rc, lv.variable, ref tmp,ref index);
                if (ok)
                {
                    if (!tmp.MemberType.IsDelegate)
                        ResolveContext.Report.Error(0, Location, "Only delegate or class members can be called with parameters");
                    else
                    {
                        isclass = true;
                        Parameters.Add(left);
                        if (!MatchParameterTypes(tmp.MemberType as DelegateTypeSpec))
                        {
                            ResolveContext.Report.Error(0, Location, "Member parameters mismatch");
                            return false;
                        }
                    }

                    if (lv.variable is VarSpec)
                    {
                        VarSpec dst = (VarSpec)lv.variable;
                        classae = new AccessExpression(dst, (left is AccessExpression) ? (left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);
                        return true;
                    }
                    else if (lv.variable is RegisterSpec)
                    {
                        RegisterSpec dst = (RegisterSpec)lv.variable;
                        classae = new AccessExpression(dst, (left is AccessExpression) ? (left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);
                        return true;
                    }
                    else if (lv.variable is FieldSpec)
                    {
                        FieldSpec dst = (FieldSpec)lv.variable;


                        classae = new AccessExpression(dst, (left is AccessExpression) ? (left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);
                        return true;
                    }
                    else if (lv.variable is ParameterSpec)
                    {
                        ParameterSpec dst = (ParameterSpec)lv.variable;
                        classae = new AccessExpression(dst, (left is AccessExpression) ? (left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);
                        return true;
                    }
                    else return false;
                }
            }
            else return false;
            return false;
        }
        bool ResolveDelegate(ResolveContext rc)
        {
            MemberSpec r = rc.Resolver.TryResolveName(_id.Name);
            rc.UnsetHighPriorityAsCurrent();
            if (r is MethodSpec)
            {
                rc.SetHighPriorityAsCurrent();
                return false;
            }
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
                        rc.SetHighPriorityAsCurrent();
                        return true;
                    }
                    else { ResolveContext.Report.Error(0, Location, "Delegate parameters mismatch");
                    rc.SetHighPriorityAsCurrent();
                        return false; }
                }
            }
            rc.SetHighPriorityAsCurrent();
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
       TypeSpec InheritedType = null;
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            ResolveScopes old = rc.CurrentScope;
            rc.CurrentScope &= ~ResolveScopes.AccessOperation;
            if (isthis || issuper && !rc.IsInClass)
                ResolveContext.Report.Error(0, Location, "This and super can be only used inside a class member declaration");
          
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

            rc.CurrentScope = old;
            rc.SetHighPriorityAsCurrent();

            MemberSignature msig = new MemberSignature();
            if (isthis)
            {
                msig = new MemberSignature(rc.CurrentNamespace,rc.CurrentType.NormalizedName + "_$ctor",tp.ToArray(),Location);
                tp.Insert(0, rc.CurrentType);
                Parameters.Insert(0, (Expr)(new ThisExpression(position).DoResolve(rc)));
                InheritedType = rc.CurrentType;
                rc.CurrentScope |= ResolveScopes.ThisAcces;
                rc.Resolver.TryResolveMethod(rc.CurrentType.NormalizedName + "_$ctor", ref Method, tp.ToArray());
                rc.CurrentScope &= ~ResolveScopes.ThisAcces;
            }
            else if (issuper)
            {
               
               
               if ((rc.CurrentType as ClassTypeSpec).ParentClass == null)
                    ResolveContext.Report.Error(0, Location, "Can't use super without having inheritance");

               InheritedType = (rc.CurrentType as ClassTypeSpec).ParentClass;
                tp.Insert(0, InheritedType);
                rc.CurrentExtensionLookup = InheritedType;
                Parameters.Insert(0, (Expr)(new SuperExpression(position).DoResolve(rc)));
                msig = new MemberSignature(rc.CurrentNamespace, InheritedType.NormalizedName + "_$ctor", tp.ToArray(), Location);
                rc.CurrentExtensionLookup = null;

                rc.CurrentScope |= ResolveScopes.SuperAccess;
                rc.Resolver.TryResolveMethod(InheritedType.NormalizedName + "_$ctor", ref Method, tp.ToArray());
                rc.CurrentScope &= ~ResolveScopes.SuperAccess;
            }
            else
            {
                if (!ResolveClassMethod(rc) && !ResolveDelegate(rc))
                    rc.Resolver.TryResolveMethod(_id.Name, ref Method, tp.ToArray());
                else if (classae != null)
                {
                    isclass = true;
                    Type = (classae.Type as DelegateTypeSpec).ReturnType;
                    rc.UnsetHighPriorityAsCurrent();
                    return this;
                }
                else
                {
                    Type = (DelegateVar.MemberType as DelegateTypeSpec).ReturnType;
                    rc.UnsetHighPriorityAsCurrent();
                    return this;
                }
                if (Method == null)
                    msig = new MemberSignature(rc.CurrentNamespace, _id.Name, tp.ToArray(), Location);

            }
            
            if ((rc.CurrentScope & ResolveScopes.AccessOperation) == ResolveScopes.AccessOperation && rc.CurrentExtensionLookup != null)
            {
             
                if (Method == null)
                    ResolveContext.Report.Error(46, Location, "Unresolved extension method");
                else if (rc.ExtensionVar != null && Parameters.Count < Method.Parameters.Count)
                {
                    if (rc.ExtensionVar.Type.Equals(rc.CurrentExtensionLookup))
                        Parameters.Insert(0, rc.ExtensionVar);
                    else
                    {
                        
                        ValueOfOp op = new ValueOfOp();
                        op.Right = rc.ExtensionVar;
                        op.CommonType = op.Right.Type.BaseType;
                        op.Right.Type = op.CommonType;
                        UnaryOperation uop = new UnaryOperation(rc.ExtensionVar, op);
                        uop.Type = op.CommonType;
                        Parameters.Insert(0,uop);
                    }
                }
                else if (!rc.StaticExtensionLookup)
                    ResolveContext.Report.Error(46, Location, "Extension methods must be called with less parameters by 1, the first is reserved for the extended type");
            }
            rc.UnsetHighPriorityAsCurrent();

                if (Method == null)
                    ResolveContext.Report.Error(46, Location, "Unknown method " + msig.NormalSignature + " ");
              
                
                else if (!Method.IsVariadic && Method.Parameters.Count != Parameters.Count)
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
                if (!TypeChecker.CompatibleTypes(Method.Parameters[i].MemberType,Parameters[i].Type))
                    return false;
                else if (Method.Parameters[i].IsReference)
                {
                    LoadEffectiveAddressOp lea = new LoadEffectiveAddressOp();
                    lea.position = Parameters[i].position;
                    Parameters[i] = new UnaryOperation(Parameters[i], lea);
                    Parameters[i].position = lea.position;
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

                ccvh.EmitCall(ec, Parameters, DelegateVar,  (DelegateVar.MemberType as DelegateTypeSpec).CCV);

                if ((DelegateVar.MemberType as DelegateTypeSpec).ReturnType.IsFloat && !(DelegateVar.MemberType as DelegateTypeSpec).ReturnType.IsPointer) // pop floating point
                    ec.EmitInstruction(new Vasm.x86.x87.FloatFree() { DestinationReg = RegistersEnum.ST0 });

            }
            else if (isclass && classae != null)
            {

                ccvh.EmitCall(ec, Parameters, classae, (tmp.MemberType as DelegateTypeSpec).CCV);

                if ((tmp.MemberType as DelegateTypeSpec).ReturnType.IsFloat && !(tmp.MemberType as DelegateTypeSpec).ReturnType.IsPointer) // pop floating point
                    ec.EmitInstruction(new Vasm.x86.x87.FloatFree() { DestinationReg = RegistersEnum.ST0 });
            }

            else
            {

                ccvh.EmitCall(ec, Parameters, Method);
                if (Method.MemberType.IsFloat && !Method.MemberType.IsPointer) // pop floating point
                    ec.EmitInstruction(new Vasm.x86.x87.FloatFree() { DestinationReg = RegistersEnum.ST0 });

            }

            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (isdelegate && DelegateVar != null)
            {
        
                ccvh.EmitCall(ec, Parameters, DelegateVar, (DelegateVar.MemberType as DelegateTypeSpec).CCV);

                if (!((DelegateVar.MemberType as DelegateTypeSpec).ReturnType.IsFloat && !(DelegateVar.MemberType as DelegateTypeSpec).ReturnType.IsPointer)) // pop floating point
                    ec.EmitPush(EmitContext.A);
               

            }
            else if (isclass)
            {
                ccvh.EmitCall(ec, Parameters, classae, (classae.Type as DelegateTypeSpec).CCV);

                if (!((tmp.MemberType as DelegateTypeSpec).ReturnType.IsFloat && !(tmp.MemberType as DelegateTypeSpec).ReturnType.IsPointer)) // pop floating point
                    ec.EmitPush(EmitContext.A);
            }
            else
            {

                ccvh.EmitCall(ec, Parameters, Method);
                if (!(Method.MemberType.IsFloat && !Method.MemberType.IsPointer)) // pop floating point
                    ec.EmitPush(EmitContext.A);

            }

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
            if (Method.MemberType.IsFloat && !Method.MemberType.IsPointer) //  floating point compare
            {
                ec.EmitInstruction(new Vasm.x86.X86.x87.FloatPushZero() );
                ec.EmitInstruction(new Vasm.x86.x87.FloatCompareAnd2Pop());
                ec.EmitInstruction(new Vasm.x86.x87.FloatStoreStatus() { DestinationReg = EmitContext.A });
                ec.EmitInstruction(new Vasm.x86.x87.FloatWait());
                ec.EmitInstruction(new Vasm.x86.x87.StoreAHToFlags());

                // jumps
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.NotEqual, ConditionalTestEnum.Equal);
            }
            else
            {
                ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });
                ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            }
            return true;
        }
        public override string CommentString()
        {
            return _id.Name + ((_param == null) ? "()" : "(" + ")");
        }
    }
}
