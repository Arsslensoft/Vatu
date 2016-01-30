using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
 public class EnumDefinition : Definition
    {
  
        public int Size { get; set; }
        public List<EnumMemberSpec> Members { get; set; }
        EnumDefinition next_def;
        EnumValue _value;
        [Rule(@"<Enum Def>     ::= <Enum Val> ~',' <Enum Def>")]
        public EnumDefinition(EnumValue val, EnumDefinition def)
        {
            _value = val;
            next_def = def;
        }
        [Rule(@"<Enum Def>     ::= <Enum Val>")]
        public EnumDefinition(EnumValue val)
        {
            _value = val;
            next_def = null;
        }

        EnumMemberSpec GetMember(ResolveContext rc,EnumValue v, TypeSpec host)
        {

            object val = null;
            if (v.Value != null && ((val = v.Value.GetValue()) != null))
            {
                ushort uval = ushort.Parse(val.ToString());
                if (uval > 255)
                    Size = 2;
                return new EnumMemberSpec(rc.CurrentNamespace,v._id.Name, uval, host, (uval < 256) ? BuiltinTypeSpec.Byte : BuiltinTypeSpec.UInt, v.Location);

            }
            else return new EnumMemberSpec(rc.CurrentNamespace, v._id.Name, host, BuiltinTypeSpec.UInt, v.Location);
        }
       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _value.Resolve(rc);
            if (next_def != null)
                ok &= next_def.Resolve(rc);

            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            Size = 1;
            Members = new List<EnumMemberSpec>();
            _value = (EnumValue)_value.DoResolve(rc);
            if (next_def != null)
                next_def = (EnumDefinition)next_def.DoResolve(rc);
            Members.Add(GetMember(rc,_value, rc.CurrentType));
            EnumDefinition m = next_def;
            while (m != null)
            {
                if (m._value != null)
                    Members.Add(GetMember(rc,m._value, rc.CurrentType));
                if (m.Size == 2)
                    Size = 2;
                m = m.next_def;
            }
            return this;
        }
      
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    
	
}