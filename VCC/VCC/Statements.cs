using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VCC.Core
{
    public class BlockStatement : NormalStatment
    {

        private Block _bloc;

        [Rule("<Normal Stm> ::= <Block>")]
        public BlockStatement(Block b)
        {
            _bloc = b;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _bloc = (Block)_bloc.DoResolve(rc);
            return _bloc;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _bloc.Resolve(rc);
            return base.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            _bloc.Emit(ec);
            return base.Emit(ec);
        }
    }
    public class Block : NormalStatment
    {
        public List<NormalStatment> Statements { get; set; }


        private NormalStatment _statements;
        [Rule("<Block>     ::= ~'{' <Stm List> ~'}' ")]
        public Block(NormalStatment stmt)
        {
            _statements = stmt;
            Statements = new List<NormalStatment>();
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _statements = (NormalStatment)_statements.DoResolve(rc);
       
                NormalStatment ns = _statements;
        
                while (ns != null)
                {
                    Statements.Add(ns);
                    ns = (NormalStatment)ns._next;
                }
           
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (_statements != null)
                return _statements.Resolve(rc);
            else return true;
        }
        public override bool Emit(EmitContext ec)
        {
            bool ok = true;
            foreach (NormalStatment stmt in Statements)
                ok &= stmt.Emit(ec);
            
            return ok;
        }
    }
    public class EmptyStatement : NormalStatment
    {



        [Rule("<Normal Stm> ::= ~';'")]
        public EmptyStatement()
        {


        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return null;
        }
        public override bool Emit(EmitContext ec)
        {
             ec.EmitInstruction(new Vasm.x86.Noop());
             return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
    }
    public class NormalStatment : Statement
    {


        public Statement _next;
        public Statement current;


        [Rule("<Stm List>  ::=  <Statement> <Stm List> ")]
        public NormalStatment(Statement stm, Statement next)
        {
            current = stm;
            _next = next;

        }
        [Rule("<Stm List>  ::=  ")]
        public NormalStatment()
            :
            this(null, null)
        {


        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (current != null)
                current = (Statement)current.DoResolve(rc);
            if (_next != null)
                _next = (Statement)_next.DoResolve(rc);
           
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (current != null)
                current.Resolve(rc);
            if (_next != null)
                _next.Resolve(rc);
            return base.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            if (current != null)
                current.Emit(ec);

         
            return base.Emit(ec);
        }
    }
    public class BaseStatement : NormalStatment
    {
        NormalStatment ns;
        public BaseStatement()
        {
            ns = null;
        }
        [Rule(@"<Statement>   ::=  <Normal Stm>")]
        public BaseStatement(NormalStatment normal)
        {
            ns = normal;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
          /*  if (ns != null)
                return ns.DoResolve(rc);
            */
            return ns.DoResolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            if (ns != null)
                return ns.Emit(ec);
            return base.Emit(ec);
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (ns != null)
                return ns.Resolve(rc);
            return base.Resolve(rc);
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
        public override bool Resolve(ResolveContext rc)
        {


            return _expr.Resolve(rc);
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _expr = (Expr)_expr.DoResolve(rc);
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            return _expr.Emit(ec);
        }
    }

    #region NormalStatements
    public class ReturnStatement : NormalStatment
    {
        public Label ReturnLabel { get; set; }

        private Expr _expr;

        [Rule("<Normal Stm> ::= ~return <Expression> ~';' ")]
        public ReturnStatement(Expr b)
        {
            _expr = b;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _expr = (Expr)_expr.DoResolve(rc);
            ReturnLabel = new Label(rc.CurrentMethod.Name + "_ret");
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            bool ok = _expr.Emit(ec);
            ec.EmitInstruction(new Vasm.x86.Jump() { DestinationLabel = this.ReturnLabel.Name });
            return ok;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return _expr.Resolve(rc);
        }
    }
    public class BreakStatement : NormalStatment
    {
        string Exit {get;set;}
        [Rule("<Normal Stm> ::= ~break ~';'")]
        public BreakStatement()
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if ((rc.CurrentScope & ResolveScopes.Case) == ResolveScopes.Case)
            {
                if (rc.EnclosingIf == null)
                    ResolveContext.Report.Error(37, Location, "Break must be used inside a case statement");
                else Exit = rc.EnclosingIf.ExitIf.Name;
            }
            else  if (rc.EnclosingLoop == null)
                ResolveContext.Report.Error(37, Location, "Break must be used inside a loop statement");
            else Exit = rc.EnclosingLoop.ExitLoop.Name ;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if(Exit != null)
            ec.EmitInstruction(new Jump() { DestinationLabel = Exit });
            return true;
        }
       
    }
    public class ContinueStatement : NormalStatment
    {
        string Condition { get; set; }
        [Rule("<Normal Stm> ::= ~continue ~';'")]
        public ContinueStatement()
        {

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if ((rc.CurrentScope & ResolveScopes.Case) == ResolveScopes.Case)
            {
                if (rc.EnclosingIf == null)
                    ResolveContext.Report.Error(37, Location, "Continue must be used inside a case statement");
                else Condition = rc.EnclosingIf.Else.Name;
            }else if (rc.EnclosingLoop == null)
                ResolveContext.Report.Error(37, Location, "Continue must be used inside a loop statement");
            else Condition = rc.EnclosingLoop.LoopCondition.Name;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (Condition != null)
                ec.EmitInstruction(new Jump() { DestinationLabel = Condition });
            return true;
        }
    }
    public class GotoStatement : NormalStatment
    {
        public Label DestinationLabel { get; set; }
        public bool Switch { get; set; }
        int CaseId;
        private Identifier _id;

        [Rule("<Normal Stm> ::= ~goto Id ~';'")]
        public GotoStatement(Identifier id)
        {
            Switch = false;
            _id = id;

        }

        [Rule("<Normal Stm> ::= ~goto ~case DecLiteral ~';'")]
        public GotoStatement(DecLiteral id)
        {
            Switch = true;
            CaseId = int.Parse(id.Value.GetValue().ToString());
            if (CaseId < 0)
                CaseId = -1;
            _id = null;

        }
        [Rule("<Normal Stm> ::= ~goto ~default ~';'")]
        public GotoStatement()
        {
            Switch = true;
            CaseId = -1;
            _id = null;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if(_id != null)
               DestinationLabel = new Label(_id.Name);
            if (Switch && rc.EnclosingSwitch != null)
            {
                if (CaseId >= 0)
                   DestinationLabel= rc.EnclosingSwitch.ResolveCase(CaseId);
                else DestinationLabel = rc.EnclosingSwitch.ResolveCase(0);
            }
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitInstruction(new Vasm.x86.Jump() { DestinationLabel = this.DestinationLabel.Name });

            return true;
        }
    }
    public class NextStatement : NormalStatment
    {
        string Exit { get; set; }
        [Rule("<Normal Stm> ::= ~next ~';'")]
        public NextStatement()
        {

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (rc.EnclosingIf == null)
                ResolveContext.Report.Error(37, Location, "Next must be used inside a if statement");
            else if(rc.EnclosingIf.Else != null)
                Exit = rc.EnclosingIf.Else.Name;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (Exit != null)
                ec.EmitInstruction(new Jump() { DestinationLabel = Exit });
            return true;
        }

    }
    public class DoWhile : NormalStatment, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }


        private Statement _stmt;
        private Expr _expr;

        [Rule("<Normal Stm> ::= ~do <Statement> ~while ~'(' <Expression> ~')'")]
        public DoWhile(BaseStatement stmt, Expr whilecnd)
        {
            _stmt = stmt;
            _expr = whilecnd;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
          
            rc.CurrentScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.WHILE);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;
       

            _expr = (Expr)_expr.DoResolve(rc);

            if (_expr.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(30, Location, "Loop condition must be a boolean expression");

            _stmt = (Statement)_stmt.DoResolve(rc);

            rc.CurrentScope &= ~ResolveScopes.Loop;
            // exit current loop
            rc.EnclosingLoop = ParentLoop;
    
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return _expr.Resolve(rc) && _stmt.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            if (_expr.current is ConstantExpression || _expr is ConstantExpression)
                EmitConstantLoop(ec);
            else
            {
             

                ec.MarkLabel(EnterLoop);

                _stmt.Emit(ec);

                ec.MarkLabel(LoopCondition);
                // emit expression branchable
                _expr.EmitBranchable(ec, EnterLoop, true);
                // exit
                ec.MarkLabel(ExitLoop);
            }
            return true;
        }

        void EmitConstantLoop(EmitContext ec)
        {

            ConstantExpression ce = null;

            if (_expr is ConstantExpression)
                ce = (ConstantExpression)_expr;
            else
                ce = (ConstantExpression)_expr.current;

            bool val = (bool)ce.GetValue();
            if (val)
            { // if true


                ec.MarkLabel(EnterLoop);

                _stmt.Emit(ec);
                ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
                ec.MarkLabel(ExitLoop);
            }
        }
    }
    #endregion

    #region Then Statements
    public class ThenStatement : NormalStatment
    {
        NormalStatment ns;
        public ThenStatement()
        {

        }
        [Rule(@"<Then Stm>   ::=  <Normal Stm>")]
        public ThenStatement(NormalStatment normal)
        {
            ns = normal;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (ns != null)
                return ns.DoResolve(rc);
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ns != null)
                return ns.Emit(ec);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            if (ns != null)
                return ns.Resolve(rc);
            else return true;
        }
    }

    public class WhileThenStatement : ThenStatement
    {
        public WhileStatement While { get; set; }
 

        [Rule("<Then Stm>   ::=  ~while ~'(' <Expression> ~')' <Then Stm>")]
        public WhileThenStatement(Expr exp, ThenStatement stmt)
        {
            While = new WhileStatement(exp, (Statement)stmt);


        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return While.DoResolve(rc);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return While.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return While.Emit(ec);
        }
    }
    public class ForThenStatement : ThenStatement
    {

        public ForStatement For { get; set; }

        [Rule("<Then Stm>   ::=  ~for ~'(' <Arg> ~';' <Arg> ~';' <Arg> ~')' <Then Stm>")]
        public ForThenStatement(ArgumentExpression init, ArgumentExpression cond, ArgumentExpression inc, ThenStatement stmt)
        {
            For = new ForStatement(init, cond, inc, stmt);


        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return For.DoResolve(rc);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return For.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return For.Emit(ec);
        }
    }
    public class IfThenStatement : ThenStatement
      {
          IfElseStatement IfElse { get; set; }

     

          [Rule("<Then Stm>   ::= ~if ~'(' <Expression> ~')' <Then Stm> ~else <Then Stm> ")]
          public IfThenStatement(Expr ifexp, ThenStatement ifstmt, ThenStatement elsestmt)
          {
              IfElse = new IfElseStatement(ifexp, ifstmt, (Statement)elsestmt);

          }

          public override SimpleToken DoResolve(ResolveContext rc)
          {
 
              return IfElse.DoResolve(rc);
          }
          public override bool Emit(EmitContext ec)
          {
         
              return IfElse.Emit(ec);
          }
        
      }
    #endregion

    #region Statements
    public class LoopStatement : BaseStatement, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }



        Statement _stmt;
 
        [Rule(@"<Statement>        ::= ~loop <Statement>")]
        public LoopStatement( Statement stmt)
        {

            _stmt = stmt;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CurrentScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.LOOP);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;

            _stmt = (Statement)_stmt.DoResolve(rc);
        

            rc.CurrentScope &= ~ResolveScopes.Loop;
            // exit current loop
            rc.EnclosingLoop = ParentLoop;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
     
            ec.MarkLabel(EnterLoop);

            _stmt.Emit(ec);
            ec.MarkLabel(LoopCondition);
            ec.EmitInstruction(new Jump() {  DestinationLabel = EnterLoop.Name });
            ec.MarkLabel(ExitLoop);
            return true;
        }
    }
    public class AsmStatement : BaseStatement
    {
        public List<string> Instructions { get; set; }
        AsmInstructions _stmt;
        [Rule(@"<Statement>        ::= ~asm ~'{' <INSTRUCTIONS>  ~'}'")]
        public AsmStatement(AsmInstructions stmt)
        {
            Instructions = new List<string>();

            _stmt = stmt;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            AsmInstructions st = _stmt;
            while (st != null)
            {
                AsmInstruction ins = st.ins;
                if (ins != null)
                    Instructions.Add(ins.Value);
                st = st.nxt;
            }
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            foreach (string ins in Instructions)
                ec.EmitInstruction(new InlineInstruction(ins));
            return true;
        }
    }
    public class VarDeclStatement : BaseStatement
    {
        public VariableDeclaration Declaration { get { return _vadecl; } }

        VariableDeclaration _vadecl;
        [Rule(@"<Statement>        ::= <Var Decl>")]
        public VarDeclStatement(VariableDeclaration vardecl)
        {
            _vadecl = vardecl;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _vadecl = (VariableDeclaration)_vadecl.DoResolve(rc);

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
       
            return _vadecl.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {

            return _vadecl.Emit(ec);
        }
    }
    public class LabelStatement : BaseStatement
    {
        public Label Label { get; set; }

        Identifier _label;
        [Rule(@"<Statement> ::= Id ~':'")]
        public LabelStatement(Identifier id)
        {
            _label = id;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            Label = ec.DefineLabel(_label.Name);
            ec.MarkLabel(Label);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
    }
    public class WhileStatement : BaseStatement, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }

     

        Expr _expr;
       Statement _stmt;

        [Rule("<Statement>    ::=  ~while ~'(' <Expression> ~')' <Statement>")]
        public WhileStatement(Expr exp, Statement stmt)
        {
            _expr = exp;
            _stmt = stmt;


        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
          
            rc.CurrentScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.WHILE);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;
       

            _expr = (Expr)_expr.DoResolve(rc);

            if (_expr.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(30, Location, "Loop condition must be a boolean expression");

            _stmt = (Statement)_stmt.DoResolve(rc);

            rc.CurrentScope &= ~ResolveScopes.Loop;
            // exit current loop
            rc.EnclosingLoop = ParentLoop;
     
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return _expr.Resolve(rc) && _stmt.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            if (_expr.current is ConstantExpression || _expr is ConstantExpression)
                EmitConstantLoop(ec);
            else
            {
                ec.EmitInstruction(new Jump() { DestinationLabel = ExitLoop.Name });

                ec.MarkLabel(EnterLoop);

                _stmt.Emit(ec);

                ec.MarkLabel(LoopCondition);
                // emit expression branchable
                _expr.EmitBranchable(ec, EnterLoop, true);
                // exit
                ec.MarkLabel(ExitLoop);
            }
            return true;
        }

        void EmitConstantLoop(EmitContext ec)
        {

            ConstantExpression ce = null;

            if (_expr is ConstantExpression)
                ce = (ConstantExpression)_expr;
            else
                ce = (ConstantExpression)_expr.current;

            bool val = (bool)ce.GetValue();
            if (val)
            { // if true


                ec.MarkLabel(EnterLoop);

                _stmt.Emit(ec);
                ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
                ec.MarkLabel(ExitLoop);
            }
        }
    }
    
    public class IfStatement : BaseStatement, IConditional
    {
        public IConditional ParentIf { get; set; }
        public Label Else { get; set; }
        public Label ExitIf { get; set; }
        Expr _expr;
        Statement _stmt;
        [Rule(@"<Statement>        ::= ~if ~'(' <Expression> ~')' <Statement>  ")]
        public IfStatement(Expr expr, BaseStatement stmt)
        {
            _expr = expr;
            _stmt = stmt;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
           
            Label lb = rc.DefineLabel(LabelType.IF);
            ExitIf = rc.DefineLabel(lb.Name + "_EXIT");
            Else = rc.DefineLabel(lb.Name + "_ELSE");
            ParentIf = rc.EnclosingIf;
            rc.EnclosingIf = this;
            rc.CurrentScope |= ResolveScopes.If;
    
            // enter if
            _expr = (Expr)_expr.DoResolve(rc);
        
            if (_expr.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(30, Location, "If condition must be a boolean expression");

            _stmt = (Statement)_stmt.DoResolve(rc);
  
            rc.CurrentScope &= ~ResolveScopes.If;
            // exit current if
            rc.EnclosingIf = ParentIf;
 
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (_expr.current is ConstantExpression || _expr is ConstantExpression)
                EmitConstantIf(ec);
            else
            {
                // emit expression branchable
                ec.EmitComment("if-expression evaluation");
                _expr.EmitBranchable(ec, ExitIf, false);
                ec.EmitComment("If body");
                _stmt.Emit(ec);
                ec.MarkLabel(ExitIf);
                ec.MarkLabel(Else);
            }
            return true;
        }
        void EmitConstantIf(EmitContext ec)
        {

            ConstantExpression ce = null;

            if (_expr is ConstantExpression)
                ce = (ConstantExpression)_expr;
            else
                ce = (ConstantExpression)_expr.current;

            bool val = (bool)ce.GetValue();
            if (val)
             // if true
                _stmt.Emit(ec);
            ec.MarkLabel(Else);
            }
        
    }
    public class IfElseStatement : BaseStatement, IConditional
    {
        public IConditional ParentIf { get; set; }
      
        public Label ExitIf { get; set; }
        public Label Else { get; set; }
 
        Expr _expr;
        Statement _stmt;
        Statement _elsestmt;
   
        [Rule(@"<Statement>        ::= ~if ~'(' <Expression> ~')' <Then Stm> ~else <Statement>   ")]
        public IfElseStatement(Expr expr, ThenStatement stmt, Statement elsestmt)
        {
            _expr = expr;
            _stmt = stmt;
            _elsestmt = elsestmt;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
        
            Label lb = rc.DefineLabel(LabelType.IF);
            ExitIf = rc.DefineLabel(lb.Name + "_EXIT");
            Else = rc.DefineLabel(lb.Name + "_ELSE");
            ParentIf = rc.EnclosingIf;
            rc.EnclosingIf = this;
            rc.CurrentScope |= ResolveScopes.If;
    
            // enter if
            _expr = (Expr)_expr.DoResolve(rc);
            _stmt = (Statement)_stmt.DoResolve(rc);
            _elsestmt = (Statement)_elsestmt.DoResolve(rc);

     
            if (_expr.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(30, Location, "If condition must be a boolean expression");

            rc.CurrentScope &= ~ResolveScopes.If;
            // exit current if
            rc.EnclosingIf = ParentIf;
    

            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (_expr.current is ConstantExpression || _expr is ConstantExpression)
                EmitIfConstant(ec);
            else
            {
                // emit expression branchable
                ec.EmitComment("if-expression evaluation");
                _expr.EmitBranchable(ec, Else, false);
                ec.EmitComment("("+_expr.CommentString() + ") is true");
                _stmt.Emit(ec);
                ec.MarkLabel(Else);
                ec.EmitComment("Else ");
                _elsestmt.Emit(ec);
                ec.MarkLabel(ExitIf);
            }
            return true;
        }
        void EmitIfConstant(EmitContext ec)
        {
            ConstantExpression ce = null;

            if (_expr is ConstantExpression)
                ce = (ConstantExpression)_expr;
            else
                ce = (ConstantExpression)_expr.current;

            bool val = (bool)ce.GetValue();
            if (!val) // emit else
                _elsestmt.Emit(ec);
            else _stmt.Emit(ec);
        }
    }

    public class Switch : NormalStatment
    {
    
        bool HasDefault { get; set; }
        public Label SWITCH { get; set; }
        public Label SWITCH_EXIT { get; set; }
       public Label ResolveCase(int i)
        {
            return _cases.IdentifyCase(i-1);

        }
        private Case _cases;
        private Expr _expr;

        [Rule("<Normal Stm> ::= ~switch ~'(' <Expression> ~')' ~'{' <Case Stms> ~'}'")]
        public Switch(Expr sw, Case cases)
        {
            _cases = cases;
            _expr = sw;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.EnclosingSwitch = this;

            _expr = (Expr)_expr.DoResolve(rc);
            _cases = (Case)_cases.DoResolve(rc);
            _cases.SetCompare(rc, _expr);
            if (!_expr.Type.IsNumeric)
                ResolveContext.Report.Error(39, Location, "Could not use switch with non numeric types");
            else if(_expr is ConstantExpression)
                ResolveContext.Report.Error(41, Location, "Could not use switch with non constant values");

            rc.EnclosingSwitch = null;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {      // define switch
            SWITCH = rc.DefineLabel(LabelType.SW);
            SWITCH_EXIT = rc.DefineLabel(SWITCH.Name +"_EXIT");
            if (_cases != null)
            {
                _cases.Resolve(rc);
             HasDefault =   _cases.SetSwitch(SWITCH,SWITCH_EXIT,0);
            }
            _expr.Resolve(rc);
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("Switch ");
          //  ec.MarkLabel(SWITCH);
            _cases.Emit(ec);
            ec.MarkLabel(SWITCH_EXIT);
            return true;
        }
    }
    public class Case : NormalStatment, IConditional
    {
        public Label CaseLabel { get; set; }
        public Label SwitchLabel { get; set; }
        public Label SwitchExit { get; set; }
        public int CaseIndex { get; set; }
        public Label IdentifyCase(int id)
        {
            if (id == CaseIndex)
                return CaseLabel;
            else if (IsDefault && id == -1)
                return CaseLabel;
            else if (_next_case != null)
                return _next_case.IdentifyCase(id);
            else return CaseLabel;
        }
        public BinaryOperation CompareOperation { get; set; }

        public IConditional ParentIf { get; set; }
        public Label ExitIf { get; set; }
        public Label Else { get; set; }

        public bool IsDefault = false;
        private Statement _statements;
        private Expr _val;
        private Case _next_case;

        public bool SetSwitch(Label lb,Label swexit, int i)
        {
            if (_statements == null)
                return false;
            CaseIndex = i;
            SwitchLabel = lb;
            if(!IsDefault)
            CaseLabel = new Label(lb.Name + "_CASE_" + CaseIndex.ToString());
            else CaseLabel = new Label(lb.Name + "_DEFAULT");
            SwitchExit = swexit;
            if (_next_case != null)
                return _next_case.SetSwitch(lb, swexit, i + 1);
            else return IsDefault;
        }
        public void SetCompare(ResolveContext rc,Expr expr)
        {
            if (!IsDefault)
            {
                CompareOperation = new BinaryOperation(expr, new EqualOperator(), _val);
                CompareOperation = (BinaryOperation)CompareOperation.DoResolve(rc);
                if (_next_case != null)
                    _next_case.SetCompare(rc, expr);
            }
        }
        [Rule("<Case Stms> ::= ~case <Value> ~':' <Stm List> <Case Stms>")]
        public Case(Expr val, Statement stmt, Case nxt)
        {
            _statements = stmt;
            _val = val;
            _next_case = nxt;
        }

        // default
        [Rule("<Case Stms> ::= ~default ~':' <Stm List>   ")]
        public Case(Statement stmt)
        {
            IsDefault = true;
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
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_statements == null)
                return null;

            if (_next_case != null && _next_case.CaseLabel != null) // set next case
                Else = _next_case.CaseLabel;
            else
                Else = SwitchExit;
          ExitIf = SwitchExit;
            ParentIf = rc.EnclosingIf;
            rc.EnclosingIf = this;
            rc.CurrentScope |= ResolveScopes.Case;
            // enter case

            if (_val != null)
            {
                _val = (Expr)_val.DoResolve(rc);
                if(!_val.Type.IsNumeric)
                    ResolveContext.Report.Error(39, Location, "Could not use switch with non numeric types");
                else if(!(_val is ConstantExpression))
                    ResolveContext.Report.Error(40, Location, "Case value must be constant");
            }
            _statements = (Statement)_statements.DoResolve(rc);
            if (_next_case != null)
                 _next_case = (Case)   _next_case.DoResolve(rc);

            // exit case
            rc.CurrentScope &= ~ResolveScopes.Case;
            rc.EnclosingIf = ParentIf;
            return this;
        }
        public override bool Emit(EmitContext ec)
        {
            if (_statements == null)
                return false;
            ec.EmitComment("Case "+CaseIndex.ToString());
            ec.MarkLabel(CaseLabel);// case lb
            // compare if not default
            if (!IsDefault)
                CompareOperation.EmitBranchable(ec, Else, false);

            //case body
            ec.EmitComment("Case " + CaseIndex.ToString() +" body");
       
            _statements.Emit(ec);
         //Exit switch
            if (!IsDefault)
            {
                ec.EmitComment("Exit switch");
                ec.EmitInstruction(new Jump() { DestinationLabel = SwitchExit.Name });
            }
            if(_next_case != null)
            _next_case.Emit(ec);
            return true;
        }

    }

    public class ForStatement : BaseStatement, ILoop
    {
        public Label EnterLoop { get; set; }
        public Label ExitLoop { get; set; }
        public Label LoopCondition { get; set; }
        public ILoop ParentLoop { get; set; }


        ArgumentExpression _init;
        ArgumentExpression _cond;
        ArgumentExpression _inc;

        Expr _initialize;
        Expr _exit;
        Expr _increment;
        Statement _stmt;

        [Rule("<Statement>     ::= ~for ~'(' <Arg> ~';' <Arg> ~';' <Arg> ~')' <Statement>")]
        public ForStatement(ArgumentExpression init, ArgumentExpression cond, ArgumentExpression inc, Statement stmt)
        {
            _init = init;
            _cond = cond;
            _inc = inc;
            _stmt = stmt;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CurrentScope |= ResolveScopes.Loop;
            Label lb = rc.DefineLabel(LabelType.FOR);
            ExitLoop = rc.DefineLabel(lb.Name + "_EXIT");
            LoopCondition = rc.DefineLabel(lb.Name + "_COND");
            EnterLoop = rc.DefineLabel(lb.Name + "_ENTER");
            ParentLoop = rc.EnclosingLoop;
            // enter loop
            rc.EnclosingLoop = this;

            if (_init.argexpr != null)
                _initialize = (Expr)_init.DoResolve(rc);

            if (_cond.argexpr != null)
                _exit = (Expr)_cond.DoResolve(rc);

            if (_inc.argexpr != null)
                _increment = (Expr)_inc.DoResolve(rc);

            _stmt = (Statement)_stmt.DoResolve(rc);

            // exit current loop
            rc.CurrentScope &= ~ResolveScopes.Loop;
            rc.EnclosingLoop = ParentLoop;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            _stmt.Resolve(rc);
            _inc.Resolve(rc);
            _init.Resolve(rc);
            _cond.Resolve(rc);
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("For init");
            if(_initialize != null)
            _initialize.Emit(ec);
            ec.EmitInstruction(new Jump() { DestinationLabel = LoopCondition.Name });
            ec.MarkLabel(EnterLoop);
            ec.EmitComment("For block");
            _stmt.Emit(ec);

            ec.EmitComment("For increment");
          
            if (_increment != null)
            _increment.Emit(ec);

            ec.MarkLabel(LoopCondition);
            if (_exit != null)
                _exit.EmitBranchable(ec, EnterLoop, true);
            else ec.EmitInstruction(new Jump() { DestinationLabel = EnterLoop.Name });
            ec.EmitComment("Exit for");
            ec.MarkLabel(ExitLoop);
            return true;
        }
    }
#endregion
}
