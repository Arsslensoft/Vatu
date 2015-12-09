using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCC
{

    // START STMT

    public class Block : Statement
    {
        private NormalStatment _statements;
        [Rule("<Block>     ::= ~'{' <Stm List> ~'}' ")]
        public Block(NormalStatment stmt)
        {
            _statements = stmt;
        }
    }

    public class Case : Statement
    {
        private NormalStatment _statements;
        private Expr _val;
        private Case _next_case;

        [Rule("<Case Stms> ::= ~case <Value> ~':' <Stm List> <Case Stms>")]
        public Case(Expr val, NormalStatment stmt, Case nxt)
        {
            _statements = stmt;
            _val = val;
            _next_case = nxt;
        }
     
        // default
        [Rule("<Case Stms> ::= ~default ~':' <Stm List>   ")]
        public Case(NormalStatment stmt)
        {
            _statements = stmt;
            _val = null;
            _next_case = null;
        }

         [Rule("<Case Stms> ::= ")]
        public Case()
        {
            _statements = null;
            _val = null;
        }

    }

    // <Normal Stm>

    public class DoWhile : NormalStatment
    {
        private BaseStatement _statements;
        private Expr _cnd;

        [Rule("<Normal Stm> ::= ~do <Statement> ~while ~'(' <Expression> ~')'")]
        public DoWhile(BaseStatement stmt, Expr whilecnd)
        {
            _statements = stmt;
            _cnd = whilecnd;
        }
    }

    public class Switch : NormalStatment
    {
        private Case _cases;
        private Expr _expr;

        [Rule("<Normal Stm> ::= ~switch ~'(' <Expression> ~')' ~'{' <Case Stms> ~'}'")]
        public Switch( Expr sw, Case cases)
        {
            _cases = cases;
            _expr = sw;
        }
    }

    public class BlockStatement : NormalStatment
    {
  
        private Block _bloc;

        [Rule("<Normal Stm> ::= <Block>")]
        public BlockStatement(Block b)
        {
            _bloc = b;
       
        }
    }

    public class ExpressionStatement : NormalStatment
    {

        private Expr _expr;

        [Rule("<Normal Stm> ::= <Expression> ~';' ")]
        public ExpressionStatement(Expr b)
        {
            _expr = b;

        }
    }

    public class GotoStatement : NormalStatment
    {

        private Identifier _id;

        [Rule("<Normal Stm> ::= ~goto Id ~';'")]
        public GotoStatement(Identifier id)
        {
            _id = id;

        }
    }
    public class ContinueStatement : NormalStatment
    {

        [Rule("<Normal Stm> ::= ~continue ~';'")]
        public ContinueStatement()
        {

        }
    }
    public class BreakStatement : NormalStatment
    {

        [Rule("<Normal Stm> ::= ~break ~';'")]
        public BreakStatement()
        {

        }
    }

    public class ReturnStatement : NormalStatment
    {

        private Expr _expr;

        [Rule("<Normal Stm> ::= ~return <Expression> ~';' ")]
        public ReturnStatement(Expr b)
        {
            _expr = b;

        }
    }
    public class EmptyStatement : NormalStatment
    {



        [Rule("<Normal Stm> ::= ~';'")]
        public EmptyStatement()
        {
         

        }
    }

    public class NormalStatment : Statement
    {
        private Statement _next;
        private Statement current;

      
       [Rule("<Stm List>  ::=  <Statement> <Stm List> ")]
        public NormalStatment(Statement stm, Statement next)
        {
            current = stm;
            _next = next;

        }
       [Rule("<Stm List>  ::=  ")]
       public NormalStatment()
           :
           this(null,null)
       {
       

       }
    }
    // <Then Stm>
    public class IfThenStatement : ThenStatement
    {
        Expr _expr;
        ThenStatement _ifstmt;
        ThenStatement _elsestmt;

        [Rule("<Then Stm>   ::= ~if ~'(' <Expression> ~')' <Then Stm> ~else <Then Stm> ")]
        public IfThenStatement(Expr ifexp,ThenStatement ifstmt, ThenStatement elsestmt)
        {
            _expr = ifexp;
            _ifstmt = ifstmt;
            _elsestmt = ifstmt;

        }
    }
    public class WhileThenStatement : ThenStatement
    {
        Expr _expr;
        ThenStatement _stmt;

        [Rule("<Then Stm>   ::=  ~while ~'(' <Expression> ~')' <Then Stm>")]
        public WhileThenStatement(Expr exp, ThenStatement stmt)
        {
            _expr = exp;
            _stmt = stmt;
       

        }
    }
    public class ForThenStatement : ThenStatement
    {
        ArgumentExpression _init;
        ArgumentExpression _cond;
        ArgumentExpression _inc;
        ThenStatement _stmt;

        [Rule("<Then Stm>   ::=  ~for ~'(' <Arg> ~';' <Arg> ~';' <Arg> ~')' <Then Stm>")]
        public ForThenStatement(ArgumentExpression init, ArgumentExpression cond, ArgumentExpression inc, ThenStatement stmt)
        {
            _init = init;
            _cond = cond;
            _inc = inc;
            _stmt = stmt;


        }
    }

    public class ThenStatement : NormalStatment
    {
        public ThenStatement()
        {

        }
        [Rule(@"<Then Stm>   ::=  <Normal Stm>")]
        public ThenStatement(NormalStatment normal)
        {

        }
    }

    // <Statement> 

    /*
     <Statement>        ::= <Var Decl>
               | Id ':'                            !Label
               | if '(' <Expression> ')' <Statement>          
               | if '(' <Expression> ')' <Then Stm> else <Statement>         
               | while '(' <Expression> ')' <Statement> 
               | for '(' <Arg> ';' <Arg> ';' <Arg> ')' <Statement>
               | <Normal Stm>
      */

    public class VarDeclStatement : BaseStatement
    {
        VariableDeclaration _vadecl;
        [Rule(@"<Statement>        ::= <Var Decl>")]
        public VarDeclStatement(VariableDeclaration vardecl)
        {
            _vadecl = vardecl;
        }
    }
    public class LabelStatement : BaseStatement
    {
        Identifier _label;
        [Rule(@"<Statement> ::= Id ~':'")]
        public LabelStatement(Identifier id)
        {
            _label = id;
        }
    }

    public class IfStatement : BaseStatement
    {
        Expr _expr;
        BaseStatement _stmt;
        [Rule(@"<Statement>        ::= ~if ~'(' <Expression> ~')' <Statement>  ")]
        public IfStatement(Expr expr, BaseStatement stmt)
        {
            _expr = expr;
            _stmt = stmt;
        }
    }
    public class IfElseStatement : BaseStatement
    {
        Expr _expr;
        ThenStatement _stmt;
        BaseStatement _elsestmt;
        [Rule(@"<Statement>        ::= ~if ~'(' <Expression> ~')' <Then Stm> ~else <Statement>   ")]
        public IfElseStatement(Expr expr, ThenStatement stmt, BaseStatement elsestmt)
        {
            _expr = expr;
            _stmt = stmt;
            _elsestmt = elsestmt;
        }
    }

    public class WhileStatement : BaseStatement
    {
        Expr _expr;
        BaseStatement _stmt;

        [Rule("<Statement>    ::=  ~while ~'(' <Expression> ~')' <Statement>")]
        public WhileStatement(Expr exp, BaseStatement stmt)
        {
            _expr = exp;
            _stmt = stmt;


        }
    }
    public class ForStatement : BaseStatement
    {
        ArgumentExpression _init;
        ArgumentExpression _cond;
        ArgumentExpression _inc;
        BaseStatement _stmt;

        [Rule("<Statement>     ::= ~for ~'(' <Arg> ~';' <Arg> ~';' <Arg> ~')' <Statement>")]
        public ForStatement(ArgumentExpression init, ArgumentExpression cond, ArgumentExpression inc, BaseStatement stmt)
        {
            _init = init;
            _cond = cond;
            _inc = inc;
            _stmt = stmt;


        }
    }
    // <Statement>  
    public class BaseStatement : NormalStatment
    {
        public BaseStatement()
        {

        }
         [Rule(@"<Statement>   ::=  <Normal Stm>")]
        public BaseStatement(NormalStatment normal)
        {

        }
    }

    // 
}
