using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using VJay;

[assembly: RuleTrim("<Value> ::= '(' <Expression> ')'", "<Expression>", SemanticTokenType = typeof(VCC.SimpleToken))]
namespace VCC
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


        public virtual ConstantExpression ConvertImplicitly(TypeSpec type)
        {
            if (this.type == type)
                return this;

            // TODO CONSTANT CONVERSION
            return null;
        }

    /*  public static Constant CreateConstantFromValue(TypeSpec t, object v, Location loc)
        {
            switch (t.BuiltinType)
            {
                case BuiltinTypeSpec.Type.Int:
                    return new IntConstant(t, (int)v, loc);
                case BuiltinTypeSpec.Type.String:
                    return new StringConstant(t, (string)v, loc);
                case BuiltinTypeSpec.Type.UInt:
                    return new UIntConstant(t, (uint)v, loc);
                case BuiltinTypeSpec.Type.Long:
                    return new LongConstant(t, (long)v, loc);
                case BuiltinTypeSpec.Type.ULong:
                    return new ULongConstant(t, (ulong)v, loc);
                case BuiltinTypeSpec.Type.Float:
                    return new FloatConstant(t, (float)v, loc);
                case BuiltinTypeSpec.Type.Double:
                    return new DoubleConstant(t, (double)v, loc);
                case BuiltinTypeSpec.Type.Short:
                    return new ShortConstant(t, (short)v, loc);
                case BuiltinTypeSpec.Type.UShort:
                    return new UShortConstant(t, (ushort)v, loc);
                case BuiltinTypeSpec.Type.SByte:
                    return new SByteConstant(t, (sbyte)v, loc);
                case BuiltinTypeSpec.Type.Byte:
                    return new ByteConstant(t, (byte)v, loc);
                case BuiltinTypeSpec.Type.Char:
                    return new CharConstant(t, (char)v, loc);
                case BuiltinTypeSpec.Type.Bool:
                    return new BoolConstant(t, (bool)v, loc);
                case BuiltinTypeSpec.Type.Decimal:
                    return new DecimalConstant(t, (decimal)v, loc);
            }

            if (t.IsEnum)
            {
                var real_type = EnumSpec.GetUnderlyingType(t);
                return new EnumConstant(CreateConstantFromValue(real_type, v, loc), t);
            }

            if (v == null)
            {
                if (t.IsNullableType)
                    return Nullable.LiftedNull.Create(t, loc);

                if (TypeSpec.IsReferenceType(t))
                    return new NullConstant(t, loc);
            }

#if STATIC
			throw new InternalErrorException ("Constant value `{0}' has unexpected underlying type `{1}'", v, t.GetSignatureForError ());
#else
            return null;
#endif
        }*/
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
   }
   /// <summary>
    /// Variable Expr
    /// </summary>
   public class VariableExpression : Identifier
   {

       [Rule(@"<Value>       ::= Id")]
       public VariableExpression(Identifier id)
           : base(id.Name)
       {
        
       }
    
   }

    // TO DO COMPLETE
   /// <summary>
   /// Variable Expr
   /// </summary>
   public class ValueExpression : Expr
   {

       public ValueExpression(Identifier id)
          
       {

       }

   }

   

    // a.b | a->b | a[5]
    /// <summary>
    /// Access Op
    /// </summary>
   public class AccessOperation : Expr
   {
       private readonly Expr _left;
       private readonly AccessOp _op;
       private readonly Expr _right;
       [Rule(@"<Op Pointer> ::= <Op Pointer> '.' <Value>")]
       [Rule(@"<Op Pointer> ::= <Op Pointer> '->' <Value>")]
       public AccessOperation(Expr left,AccessOp op, Expr target)
       {
           _left = left;
           _right = target;
           _op = op;

       }


       [Rule(@"<Op Pointer> ::= <Op Pointer> ~'[' <Expression> ~']'")]
       public AccessOperation(Expr left, Expr target)
       {
           _left = left;
           _right = target;
           _op = new ByIndexOperator();

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
   }

   /// <summary>
   /// Handle all unary operators
   /// </summary>
   public class UnaryOperation : Expr
   {
       private readonly Expr _target;
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
           _target = target;
           _op = op;
     
       }

       // Postfix
       [Rule(@"<Op Unary>   ::= <Op Pointer> '--'")]
       [Rule(@"<Op Unary>   ::= <Op Pointer> '++'")]
       public UnaryOperation(Expr target,UnaryOp op)
       {
           _target = target;
           _op = op;

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
       protected readonly Expr _left;
       protected readonly Expr _right;
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
           _left = left;
           _op = op;
           _right = right;
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
       private readonly Expr _cond;
       private readonly Expr _true;
       private readonly Expr _false;

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

   }

   /// <summary>
   /// <Op Assign>
   /// </summary>
   public class AssignExpression : Expr
   {
       Expr _src;
       AssignOp _op;
       Expr _target;

       public Expr Source { get { return _src; } }
       public Expr Target { get { return _target; } }

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
           _src = src;
           _op = op;
           _target = target;
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
       [Rule(@"<Arg>       ::= <Expression> ")]
       public ArgumentExpression(Expr expr)
       {

       }

       [Rule(@"<Arg>       ::= ")]
       public ArgumentExpression()
       {

       }

   }
}
