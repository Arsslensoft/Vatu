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
                    TypeSpec oldext = rc.HighPriorityExtensionLookup;
                    bool staticext = rc.HighPriorityStaticExtensionLookup;


                    rc.HighPriorityExtensionLookup = LeftType.Type;
                    rc.HighPriorityStaticExtensionLookup = true;

                    Right = (Expr)Right.DoResolve(rc);
                    if (Right is VariableExpression && (Right as VariableExpression).variable == null)
                        ResolveContext.Report.Error(0, Location, "Unresolved extended field");
                    else if (Right is MethodExpression && (Right as MethodExpression).Method == null)
                        ResolveContext.Report.Error(0, Location, "Unresolved extended method");

                    // restore
                    rc.HighPriorityExtensionLookup = oldext;
                    rc.HighPriorityStaticExtensionLookup = staticext;
                

                    rc.CurrentScope &= ~ResolveScopes.AccessOperation;
                    return Right;
                }
            }
            else if (Namespace != null) // NS::Value
            {
                Namespace ns = rc.Resolver.ResolveNS(Namespace.Name);

                if (Namespace.Default == ns && Namespace.Name != "global")
                {
                    // check child
                     ns = rc.Resolver.ResolveNS(rc.CurrentNamespace.Name+"::"+Namespace.Name);
                     if (Namespace.Default == ns)
                         ResolveContext.Report.Error(0, Location, "Global namespace cannot be used for access");

                     else Namespace = ns;

                }


                Namespace lastns = rc.HighPriorityNamespace;
               
                rc.CurrentScope |= ResolveScopes.ByNameAccess;
                rc.HighPriorityNamespace = Namespace;

                Right = (Expr)Right.DoResolve(rc);

               
                rc.HighPriorityNamespace = lastns;
                rc.CurrentScope &= ~ResolveScopes.AccessOperation;
                rc.CurrentScope &= ~ResolveScopes.ByNameAccess;
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
