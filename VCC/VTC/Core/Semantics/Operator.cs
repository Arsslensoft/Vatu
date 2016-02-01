using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public abstract class Operator : SimpleToken, IEmit, IEmitExpr
    {
        public bool FloatingPointSupported = false;
        public Namespace Namespace { get; set; }
        public Expr Left { get; set; }
        public Expr Right { get; set; }

        public bool FixConstant(ResolveContext rc)
        {
            bool conv = false;

            if (Left.Type.IsFloat && !FloatingPointSupported && CompilerContext.CompilerOptions.FloatingPointEnabled)
            {
                ResolveContext.Report.Error(0, Left.Location, "Floating Point not supported for this kind of operators");
                return false;
            }
            else if (Left is ConstantExpression && Right is ConstantExpression)
            {
                // greater conversion
                if (Left.Type.Size > Right.Type.Size)
                {
                    Right = (Right as ConstantExpression).ConvertImplicitly(rc, Left.Type, ref conv);
                    return conv;
                }
                else if (Left.Type.Size < Right.Type.Size)
                {
                    Left = (Left as ConstantExpression).ConvertImplicitly(rc, Right.Type, ref conv);
                    return conv;
                }
                else return (Left.Type.Equals(Right.Type));
            }

            else if (Left is ConstantExpression)
            {
                Left = (Left as ConstantExpression).ConvertImplicitly(rc, Right.Type, ref conv);
                return conv;

            }
            else if (Right is ConstantExpression)
            {
                Right = (Right as ConstantExpression).ConvertImplicitly(rc, Left.Type, ref conv);
                return conv;
            }
            else if (Left == null || Right == null)
            {
                ResolveContext.Report.Error(0, Location, "An operation member is null");
                conv = false;
                return false;
            }
            else
                return (TypeChecker.CompatibleTypes(Left.Type, Right.Type));
        }
        public TypeSpec CommonType { get; set; }

        public void UnaryCheck(ResolveContext rc)
        {
            if (Right.Type.IsFloat && !FloatingPointSupported && CompilerContext.CompilerOptions.FloatingPointEnabled)
                ResolveContext.Report.Error(0, Right.Location, "Floating Point not supported for this kind of operators");
        
        }

        public Operator()
        {
            loc = CompilerContext.TranslateLocation(position);
        }
       
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
       public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {


            return true;

        }
        public virtual bool EmitFromStack(EmitContext ec)
        {


            return true;
        }
        public virtual bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            return true;
        }
        public virtual bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return true;
        }
        public virtual string CommentString()
        {
            if (symbol != null)
                return symbol.Name;
            else return "";
        }
    }
}
