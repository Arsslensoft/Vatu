using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Register Spec
    /// </summary>
    public class RegisterSpec : MemberSpec, IEquatable<RegisterSpec>
    {

        public ReferenceSpec Emitter { get; set; }
        public int RegisterIndex { get { return Emitter.Offset; } set { Emitter.Offset = value; } }
        public RegistersEnum Register { get { return Emitter.Register; } set { Emitter.Register = value; } }
        public int InitialRegisterIndex
        {
            get
            {
                return Emitter.InitialIndex;

            }
            set
            {

                Emitter.InitialIndex = value;
            }
        }

        public RegisterSpec(TypeSpec type, RegistersEnum rg, Location loc,int index,bool access =false)
            : base(rg.ToString(), new MemberSignature(Namespace.Default, rg.ToString(), loc), Modifiers.NoModifier, ReferenceKind.LocalVariable)
        {
          
            memberType = type;

            Emitter = ReferenceSpec.GetEmitter(this, memberType,index, ReferenceKind.Register, access);
            InitialRegisterIndex = index;
            Register = rg;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }

        public override bool EmitToStack(EmitContext ec)
        {

            return Emitter.EmitToStack(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {

            return Emitter.EmitFromStack(ec);
        }

        public override bool LoadEffectiveAddress(EmitContext ec)
        {


            return Emitter.LoadEffectiveAddress(ec);
        }
        public override bool ValueOf(EmitContext ec)
        {

            return Emitter.ValueOf(ec);
        }
        public override bool ValueOfStack(EmitContext ec)
        {

            return Emitter.ValueOfStack(ec);
        }

        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {
            return Emitter.ValueOfAccess(ec, off, mem);
        }
        public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
        {
            return Emitter.ValueOfStackAccess(ec, off, mem);
        }

        public bool Equals(RegisterSpec tp)
        {
            return tp.Signature == Signature;
        }

    }

	
	
}