using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class TryCatchStatement : BaseStatement, ITry
    {
        public ITry ParentTry { get; set; }
        public Label EnterTry { get; set; }
        public Label ExitTry { get; set; }
        public Label TryCatch { get; set; }
        public Label TryReturn { get; set; }
   

        Label enclosing_return = null;

        Statement _stmt;
        Statement _cstmt;
        VariableExpression ExHandler ;

        public bool SupportedThrow(VariableExpression exp)
        {
            if (exp.Type == ExHandler.Type)
                return true;
            else return false;
        }

        [Rule(@"<Statement>        ::= ~try <Statement> ~catch ~'('  <Var Expr> ~')' <Statement>")]
        public TryCatchStatement(Statement stmt, VariableExpression errc, Statement cstmt)
        {
            ExHandler = errc;
            _cstmt = cstmt;
            _stmt = stmt;
        }
        public override bool Resolve(ResolveContext rc)
        {
            
            return _stmt.Resolve(rc);
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            rc.CurrentScope |= ResolveScopes.Try;
            Label lb = rc.DefineLabel(LabelType.TRY_CATCH);
            ExitTry = rc.DefineLabel(lb.Name + "_EXIT");
            TryCatch = rc.DefineLabel(lb.Name + "_CATCH");
            EnterTry = rc.DefineLabel(lb.Name + "_ENTER");
            TryReturn = rc.DefineLabel(lb.Name + "_RETURN");
            ParentTry = rc.EnclosingTry;
            // enter loop
            rc.EnclosingTry = this;



            ExHandler = (VariableExpression)ExHandler.DoResolve(rc);
            _stmt = (Statement)_stmt.DoResolve(rc);

            _cstmt = (Statement)_cstmt.DoResolve(rc);
            if(rc.EnclosingTry == null)
                            enclosing_return = new Label(rc.CurrentMethod.Signature + "_ret");
            else
                enclosing_return = rc.EnclosingTry.TryReturn;

            rc.CurrentScope &= ~ResolveScopes.Try;
            // exit current loop

            rc.EnclosingTry = ParentTry;
            return this;
        }
   
        public override bool Emit(EmitContext ec)
        {
            

            // try enter routine
            ec.MarkLabel(EnterTry);
            ec.EmitComment("Try Enter Routine");
            ec.EmitComment("TODO CALL TENR");
            
            // try code
            _stmt.Emit(ec);
            ec.EmitInstruction(new Jump() { DestinationLabel = ExitTry.Name });
            // catch
            ec.MarkLabel(TryCatch);
            ExHandler.EmitFromStack(ec); // exception
            _cstmt.Emit(ec);
            ec.EmitInstruction(new Jump() { DestinationLabel = ExitTry.Name });

            ec.MarkLabel(TryReturn);
            ec.EmitComment("Try Return Routine");
            // exit routine
            ec.EmitComment("Try Exit Routine");
            ec.EmitComment("TODO CALL TEXR");


            ec.EmitInstruction(new Jump() {DestinationLabel = enclosing_return.Name });
            ec.MarkLabel(ExitTry);
            // exit routine code
            ec.EmitComment("Try Exit Routine");
      


            return true;
        }
      

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
         
            FlowState ok = ExHandler.DoFlowAnalysis(fc);


          _stmt.DoFlowAnalysis(fc);


            _cstmt.DoFlowAnalysis(fc);

      
            return ok;
        }
    }
    
	
}