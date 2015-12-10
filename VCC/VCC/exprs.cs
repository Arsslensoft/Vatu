using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;


[assembly: RuleTrim("<Value> ::= '(' <Expression> ')'", "<Expression>", SemanticTokenType = typeof(VCC.Core.SimpleToken))]
namespace VCC.Core
{
   public class ConstantExpression : Expr
    {

       public ConstantExpression(TypeSpec type, Location loc)
           : base(type, loc)
       {

       }


 

        /// <summary>
        ///  This is used to obtain the actual value of the literal
        ///  cast into an object.
        /// </summary>
       public virtual object GetValue()
       {
           return null;
       }


        public virtual ConstantExpression ConvertImplicitly(ResolveContext rc,TypeSpec type)
        {
            if (this.type == type)
                return this;

            if (!type.IsBuiltinType)
            {
                rc.Report.Error("Cannot convert type " +this. type.Name + " to " + type.Name);
                return null;
            }
            if(type.IsPointer && (this.type != BuiltinTypeSpec.Int && this.type != BuiltinTypeSpec.Short))
            {
                rc.Report.Error("Cannot convert pointer type " + this.type.Name + " to " + type.Name);
                return null;
            }
      
            return ConstantExpression.CreateConstantFromValue(type,this.GetValue(), loc);
        }

     public static ConstantExpression CreateConstantFromValue(TypeSpec t, object v, Location loc)
        {
            switch (t.BuiltinType)
            {
                case BuiltinTypes.Int:
                    return new IntConstant((int)v, loc);
                case BuiltinTypes.String:
                    return new StringConstant((string)v, loc);
                case BuiltinTypes.UInt:
                    return new UIntConstant((uint)v, loc);
                case BuiltinTypes.Long:
                    return new LongConstant((long)v, loc);
                case BuiltinTypes.ULong:
                    return new ULongConstant((ulong)v, loc);
                case BuiltinTypes.Float:
                    return new FloatConstant((float)v, loc);
                case BuiltinTypes.Double:
                    return new DoubleConstant((double)v, loc);
                case BuiltinTypes.Short:
                    return new ShortConstant((short)v, loc);
                case BuiltinTypes.UShort:
                    return new UShortConstant((ushort)v, loc);
                case BuiltinTypes.Byte:
                    return new ByteConstant((sbyte)v, loc);
                case BuiltinTypes.Char:
                    return new CharConstant((byte)v, loc);
                case BuiltinTypes.Bool:
                    return new BoolConstant((bool)v, loc);
      
            }

          
            return null;

        }
    }

   // START SEMANTIC IMPLEMENTATION
    /// <summary>
    /// Method Expression
    /// </summary>
   /*
    Id '(' <Expression> ')'
    Id '(' ')'     
     */
   public class MethodExpression : Expr
   {
       public MethodSpec Method { get; set; }

       protected Identifier _id;
       protected Expr _param;
       [Rule(@"<Value>       ::= Id ~'(' <Expression> ~')'")]
       public MethodExpression(Identifier id, Expr expr)
       {
           _id = id;
           _param = expr;
       }
       [Rule(@"<Value>       ::= Id ~'(' ~')'")]
       public MethodExpression(Identifier id)
       {
           _id = id;
           _param = null;
       }

       public override bool Resolve(ResolveContext rc)
       {
           Method = rc.ResolveMethod(_id.Name);
           return base.Resolve(rc);
       }
       public override SimpleToken DoResolve(ResolveContext rc)
       {
           return this            ;
       }
   }

   /// <summary>
    /// Variable Expr
    /// </summary>
   public class VariableExpression : Identifier
   {
       VarSpec variable;

       [Rule(@"<Value>       ::= Id")]
       public VariableExpression(Identifier id)
           : base(id.Name)
       {
        
       }
       public override SimpleToken DoResolve(ResolveContext rc)
       {
           return this;
       }
       public override bool Resolve(ResolveContext rc)
       {
          variable= rc.ResolveVar(_idName);
           return base.Resolve(rc);
       }
   }


    // a.b | a->b | a[5]
    /// <summary>
    /// Access Op
    /// </summary>
   public class AccessOperation : Expr
   {
       

       private readonly AccessOp _op;
       [Rule(@"<Op Pointer> ::= <Op Pointer> '.' <Value>")]
       [Rule(@"<Op Pointer> ::= <Op Pointer> '->' <Value>")]
       public AccessOperation(Expr left,AccessOp op, Expr target)
       {
           _op = op;
           _op.Left = left;
           _op.Right = target;
      

       }


       [Rule(@"<Op Pointer> ::= <Op Pointer> ~'[' <Expression> ~']'")]
       public AccessOperation(Expr left, Expr target)
       {
           _op = new ByIndexOperator();
           _op.Left = left;
           _op.Right = target;
          

       }
#if IMPL
       [Rule(@"<Op Pointer> ::=  <Value>")]
       public AccessOperation(Expression target)
           
       {
           _left = null;
           _op = null;
           _right = target;
         

       }
#endif

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           _op.Right = (Expr)_op.Right.DoResolve(rc);
           _op.Left = (Expr)_op.Left.DoResolve(rc);

           return this;
       }
       public override bool Resolve(ResolveContext rc)
       { 
           bool ok =          _op.Left.Resolve(rc);
          ok &= _op.Right.Resolve(rc);

           return base.Resolve(rc) && ok;
       }
   }

   /// <summary>
   /// Handle all unary operators
   /// </summary>
   public class UnaryOperation : Expr
   {

       private readonly Operator _op;

       [Rule(@"<Op Unary>   ::= '!'    <Op Unary>")]
       [Rule(@"<Op Unary>   ::= '~'    <Op Unary>")]
       [Rule(@"<Op Unary>   ::= '-'    <Op Unary>")]
       [Rule(@"<Op Unary>   ::= '*'    <Op Unary>")]
       [Rule(@"<Op Unary>   ::= '&'    <Op Unary>")]
       [Rule(@"<Op Unary>   ::= '--'   <Op Unary>")]
       [Rule(@"<Op Unary>   ::= '++'   <Op Unary>")]
       public UnaryOperation(Operator op, Expr target)
       {
           _op = op;
           _op.Right = target;
    
     
       }

       // Postfix
       [Rule(@"<Op Unary>   ::= <Op Pointer> '--'")]
       [Rule(@"<Op Unary>   ::= <Op Pointer> '++'")]
       public UnaryOperation(Expr target,UnaryOp op)
       {
           _op = op;
           _op.Right = target;

       }

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           
           _op.Right = (Expr)_op.Right.DoResolve(rc);


           return this;
       }
       public override bool Resolve(ResolveContext rc)
       {
           bool           ok = _op.Right.Resolve(rc);

           return base.Resolve(rc) && ok;
       }

#if IMPL
       [Rule(@"<Op Unary>   ::= <Op Pointer>")]
       public UnaryOperation(AccessOperation target)
       {
           _target = target;
           _op = null;

       }
#endif
   }


   /// <summary>
   /// Handle all binary operators
   /// </summary>
   public class BinaryOperation : Expr
   {

       protected readonly BinaryOp _op;





       [Rule(@"<Op Or>      ::= <Op Or> '||' <Op And>")]
       [Rule(@"<Op And>     ::= <Op And> '&&' <Op BinOR>")]
       [Rule(@"<Op BinOR>   ::= <Op BinOR> '|' <Op BinXOR>")]
       [Rule(@"<Op BinXOR>  ::= <Op BinXOR> '^' <Op BinAND>")]
       [Rule(@"<Op BinAND>  ::= <Op BinAND> '&' <Op Equate>")]
       [Rule(@"<Op Equate>  ::= <Op Equate> '==' <Op NEqual>")]
       [Rule(@"<Op NEqual>  ::= <Op NEqual> '!=' <Op Compare>")]
       [Rule(@"<Op Compare> ::= <Op Compare> '<'  <Op Shift>")]
       [Rule(@"<Op Compare> ::= <Op Compare> '>'  <Op Shift>")]
       [Rule(@"<Op Compare> ::= <Op Compare> '<=' <Op Shift>")]
       [Rule(@"<Op Compare> ::= <Op Compare> '>=' <Op Shift>")]
       [Rule(@"<Op Shift>   ::= <Op Shift> '<<' <Op Add>")]
       [Rule(@"<Op Shift>   ::= <Op Shift> '>>' <Op Add>")]
       [Rule(@"<Op Add>     ::= <Op Add> '+' <Op Mult>")]
       [Rule(@"<Op Add>     ::= <Op Add> '-' <Op Mult>")]
       [Rule(@"<Op Mult>    ::= <Op Mult> '*' <Op Unary>")]
       [Rule(@"<Op Mult>    ::= <Op Mult> '/' <Op Unary>")]
       [Rule(@"<Op Mult>    ::= <Op Mult> '%' <Op Unary>")]
       public BinaryOperation(Expr left, BinaryOp op, Expr right)
       {
           _op = op;
           _op.Left = left;

           _op.Right = right;
       }


       public override SimpleToken DoResolve(ResolveContext rc)
       {
           _op.Right = (Expr)_op.Right.DoResolve(rc);
           _op.Left = (Expr)_op.Left.DoResolve(rc);

           return this;
       }
       public override bool Resolve(ResolveContext rc)
       {
           bool ok = _op.Left.Resolve(rc);
           ok &= _op.Right.Resolve(rc);

           return base.Resolve(rc) && ok;
       }
       #if IMPL
       [Rule(@"<Op Mult>    ::= <Op Unary>")]
       public BinaryOperation(UnaryOperation left)
       {
           _left = left;
           _op = null;
           _right = null;
       }
#endif
   }

   /// <summary>
   /// <Op If>
   /// </summary>
   public class IfExpression : Expr
   {
       private Expr _cond;
       private Expr _true;
       private Expr _false;

       [Rule(@"<Op If>      ::= <Op Or> ~'?' <Op If> ~':' <Op If>")]
       public IfExpression(Expr cnd, Expr tr, Expr fl)
       {
           _cond = cnd;
           _true = tr;
           _false = fl;
       }
       #if IMPL
       [Rule(@"<Op If>      ::= <Op Or>")]
       public IfExpression(BinaryOperation cnd)
       {
           _cond = cnd;
           _true = null;
           _false = null;
       }
#endif
       public override SimpleToken DoResolve(ResolveContext rc)
       {
           _cond = (Expr)_cond.DoResolve(rc);
           _true = (Expr)_true.DoResolve(rc);
           _false = (Expr)_false.DoResolve(rc);
           return this;
       }
       public override bool Resolve(ResolveContext rc)
       {
           bool ok = _cond.Resolve(rc);
           ok &= _true.Resolve(rc);
           ok &= _false.Resolve(rc);
           return base.Resolve(rc) && ok;
       }
   }

   /// <summary>
   /// <Op Assign>
   /// </summary>
   public class AssignExpression : Expr
   {

       AssignOp _op;
  



       [Rule(@"<Op Assign>  ::= <Op If> '='   <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '+='  <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '-='  <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '*='  <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '/='  <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '^='  <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '&='  <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '|='  <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '>>=' <Op Assign>")]
       [Rule(@"<Op Assign>  ::= <Op If> '<<=' <Op Assign>")]
       public AssignExpression(Expr src, AssignOp op, Expr target)
       {
          
           _op = op;
           _op.Left = src;
           _op.Right = target;
       }


       public override SimpleToken DoResolve(ResolveContext rc)
       {
           _op.Right = (Expr)_op.Right.DoResolve(rc);
           _op.Left = (Expr)_op.Left.DoResolve(rc);

           return this;
       }
       public override bool Resolve(ResolveContext rc)
       {
           bool ok = _op.Left.Resolve(rc);
           ok &= _op.Right.Resolve(rc);

           return base.Resolve(rc) && ok;
       }
#if IMPL
       [Rule(@"<Op Assign>  ::= <Op If>")]
       public AssignExpression(IfExpression expr)
       {
           _src = expr;
           _op = null;
           _target = null;
       }
#endif
   }


   /// <summary>
   /// <Arg>
   /// </summary>
   public class ArgumentExpression : Expr
   {
       Expr argexpr;
       [Rule(@"<Arg>       ::= <Expression> ")]
       public ArgumentExpression(Expr expr)
       {
           argexpr = expr;
       }

       [Rule(@"<Arg>       ::= ")]
       public ArgumentExpression()
       {

       }

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           if(argexpr != null)
               argexpr = (Expr)argexpr.DoResolve(rc);
           

           return this;
       }
       public override bool Resolve(ResolveContext rc)
       {        if(argexpr != null){
           bool ok = argexpr.Resolve(rc);
         

           return base.Resolve(rc) && ok;
       }
       else return false;
       }

   }
}
