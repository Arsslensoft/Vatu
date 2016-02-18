﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core
{
    public class SetterDefinition : Definition
    {
        public BlockOrSemi Block;
        public MethodSpec Setter { get; set; }
        public bool AutoGenerated { get { return Block.IsSemi; } }
       [Rule(@"<Setter Decl> ::= ~set <Block Or Semi>")]
       public SetterDefinition(BlockOrSemi bs)
       {
           Block = bs;
       }


       public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
       {

           return Block.DoFlowAnalysis(fc);
       }
       public override bool Resolve(ResolveContext rc)
       {
           return Block.Resolve(rc);
       }

       public override SimpleToken DoResolve(ResolveContext rc)
       {
           Block = (BlockOrSemi)Block.DoResolve(rc);
           return base.DoResolve(rc);
       }


       public void CreateSetter(ResolveContext rc,PropertySpec ps)
       {
           Setter = new MethodSpec(ps.NS, "set_" + ps.Name, ps.Modifiers,BuiltinTypeSpec.Void, CallingConventions.Default, new TypeSpec[1] { ps.memberType }, ps.Signature.Location);
           ps.Setter = Setter;

       }
    }

   
}
