using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core
{
   public class BlockOrSemi : Definition
    {
       public bool IsSemi { get; set; }


       public Block Block { get; set; }

       [Rule(@"<Block Or Semi> ::= <Block> ")]
       public BlockOrSemi(Block b)
       {
           IsSemi = false;
           Block = b;
       }

       [Rule(@"<Block Or Semi> ::= ~';' ")]
       public BlockOrSemi()
       {
           IsSemi = true;
       }


       public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
       {
           if (Block != null)
               return Block.DoFlowAnalysis(fc);
           return base.DoFlowAnalysis(fc);
       }

       public override bool Resolve(ResolveContext rc)
       {
           if (Block != null)
               Block.Resolve(rc);
           return base.Resolve(rc);
       }
       public override SimpleToken DoResolve(ResolveContext rc)
       {
           if (!IsSemi)
               Block = (Block)Block.DoResolve(rc);


           return base.DoResolve(rc);
       }
    }
}
