
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using bsn.GoldParser.Grammar;
using bsn.GoldParser.Semantic;
using System.Text;
using bsn.GoldParser.Parser;
using ExprVCC;
using System.Reflection;
using System.Linq;
using VJay;
using VCC;

 
[assembly: RuleTrim("<Value> ::= '(' <Expression> ')'", "<Expression>", SemanticTokenType = typeof (ExprVCC.SimpleToken))]
namespace ExprVCC
{

    class REPLCator
    {


        public void Run()
        {

            // grab an execution context to be used for the entire 'session'
            using (var ctx = new SimpleExecutionContext())
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ExprVCC.Simple2.cgt"))
                {
                    Banner(ctx);

                    // compile/run each statement one entry at a time
                    CompiledGrammar grammar = CompiledGrammar.Load(stream);
                    var actions = new SemanticTypeActions<SimpleToken>(grammar);

                    for (string input = ReadABit(ctx); !string.IsNullOrEmpty(input); input = ReadABit(ctx))
                    {
                        var processor = new SemanticProcessor<SimpleToken>(new StringReader(input), actions);

                        ParseMessage parseMessage = processor.ParseAll();
                        if (parseMessage == ParseMessage.Accept)
                        {

                            ctx.OutputStream.WriteLine("Ok.\n");

                            var stmts = processor.CurrentToken as Sequence<Statement>;
                            if (stmts != null)
                            {
                                foreach (Statement stmt in stmts)
                                {
                                    stmt.Execute(ctx);
                                }
                            }
                        }
                        else
                        {


                            IToken token = processor.CurrentToken;
                            ctx.OutputStream.WriteLine("At index: {0} [{1}]", token.Position.Index, parseMessage);
                            //Console.WriteLine(string.Format("{0} {1}", "^".PadLeft(token.Position.Index + 1), parseMessage));
                        }
                    }

                }


            }
        }

        // credits
        private void Banner(SimpleExecutionContext ctx)
        {
            ctx.OutputStream.WriteLine("*** SIMPLE2 REPL *** ");
            ctx.OutputStream.WriteLine(" by Dave Dolan on August 25, 2010.");
            ctx.OutputStream.WriteLine("\n -- Enter a blank line to 'go' or Ctrl+C to exit\n\n");
        }

        // reads until a blank line is entered
        private string ReadABit(SimpleExecutionContext ctx)
        {

            ctx.OutputStream.Write("> ");

            string inputLine = null;

            StringBuilder sb = new StringBuilder();

            while (string.Empty != inputLine)
            {
                inputLine = ctx.InputStream.ReadLine();
                if (!string.IsNullOrEmpty(inputLine))
                {
                    sb.AppendLine(inputLine);
                }
            }

            return sb.ToString();
        }
    }
    [Terminal("(EOF)")]
    [Terminal("(Error)")]
    [Terminal("(Whitespace)")]
    [Terminal("(")]
    [Terminal(")")]
    [Terminal("=")]
    public class SimpleToken : SemanticToken{
    }

    [Terminal("assign")]
    [Terminal("display")]
    [Terminal("do")]
    [Terminal("else")]
    [Terminal("end")]
    [Terminal("if")]
    [Terminal("read")]
    [Terminal("then")]
    [Terminal("while")]
    public class KeywordToken : SimpleToken{
    }

    public class SimpleExecutionContext : IDisposable{
        // we don't want to dispose of the console streams
        private readonly bool DoDisposeStreams;

        private Dictionary<string, object> GlobalVariables = new Dictionary<string, object>();
        public TextReader InputStream;
        public TextWriter OutputStream;

        public SimpleExecutionContext(){
            InputStream = Console.In;
            OutputStream = Console.Out;
        }

        public SimpleExecutionContext(Stream input, Stream output){
            InputStream = new StreamReader(input);
            OutputStream = new StreamWriter(output);
            DoDisposeStreams = true;
        }

        #region IDisposable Members

        public void Dispose(){
            if (DoDisposeStreams){
                OutputStream.Dispose();
                InputStream.Dispose();
            }
        }

        #endregion

        public object this[string idx]{
            get{

                var uppperValue = idx.ToUpper();
                if (GlobalVariables.ContainsKey(uppperValue)){
                    return GlobalVariables[uppperValue];
                }
                throw new Exception(uppperValue);
            }

            set{
                var upperValue = idx.ToUpper();
                GlobalVariables[upperValue] = value;
            }
        }
    }

    public abstract class Statement : SimpleToken{
        public abstract void Execute(SimpleExecutionContext ctx);
    }
    public abstract class Expression : SimpleToken, IEmitExpr, IEmit, IResolve
    {
        protected Location loc;
        protected TypeSpec type;

        public TypeSpec Type
        {
            get { return type; }
            set { type = value; }
        }
        public Location Location { get { return loc; } }

        public Expression(TypeSpec tp, Location lc)
        {
            type = tp;
            loc = lc;
        }
        public Expression(Location lc)
        {
            type = null;
            loc = lc;
        }
        public Expression()
            : this(Location.Null)
        {
     
        }
        public virtual bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public virtual bool Emit(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitFromStack(EmitContext ec)
        {
            return true;
        }
        public virtual bool EmitToStack(EmitContext ec)
        {
            return true;
        }
        public virtual Expression DoResolve(ResolveContext rc)
        {
            return null;
        }
        public abstract object GetValue(SimpleExecutionContext ctx);
    }
 /*   public abstract class Expression : SimpleToken{
        public abstract object GetValue(SimpleExecutionContext ctx);
    }
    */
    public abstract class BinaryOperator : SimpleToken{
        public abstract object Evaluate(object left, object right);
    }


    public class Negate : Expression{
        private readonly Expression computable;

        [Rule("<Negate Exp>  ::= ~'-' <Value>")]
        public Negate(SimpleToken computable){
            this.computable = (Expression)computable;
        }

        public override object GetValue(SimpleExecutionContext ctx){
            return -(Convert.ToDecimal(computable.GetValue(ctx)));
        }
    }

    [Terminal("+")]
    public class PlusOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return Convert.ToDecimal(left) + Convert.ToDecimal(right);
        }
    }

    [Terminal("-")]
    public class MinusOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return Convert.ToDecimal(left) - Convert.ToDecimal(right);
        }
    }

    [Terminal("*")]
    public class MultOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return Convert.ToDecimal(left)*Convert.ToDecimal(right);
        }
    }

    [Terminal("**")]
    public class PowerOperator : BinaryOperator
    {
        public override object Evaluate(object left, object right)
        {
            return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));
        }
    }
    [Terminal("&")]
    public class AndOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return string.Concat(Convert.ToString(left), Convert.ToString(right));
        }
    }

    [Terminal("/")]
    public class DivideOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return Convert.ToDecimal(left)/Convert.ToDecimal(right);
        }
    }

    [Terminal("==")]
    public class EqualEqualOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return ((IComparable) left).CompareTo(right) == 0;
        }
    }

    [Terminal("<>")]
    public class NotEqualOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return ((IComparable) left).CompareTo(right) != 0;
        }
    }


    [Terminal("<")]
    public class LTOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return ((IComparable) left).CompareTo(right) < 0;
        }
    }

    [Terminal("<=")]
    public class LTEOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return ((IComparable) left).CompareTo(right) <= 0;
        }
    }

    [Terminal(">")]
    public class GTOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return ((IComparable) left).CompareTo(right) > 0;
        }
    }

    [Terminal(">=")]
    public class GTEOperator : BinaryOperator{
        public override object Evaluate(object left, object right){
            return ((IComparable) left).CompareTo(right) >= 0;
        }
    }


    public class BinaryOperation : Expression{
        private readonly Expression _left;
        private readonly BinaryOperator _op;
        private readonly Expression _right;


        [Rule(@"<Expression> ::= <Expression> '>' <Add Exp>")]
        [Rule(@"<Expression> ::= <Expression> '<' <Add Exp>")]
        [Rule(@"<Expression> ::= <Expression> '<=' <Add Exp>")]
        [Rule(@"<Expression> ::= <Expression> '>=' <Add Exp>")]
        [Rule(@"<Expression> ::= <Expression> '==' <Add Exp>")]
        [Rule(@"<Expression> ::= <Expression> '<>' <Add Exp>")]
        [Rule(@"<Add Exp> ::= <Add Exp> '+' <Mult Exp>")]
        [Rule(@"<Add Exp> ::= <Add Exp> '-' <Mult Exp>")]
        [Rule(@"<Add Exp> ::= <Add Exp> '&' <Mult Exp>")]
        [Rule(@"<Mult Exp> ::= <Mult Exp> '*' <Pow Exp>")]
        [Rule(@"<Mult Exp> ::= <Mult Exp> '/' <Pow Exp>")]
        [Rule(@"<Pow Exp> ::= <Pow Exp> '**' <Negate Exp>")]
        public BinaryOperation(Expression left, BinaryOperator op, Expression right){
            _left = left;
            _op = op;
            _right = right;
        }
        
       
        public override object GetValue(SimpleExecutionContext ctx){

            object lStart = _left.GetValue(ctx);
            object rStart = _right.GetValue(ctx);

            object lFinal;
            object rFinal;

            // this attempts to determine the Greatest Common Type
            TypeChecker.GreatestCommonType gct = TypeChecker.GCT(lStart, rStart);

            // convert the values to the Greatest Common Type
            switch(gct){
                case TypeChecker.GreatestCommonType.StringType:
                    lFinal = Convert.ToString(lStart);
                    rFinal = Convert.ToString(rStart);
                    break;
                case TypeChecker.GreatestCommonType.BooleanType:
                    lFinal = Convert.ToBoolean(lStart);
                    rFinal = Convert.ToBoolean(rStart);
                    break;
                case TypeChecker.GreatestCommonType.NumericType:
                    lFinal = Convert.ToDecimal(lStart);
                    rFinal = Convert.ToDecimal(rStart);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // execute on the converted values
            return _op.Evaluate(lFinal, rFinal);
        }
    }

    [Terminal("Id")]
    public class Identifier : Expression{
        internal readonly string _idName;

        public Identifier(string idName){
            _idName = idName;
        }

        public override object GetValue(SimpleExecutionContext ctx){
            
            return ctx[_idName];
        }
    }
  
    public class MethodExpression : Expression
    {
       [Rule(@"<Value>       ::= Id ~'(' <Expression> ~')'")]
       public MethodExpression(Identifier id, Expression expr)
       {
      
       }
       public override object GetValue(SimpleExecutionContext ctx)
       {

           return 1;
       }
    }

    [Terminal("StringLiteral")]
    public class StringLiteral : Expression{
        private readonly string _value;

        public StringLiteral(string value){
            _value = value.Substring(1, value.Length - 2);
        }


        public override object GetValue(SimpleExecutionContext ctx){
            return _value;
        }
    }

    [Terminal("NumberLiteral")]
    public class NumberLiteral : Expression{
        private readonly decimal _value;

        public NumberLiteral(string value){
            _value = Convert.ToDecimal(value);
        }

        public override object GetValue(SimpleExecutionContext ctx){
            return _value;
        }
    }
    [Terminal("BooleanLiteral")]
    public class BooleanLiteral : Expression{
        private readonly bool _value;

        public BooleanLiteral(string value){
            _value = Convert.ToBoolean(value);
        }

        public override object GetValue(SimpleExecutionContext ctx){
            return _value;
        }
    }
      [Terminal("HexLiteral")]
    public class HexLiteral : Expression{
        private readonly int _value;

        public HexLiteral(string value){
            _value = Convert.ToInt32(value, 16);
        }

        public override object GetValue(SimpleExecutionContext ctx){
            return _value;
        }
    }
      [Terminal("NullLiteral")]
      public class NullLiteral : Expression
      {
          private readonly object              _value;

          public NullLiteral(string value)
          {
              _value = null;
          }

          public override object GetValue(SimpleExecutionContext ctx)
          {
              return _value;
          }
      }
    public class AssignStatement : Statement{
        private readonly Expression _expr;
        private readonly Identifier _receiver;

        [Rule(@"<Statement> ::= ~assign Id ~'=' <Expression>")]
        public AssignStatement(Identifier receiver, Expression expr){
            _receiver = receiver;
            _expr = expr;
        }

        public override void Execute(SimpleExecutionContext ctx){
            ctx[_receiver._idName] = _expr.GetValue(ctx);
        }
    }

    public class DisplayStatement : Statement{
        private readonly Expression _expr;
        private readonly Identifier _identToRead;


        [Rule(@"<Statement> ::= ~display <Expression>")]
        public DisplayStatement(Expression expr)
            : this(expr, null){
        }

        [Rule(@"<Statement> ::= ~display <Expression> ~read Id")]
        public DisplayStatement(Expression expr, Identifier identToRead){
            _expr = expr;
            _identToRead = identToRead;
        }


        public override void Execute(SimpleExecutionContext ctx){
            object outputToDisplay = _expr.GetValue(ctx);

            if (_identToRead == null){
                ctx.OutputStream.WriteLine(outputToDisplay.ToString());
            }
            else{
                ctx.OutputStream.Write("{0} \n>", outputToDisplay);
                ctx[_identToRead._idName] = ctx.InputStream.ReadLine();
            }
        }
    }

    public class WhileStatement : Statement{
        private readonly Expression _test;
        private readonly Sequence<Statement> _trueStatements;

        [Rule(@"<Statement> ::= ~while <Expression> ~do <Statements> ~end")]
        public WhileStatement(Expression test, Sequence<Statement> trueStatements){
            _test = test;
            _trueStatements = trueStatements;
        }

        public override void Execute(SimpleExecutionContext ctx){
            while (Convert.ToBoolean(_test.GetValue(ctx))){
                foreach (Statement stmt in _trueStatements){
                    stmt.Execute(ctx);
                }
            }
        }
    }

    public class IfStatement : Statement{
        private readonly Sequence<Statement> _falseStatements;
        private readonly Expression _test;
        private readonly Sequence<Statement> _trueStatements;

        [Rule(@"<Statement> ::= ~if <Expression> ~then <Statements> ~end")]
        public IfStatement(Expression _test, Sequence<Statement> trueStatements)
            : this(_test, trueStatements, null){
        }

        [Rule(@"<Statement> ::= ~if <Expression> ~then <Statements> ~else <Statements> ~end")]
        public IfStatement(Expression test, Sequence<Statement> trueStatements, Sequence<Statement> falseStatements){
            _test = test;
            _trueStatements = trueStatements;
            _falseStatements = falseStatements;
        }

        public override void Execute(SimpleExecutionContext ctx){
            if (Convert.ToBoolean(_test.GetValue(ctx))){
                foreach (Statement stmt in _trueStatements){
                    stmt.Execute(ctx);
                }
            }
            else{
                if (_falseStatements != null){
                    foreach (Statement stmt in _falseStatements){
                        stmt.Execute(ctx);
                    }
                }
            }
        }
    }


    public class Sequence<T> : SimpleToken, IEnumerable<T> where T : SimpleToken{
        private readonly T item;
        private readonly Sequence<T> next;


        public Sequence() : this(null, null){
        }

        [Rule("<Statements> ::= <Statement>", typeof (Statement))]
        public Sequence(T item) : this(item, null){
        }


        [Rule("<Statements> ::= <Statement> <Statements>", typeof (Statement))]
        public Sequence(T item, Sequence<T> next){
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator(){
            for (Sequence<T> sequence = this; sequence != null; sequence = sequence.next){
                if (sequence.item != null){
                    yield return sequence.item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        #endregion
    }

    static  class TypeChecker{
        public static bool IsBoolean(object obj){
            return (Type.GetTypeCode(obj.GetType()) == TypeCode.Boolean);
        }

        public static bool IsNumeric(object obj){
            switch(Type.GetTypeCode(obj.GetType())){
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return false;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                case TypeCode.DateTime:
                case TypeCode.String:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum GreatestCommonType{
            StringType,
            BooleanType,
            NumericType
        }

        public static GreatestCommonType GCT(params object[] toCheck){
            int boolCount = 0;
            int stringCount = 0;
            int numCount = 0;

            foreach (var obj in toCheck){
                if (IsBoolean(obj))
                    boolCount++;
                else if (IsNumeric(obj))
                    numCount++;
                else{
                    stringCount++;
                }
            }

            if (boolCount == toCheck.Length){
                return GreatestCommonType.BooleanType;
            }
            
            if (numCount == toCheck.Length){
                return GreatestCommonType.NumericType;
            }

            if(numCount > 0 && boolCount == 0){
                return GreatestCommonType.NumericType;
            }

            return GreatestCommonType.StringType;

        }

    }

    internal class Program{
        private static void GetAllTxt()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            string folderName = string.Format("{0}", executingAssembly.GetName().Name);
            foreach(string s in  executingAssembly .GetManifestResourceNames() .Where(r => r.StartsWith(folderName) && r.EndsWith(".cgt")) .ToArray())
            {
                Console.WriteLine(s + "  " + executingAssembly.GetManifestResourceStream(s) != null);

          
            }
        }
        private static void Main(string[] args){

            GetAllTxt();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ExprVCC.Simple2.cgt"))
            {
                CompiledGrammar grammar = CompiledGrammar.Load(stream);
                var actions = new SemanticTypeActions<SimpleToken>(grammar);
                try
                {
                    actions.Initialize(true);
                }
                catch (InvalidOperationException ex)
                {
                    Console.Write(ex.Message);
                    Console.ReadKey(true);
                    return;
                }


                REPLCator runner = new REPLCator();
                runner.Run();

            }
        }
    }
}
