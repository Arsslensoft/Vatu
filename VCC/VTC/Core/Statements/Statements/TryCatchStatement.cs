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
        public MemberSpec TryCatchStatus { get; set; }

        Label enclosing_return = null;

        Statement _stmt;
        Statement _cstmt;
        ushort ExHandler = 0;
    
        [Rule(@"<Statement>        ::= ~try <Statement> ~catch ~'(' <Integral Const> ~')' <Statement>")]
        public TryCatchStatement(Statement stmt, Literal errc,Statement cstmt)
        {
            ExHandler = ushort.Parse(errc.Value.GetValue().ToString());
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


            TryCatchStatus = rc.Resolver.TryResolveName("TryCatchState");
            if (TryCatchStatus == null)
                ResolveContext.Report.Error(0, Location, "Try catch can't be used without the TryCatchState variable");
            else if (TryCatchStatus.MemberType.GetSize(TryCatchStatus.MemberType)  != 6 || !TryCatchStatus.MemberType.IsStruct)
                ResolveContext.Report.Error(0, Location, "TryCatchState variable must be a struct size of 6 bytes for CS, Error code & catch @ ");


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
            TryCatchStatus.EmitToStack(ec); // copy

            ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(TryCatch.Name), Size = 16 });
            ec.EmitPush(ExHandler);
            ec.EmitPush(RegistersEnum.CS, 16);
            // pop
            TryCatchStatus.EmitFromStack(ec);

            // try code
            _stmt.Emit(ec);

            // catch
            ec.MarkLabel(TryCatch);
            _cstmt.Emit(ec);
            ec.EmitInstruction(new Jump() { DestinationLabel = ExitTry.Name });

            ec.MarkLabel(TryReturn);
            ec.EmitComment("Try Return Routine");
            // exit routine
            ec.EmitComment("Try Exit Routine");
            // pop copy
            TryCatchStatus.EmitFromStack(ec);


            ec.EmitInstruction(new Jump() {DestinationLabel = enclosing_return.Name });
            ec.MarkLabel(ExitTry);
            // exit routine code
            ec.EmitComment("Try Exit Routine");
            // pop copy
            TryCatchStatus.EmitFromStack(ec);


            return true;
        }
      

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            CodePath cur = new CodePath(_stmt.loc); // sub code path
   
            CodePath back = fc.CodePathReturn;
            fc.CodePathReturn = cur; // set current code path
            FlowState ok = FlowState.Valid;


            _stmt.DoFlowAnalysis(fc);

            back.AddPath(cur);

            cur = new CodePath(_cstmt.loc); // sub code path

            fc.CodePathReturn = cur; // set current code path

            _cstmt.DoFlowAnalysis(fc);

            back.AddPath(cur);
            fc.CodePathReturn = back; // restore code path
            return ok;
        }
    }
    
	
}