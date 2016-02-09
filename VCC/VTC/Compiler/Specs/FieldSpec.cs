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

        public FieldSpec(Namespace ns,string name, Modifiers mods, TypeSpec type, Location loc,bool access =false)
            : base(name, new MemberSignature(ns,name, loc), mods,ReferenceKind.Field)
        {
         
            NS = ns;
            IsIndexed = false;
            memberType = type;
            if (memberType.IsMultiDimensionArray)
            {
                if (!access)
                    Emitter = new MatrixEmitter(this, 0, ReferenceKind.Field);
                else Emitter = new HostedMatrixEmitter(this, 0, ReferenceKind.Field);
            }
            else if (memberType.IsArray)
            {
                if(!access)
                       Emitter = new ArrayEmitter(this, 0, ReferenceKind.Field);
                else Emitter = new HostedArrayEmitter(this, 0, ReferenceKind.Field);
            }

            else if (memberType.IsBuiltinType || memberType.IsDelegate || memberType.IsTemplate)
            {
                if (memberType.IsFloat && !memberType.IsPointer)
                    Emitter = new FloatEmitter(this, 0, ReferenceKind.Field);
                else if (memberType.IsSigned && memberType.Size == 1)
                    Emitter = new SByteEmitter(this, 0, ReferenceKind.Field);

                else  if (memberType.Size == 2)
                    Emitter = new WordEmitter(this, 0, ReferenceKind.Field);
                else if (memberType.Size == 1)
                    Emitter = new ByteEmitter(this, 0, ReferenceKind.Field);
                else if(memberType.IsTemplate && memberType.Size > 2)
                    Emitter = new StructEmitter(this, 0, ReferenceKind.Field);
            }
            else if (memberType.IsForeignType)
                Emitter = new StructEmitter(this, 0, ReferenceKind.Field);
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