using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class VarDeclStatement : BaseStatement
    {
        public VariableDeclaration Declaration { get { return _vadecl; } }

        VariableDeclaration _vadecl;
        [Rule(@"<Statement>        ::= <Struct Var Decl> ")]
        public VarDeclStatement(VariableDeclaration vardecl)
        {
            _vadecl = vardecl;
        }
       public override bool Resolve(ResolveContext rc)
        {
       
            return _vadecl.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _vadecl = (VariableDeclaration)_vadecl.DoResolve(rc);

            return this;
        }
        
        public override bool Emit(EmitContext ec)
        {

            return _vadecl.Emit(ec);
        }

      
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _vadecl.DoFlowAnalysis(fc);
        }
    }
    
	
}