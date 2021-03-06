using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class InterruptDeclaration : Declaration
    {
        MethodSpec method;
        Modifiers mods = Modifiers.NoModifier;
        public string ItName;

        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }

        ushort interrupt;
        Block _b;
        bool _isolated=false;

        [Rule(@"<Inter Decl> ::= ~interrupt <Integral Const> <Block>")]
        public InterruptDeclaration(Literal hlit, Block b)
        {
          //  Location = hlit.Location;
            interrupt = ushort.Parse(hlit.Value.GetValue().ToString());
            _b = b;
            ItName = "INTERRUPT_" + interrupt.ToString("X2") + "H";
        }
        [Rule(@"<Inter Decl> ::= isolated ~interrupt <Integral Const> <Block>")]
        public InterruptDeclaration(SimpleToken t,Literal hlit, Block b)
        {
            _isolated = true;
            //  Location = hlit.Location;
            interrupt = ushort.Parse(hlit.Value.GetValue().ToString());
            _b = b;
            ItName = "INTERRUPT_" + interrupt.ToString("X2") + "H";
        }

       public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;

            if (_b != null)
                ok &= _b.Resolve(rc);
            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {

            ItName = "INTERRUPT_" + interrupt.ToString("X2") + "H";

             rc.Resolver.TryResolveMethod(ItName,ref method);
            if (method != null)
                ResolveContext.Report.Error(9, Location, "Duplicate interrupt name, multiple interrupt definition is not allowed");


            if (!CompilerContext.CompilerOptions.IsInterrupt)
                ResolveContext.Report.Error(0, Location, "Interrupt definition disabled, check interrupt option");
            method = new MethodSpec(rc.CurrentNamespace, ItName, mods, BuiltinTypeSpec.Void, CallingConventions.StdCall, null, this.Location);

            rc.KnowMethod(method);
            rc.CurrentMethod = method;

            if (_b != null)
                _b = (Block)_b.DoResolve(rc);
            return this;
        }
   
        public override bool Emit(EmitContext ec)
        {



            Label mlb = ec.DefineLabel(method.Signature.ToString());
            mlb.Method = true;
            ec.MarkLabel(mlb);
            ec.EmitComment("Interrupt: Number = " + interrupt.ToString());
            // save flags
            if (_isolated)
            {

                ec.EmitComment("save registers");
                ec.EmitInstruction(new Pushad());
            }
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });

            // allocate variables

            ushort size = 0;
            foreach (VarSpec v in ec.CurrentResolve.GetLocals())
                size += (ushort)(v.memberType.IsArray ? v.memberType.GetSize(v.memberType) : v.MemberType.Size);

            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });


            ec.EmitComment("Block");
            // Emit Code
            if (_b != null)
                _b.Emit(ec);

            ec.EmitComment("return label");
            // Return Label
            ec.MarkLabel(ec.DefineLabel(method.Signature + "_ret"));
            // Destroy Stack Frame
            ec.EmitComment("destroy stackframe");
            ec.EmitInstruction(new Leave());

            // restore flags
            if (_isolated)
            {
                ec.EmitComment("restore registers");
                ec.EmitInstruction(new Popad());
            }
            // ret
            ec.EmitInstruction(new IRET());

            ec.EmitINT(interrupt, mlb);
            return true;
        }
   
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            fc.AddNew(method);
            fc.MarkAsUsed(method);


            fc.LookForUnreachableBrace = !fc.NoReturnCheck;
            fc.NoReturnCheck = true;
            FlowState fs = FlowState.Valid;
            if (_b != null)
                fs = _b.DoFlowAnalysis(fc);
            else
                fs = base.DoFlowAnalysis(fc);

            if (!fs.Reachable.IsUnreachable && !fc.NoReturnCheck)
                fc.ReportNotAllCodePathsReturns(Location);

            fc.LookForUnreachableBrace = false;

            return base.DoFlowAnalysis(fc);
        }
      
    }
}
