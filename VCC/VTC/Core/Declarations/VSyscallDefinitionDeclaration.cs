using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Declarations
{
    public class VSyscallDefinitionDeclaration : Declaration
    {
        VSyscallSpec ops;

        ushort inter;
        Modifier _mod;

        [Rule(@"<Vatu Syscall Decl>  ::=  <Mod> ~define ~vsyscall <Integral Const>  ~';' ")]
        public VSyscallDefinitionDeclaration(Modifier mod, Literal name)
        {
            _mod = mod;
            inter = ushort.Parse(name.Value.GetValue().ToString());
        }
     
   
       public override bool Resolve(ResolveContext rc)
        {


            return true;

        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
             rc.Resolver.TryResolveVSyscall(inter,ref ops);
            if (ops != null)
                ResolveContext.Report.Error(0, Location, "Duplicate Vatu System Call interrupt definition");

     ops = new VSyscallSpec(rc.CurrentNamespace, "VSYSCALL_INT_"+inter.ToString(), inter, _mod.ModifierList, Location);
            rc.Resolver.CurrentVSyscallInterrupt = inter;
            rc.Resolver.KnowSyscallInt(ops);
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {

            return true;
        }
    }
}
