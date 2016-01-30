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
        public Namespace Namespace { get; set; }
        public Expr Left { get; set; }
        public Expr Right { get; set; }

        public bool FixConstant(ResolveContext rc)
        {
            bool conv = false;
            if (Left is ConstantExpression && Right is ConstantExpression)
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
            else
                return (Left.Type.Equals(Right.Type));
        }
        public TypeSpec CommonType { get; set; }

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
