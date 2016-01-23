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
	 public class AddressOfOperator : Expr
    {



        private Identifier _id;



        [Rule(@"<Op Unary> ::= ~addressof ~'(' Id ~')'")]
        public AddressOfOperator(Identifier type)
        {
            _id = type;
        }

        public string Label=null;
        MemberSpec ms= null;
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            Type = BuiltinTypeSpec.UInt;
            if (_id != null)
            {
                if (_id.Name == "begin")
                    Label = "PROGRAM_ORG";
                else if (_id.Name == "end")
                    Label = "PROGRAM_END";
                else
                {
                  ms=  rc.Resolver.TryResolveName(_id.Name);
                    if (ms == null)
                    {
                        MethodSpec mt = null;
                      rc.Resolver.TryResolveMethod(_id.Name,ref mt);
                      ms = mt;
                      if (mt == null)
                            Label = _id.Name;
                           
                    }
                      
                }
            }
            if(Label == null && ms == null)
                ResolveContext.Report.Error(0, Location, "Unresolved name " + _id.Name);
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            return true;
        }
         public override bool Emit(EmitContext ec)
           {
               if (ms == null)
               {


                   ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceRef = ElementReference.New(Label), Size = 16 });
                   ec.EmitPush(EmitContext.A);
               }
               else ms.LoadEffectiveAddress(ec);
               return true;
           }
           public override bool EmitToStack(EmitContext ec)
         {
             if (ms == null)
             {

                 ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.A, SourceRef = ElementReference.New(Label), Size = 16 });
                 ec.EmitPush(EmitContext.A);
             }
             else ms.LoadEffectiveAddress(ec);
               return true;
           }
       

           public override string CommentString()
           {
               return "AddressOf(" + Label + ")";
           }
    }
   
	
}