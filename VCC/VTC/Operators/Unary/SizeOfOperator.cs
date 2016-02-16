using VTC.Base.GoldParser.Parser;
using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
	public class SizeOfOperator : Expr
    {
        public ushort Size
        {
            get;
            set;
        }

        private TypeIdentifier _type;
        private Identifier _id;
   

        [Rule(@"<Op Unary> ::= ~sizeof ~'(' <Type> ~')'")]
        public SizeOfOperator(TypeIdentifier type)
        {
            _type = type;
        }
        [Rule(@"<Op Unary> ::= ~sizeof ~'(' Id ~')'")]
        public SizeOfOperator(Identifier type)
        {
            _id = type;

        }

       public override bool Resolve(ResolveContext rc)
        {
            Size = 0;
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            Size = 0;
            if (_type != null)
            {
                _type = (TypeIdentifier)_type.DoResolve(rc);
                Type = _type.Type;
                Size = (ushort)Type.Size;
            }

            if (_id != null )
            {
                Type = rc.Resolver.TryResolveVar(_id.Name).MemberType;
              
                Size = (ushort)Type.Size;
          
            

            }

            return new UIntConstant(Size,Location) ;
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