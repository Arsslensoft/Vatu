using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    [Terminal("::")]
    public class ByNameOperator : AccessOp
    {
        public ByNameOperator()
        {
            Register = RegistersEnum.AX;
            _op = AccessOperator.ByName;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            
            if (LeftType != null)
            {
                LeftType = (TypeToken)LeftType.DoResolve(rc);
                if (LeftType.Type == null)
                    ResolveContext.Report.Error("Failed to resolve type");
                else
                {
                    // back up 
                    TypeSpec oldext = rc.CurrentExtensionLookup;
                    bool staticext = rc.StaticExtensionLookup;
                    rc.ResolverStack.Push(new ResolveState(rc.CurrentNamespace, rc.CurrentExtensionLookup, rc.StaticExtensionLookup));

                    rc.CurrentExtensionLookup = LeftType.Type;
                    rc.StaticExtensionLookup = true;
                    Right = (Expr)Right.DoResolve(rc);
                    if (Right is VariableExpression && (Right as VariableExpression).variable == null)
                        ResolveContext.Report.Error(0, Location, "Unresolved extended field");
                    else if (Right is MethodExpression && (Right as MethodExpression).Method == null)
                        ResolveContext.Report.Error(0, Location, "Unresolved extended method");
                    // restore
                    rc.CurrentExtensionLookup = oldext;
                    rc.StaticExtensionLookup = staticext;
                    rc.ResolverStack.Pop();

                    rc.CurrentScope &= ~ResolveScopes.AccessOperation;
                    return Right;
                }
            }
            else if (Namespace != null) // NS::Value
            {
                Namespace lastns = rc.CurrentNamespace;
                rc.ResolverStack.Push(new ResolveState(rc.CurrentNamespace, rc.CurrentExtensionLookup, rc.StaticExtensionLookup));

                rc.CurrentNamespace = Namespace;

                Right = (Expr)Right.DoResolve(rc);

                rc.ResolverStack.Pop();
                rc.CurrentNamespace = lastns;
                rc.CurrentScope &= ~ResolveScopes.AccessOperation;
                return Right;
            }

            rc.CurrentScope &= ~ResolveScopes.AccessOperation;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            return Right.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return Right.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Right.EmitToStack(ec);
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            return Right.EmitToRegister(ec, rg);
        }
    }
}
