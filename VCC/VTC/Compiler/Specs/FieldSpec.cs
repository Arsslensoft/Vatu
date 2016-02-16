using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
	 /// <summary>
    /// Global Variable Specs
    /// </summary>
    public class FieldSpec : MemberSpec, IEquatable<FieldSpec>
    {
        public ReferenceSpec Emitter { get; set; }
        public bool IsIndexed { get; set; }
        public int FieldOffset
        {

            get
            {
                return Emitter.Offset;
            }
            set
            {
                Emitter.Offset = value;
            }
        }
        public int InitialFieldIndex
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
        public FieldSpec(Namespace ns,string name, Modifiers mods, TypeSpec type, Location loc,bool access =false)
            : base(name, new MemberSignature(ns,name, loc), mods,ReferenceKind.Field)
        {
         
            NS = ns;
            IsIndexed = false;
            memberType = type;

            Emitter = ReferenceSpec.GetEmitter(this, memberType, 0, ReferenceKind.Field, access);
            InitialFieldIndex = 0;
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
        public override string ToString()
        {
            return Signature.ToString();
        }
        public bool Equals(FieldSpec tp)
        {
            return tp.Signature == Signature;
        }
       
    }
    
	
}