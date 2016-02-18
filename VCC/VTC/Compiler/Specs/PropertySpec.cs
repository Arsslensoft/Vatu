using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{

    /// <summary>
    /// Method Specs
    /// </summary>
    public class PropertySpec : FieldSpec, IEquatable<PropertySpec>
    {
        public MethodSpec Getter { get; set; }
        public MethodSpec Setter { get; set; }
        public FieldSpec AutoBoundField { get; set; }
        public CallingConventionsHandler ccvh;

        public PropertySpec(Namespace ns, string name, Modifiers mods, TypeSpec type,  Location loc,bool access = false)
            :  base(ns,name,mods,type,loc,access)
        {
            ccvh = new CallingConventionsHandler();
            RegisterSpec rs = new RegisterSpec(type, RegistersEnum.AX, loc, 0);
            Emitter = rs.Emitter;
            Signature = new MemberSignature(ns, name,loc);
        }


        public override bool EmitToStack(EmitContext ec)
        {

            ec.EmitCallOperatorFromStack(Getter); // no params
                       return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitCallOperatorFromStack(Setter); // no params
            return true;
        }
        public override bool LoadEffectiveAddress(EmitContext ec)
        {
            throw new NotSupportedException("Properties can't have an effective address");
        }
        public override bool ValueOf(EmitContext ec)
        {
            EmitToStack(ec);
            return Emitter.ValueOf(ec);
        }
        public override bool ValueOfStack(EmitContext ec)
        {
            EmitToStack(ec);
            return Emitter.ValueOfStack(ec);
        }

        public override bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
        {
            EmitToStack(ec);
            return Emitter.ValueOfAccess(ec, off, mem);
        }
        public override bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
        {
            EmitToStack(ec);
            return Emitter.ValueOfStackAccess(ec, off, mem);
        }

        public override string ToString()
        {
            return Signature.ToString();
        }

        public bool Equals(PropertySpec tp)
        {
            return tp.Signature == Signature;
        }


    }




}