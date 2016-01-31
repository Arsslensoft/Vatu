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
        public ParameterSpec ReferenceParameter
        {
            get
            {
                return Emitter.ReferenceParameter;

            }
            set
            {
                Emitter.ReferenceParameter = value;
            }
        }
     
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
                return Emitter.InitialStackIndex;

            }
            set
            {
                Emitter.InitialStackIndex = value;
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
    
        public ParameterSpec(string name, MethodSpec host, TypeSpec type, Location loc,int initstackidx, Modifiers mods = VTC.Modifiers.NoModifier, bool access = false)
            : base(name, new MemberSignature(Namespace.Default, host.Name + "_param_" + name, loc), mods,ReferenceKind.Parameter)
        {
            method = host;
            memberType = type;



            if (IsReference)
            {

                Emitter = new ReferenceEmitter(this, 4, ReferenceKind.Parameter);
                if (!type.IsMultiDimensionArray)
                    ReferenceParameter = new ParameterSpec(name, host, type.MakePointer(), loc, initstackidx, VTC.Modifiers.NoModifier, access);
                else
                      ReferenceParameter = new ParameterSpec(name, host, type, loc, initstackidx, VTC.Modifiers.NoModifier, access);
            }
            else if (memberType.IsMultiDimensionArray)
            {
                if (!access)
                    Emitter = new MatrixEmitter(this, 4, ReferenceKind.Parameter);
                else
                    Emitter = new HostedMatrixEmitter(this, 4, ReferenceKind.Parameter);
            }
            else if (memberType.IsArray)
            {
                if (!access)
                    Emitter = new ArrayEmitter(this, 4, ReferenceKind.Parameter);
                else
                    Emitter = new HostedArrayEmitter(this, 4, ReferenceKind.Parameter);
            }
            else if (memberType.IsBuiltinType || memberType.IsDelegate)
            {
                if (memberType.IsFloat && !memberType.IsPointer)
                    Emitter = new FloatEmitter(this, 4, ReferenceKind.Parameter);
                else if (memberType.IsSigned && memberType.Size == 1)
                    Emitter = new SByteEmitter(this, 4, ReferenceKind.Parameter);

                else if (memberType.Size == 2)
                    Emitter = new WordEmitter(this, 4, ReferenceKind.Parameter);
                else if (memberType.Size == 1)
                    Emitter = new ByteEmitter(this, 4, ReferenceKind.Parameter);
            }
            else if (memberType.IsForeignType)
                Emitter = new StructEmitter(this, 4, ReferenceKind.Parameter);
          
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