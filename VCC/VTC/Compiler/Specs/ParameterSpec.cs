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
    public class ParameterSpec : MemberSpec, IEquatable<ParameterSpec>
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
        public int InitialStackIndex
        {
            get
            {
                return Emitter.InitialIndex;

            }
            set
            {
                if (Emitter == null)
                {

                }
                Emitter.InitialIndex = value;
            }
        }
        public ReferenceSpec Emitter { get; set; }


        public MethodSpec MethodHost
        {
            get
            {
                return method;
            }
        }
        public bool IsParameter
        {
            get
            {
                return true;
            }
        }
    
        public ParameterSpec(Namespace ns,string name, MethodSpec host, TypeSpec type, Location loc,int initstackidx, Modifiers mods = VTC.Modifiers.NoModifier, bool access = false)
            : base(name, new MemberSignature(ns, host.Name + "_param_" + name, loc), mods,ReferenceKind.Parameter)
        {
            method = host;
            memberType = type;


            Emitter = ReferenceSpec.GetEmitter(this, memberType, 4, ReferenceKind.Parameter, access,IsReference);
         
       
            InitialStackIndex = initstackidx;
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
        public bool Equals(ParameterSpec tp)
        {
            return tp.Signature == Signature;
        }

    
    }

	
}