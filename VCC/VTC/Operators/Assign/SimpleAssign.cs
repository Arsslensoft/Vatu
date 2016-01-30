using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    [Terminal("=")]
    public class SimpleAssignOperator : AssignOp
    {
      
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            /*  if (Left.Type.IsStruct)
                   ResolveContext.Report.Error(44, Location, "Cannot assign struct values");*/

            rc.Resolver.TryResolveMethod("Op_implicit_Cast_" + Left.Type.NormalizedName, ref OvlrdOp, new TypeSpec[1] { Right.Type });

            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            if (OvlrdOp != null)
                return this;


                if (Left.Type.IsEnum)
                    ResolveContext.Report.Error(44, Location, "Cannot assign enum values outside it's declaration");

                if (!TypeChecker.CompatibleTypes(Left.Type, Right.Type) && !FixConstant(rc))
                    ResolveContext.Report.Error(35, Location, "Source and target must have same types");

                if (Left.Type is ArrayTypeSpec)
                    ResolveContext.Report.Error(35, Location, "Cannot assign array variables, use intermediate pointers instead");
           
            return this;
        }
        public override bool Emit(EmitContext ec)
        {


            if (OvlrdOp != null)
                EmitOverrideOperator(ec);
            else
                Right.EmitToStack(ec);
            ec.EmitComment(Left.CommentString() + Name + Right.CommentString());
            Left.EmitFromStack(ec);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (OvlrdOp != null)
                EmitOverrideOperator(ec);

            return Left.EmitFromStack(ec);
        }
    }
}
