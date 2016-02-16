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
                   
                  
                    // backup
                    rc.CreateNewState();


                    if ((Right is VariableExpression) || (Right is DeclaredExpression && (Right as DeclaredExpression).Expression is VariableExpression))
                        rc.CurrentGlobalScope |= ResolveScopes.VariableExtensionAccess;
                    else
                        rc.CurrentGlobalScope |= ResolveScopes.MethodExtensionAccess;

                    rc.CurrentExtensionLookup = LeftType.Type;
                    rc.StaticExtensionLookup = true;

                    Right = (Expr)Right.DoResolve(rc);
                   
                    // restore
                    rc.RestoreOldState();

                    return Right;
                }
            }
            else if (Namespace != null) // NS::Value
            {
                // backup
                rc.CreateNewState();
                if (Namespace.Name == "global")
                    ResolveContext.Report.Error(0, Location, "Global namespace cannot be used for access");
                
                Namespace ns = rc.Resolver.ResolveNS(Namespace.Name);

                if (Namespace.Default == ns && Namespace.Name != "global")
                {
                    // check child
                    ns = rc.Resolver.ResolveNS(rc.CurrentNamespace.Name + "::" + Namespace.Name);
                    Namespace = ns;

                }

                  if (Namespace.Default == Namespace)
                        ResolveContext.Report.Error(0, Location, "Unknown namespace");

                rc.CurrentGlobalScope |= ResolveScopes.ByNameAccess;
                rc.CurrentNamespace = Namespace;

                Right = (Expr)Right.DoResolve(rc);

               
                // restore
                rc.RestoreOldState();

                return Right;
            }

          
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
