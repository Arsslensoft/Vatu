using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

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
            if (ns != null)
                return ns.DoResolve(rc);

            return base.DoResolve(rc);
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

    #endregion

    #region Then Statements

    #endregion

    #region Statements

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
    public class GotoStatement : NormalStatment
    {
        public Label DestinationLabel { get; set; }

        private Identifier _id;

        [Rule("<Normal Stm> ::= ~goto Id ~';'")]
        public GotoStatement(Identifier id)
        {
            _id = id;

        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            DestinationLabel = new Label(_id.Name);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitInstruction(new Vasm.x86.Jump(){ DestinationLabel = this.DestinationLabel.Name});
        
            return true;
        }
    }

#endregion
}
