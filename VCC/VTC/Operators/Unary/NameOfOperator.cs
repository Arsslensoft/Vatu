using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
	
	 public class NameOfOperator : Expr
    {
  

      
        private Identifier _id;
    


        [Rule(@"<Op Unary> ::= ~nameof ~'(' Id ~')'")]
        public NameOfOperator(Identifier type)
        {
            _id = type;
        }
      

       public override bool Resolve(ResolveContext rc)
        {
         
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {

            Type = BuiltinTypeSpec.String;
            if (_id != null )
            {
                MemberSpec ms = rc.Resolver.TryResolveName(_id.Name);

                if (ms != null)
                    return new StringConstant(ms.Name, Location);
                    
                else
                    ResolveContext.Report.Error(0,Location,"Unresolved name " + _id.Name);
            }

            return this;
        }
  
        /*   public override bool Emit(EmitContext ec)
           {
               RegistersEnum acc = ec.GetNextRegister();
               ec.EmitInstruction(new Mov() { DestinationReg = acc, SourceValue = (ushort)this.Size, Size = 16 });
               return true;
           }
           public override bool EmitToStack(EmitContext ec)
           {
               ec.EmitInstruction(new Push() { DestinationValue = (ushort)this.Size, Size = 16 });
               return true;
           }
           public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
           {
               ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)this.Size, Size = 16 });
               return true;
           }

           public override string CommentString()
           {
               return "sizeOf(" + Type.Name + ")";
           }*/
    }
   
}