using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	 /// <summary>
    /// Local Variable Specs
    /// </summary>
    public class VarSpec : MemberSpec, IEquatable<VarSpec>
    {
       
        MethodSpec method;
        public int StackIdx
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
        public bool Initialized { get; set; }
        public ReferenceSpec Emitter { get; set; }
        public MethodSpec MethodHost
        {
            get
            {
                return method;
            }
        }
        public int FlowIndex { get; set; }
        public Namespace NS { get; set; }
        public VarSpec(Namespace ns, string name, MethodSpec host, TypeSpec type, Location loc, int flow_idx, Modifiers mods = VTC.Modifiers.NoModifier, bool access = false)
            : base(name, new MemberSignature(ns, host.Name + "_" + name, loc), mods, ReferenceKind.LocalVariable)
        {
            method = host;
            memberType = type;
            NS = ns;
            Initialized = false;
            FlowIndex = flow_idx;

            if (memberType.IsMultiDimensionArray)
            {
                if (access)
                    Emitter = new HostedMatrixEmitter(this, 0, ReferenceKind.LocalVariable);
                else
                    Emitter = new MatrixEmitter(this, 0, ReferenceKind.LocalVariable);
            }
            else if (memberType.IsArray)
            {
                if(access)
                    Emitter = new HostedArrayEmitter(this, 0, ReferenceKind.LocalVariable);
                else
                    Emitter = new ArrayEmitter(this, 0, ReferenceKind.LocalVariable);
            }
            else if (memberType.IsBuiltinType || memberType.IsDelegate)
            {
                if (memberType.Size == 2)
                    Emitter = new WordEmitter(this, 0, ReferenceKind.LocalVariable);
                else if (memberType.Size == 1)
                    Emitter = new ByteEmitter(this, 0, ReferenceKind.LocalVariable);
            }
            else if (memberType.IsForeignType)
                Emitter = new StructEmitter(this, 0, ReferenceKind.LocalVariable);
           

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


        public bool Equals(VarSpec tp)
        {
            return tp.Signature == Signature;
        }
    
    }

	
	
}