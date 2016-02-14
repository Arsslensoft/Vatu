using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	public class ParameterDefinition : Definition
    {
        public TypeSpec ParameterType { get; set; }
        public ParameterSpec ParameterName { get; set; }


        Identifier _id;
        TypeIdentifier _type;
        bool constant = false;
        bool REF = false;

        [Rule(@"<Param>      ::= ref <Type> Id")]
        [Rule(@"<Param>      ::= const <Type> Id")]
        public ParameterDefinition(SimpleToken tok, TypeIdentifier ptr, Identifier var)
        {
            _id = var;
            _type = ptr;
            if (tok.Name == "const")
                constant = true;
            else REF = true;
        }

        [Rule(@"<Param>      ::= <Type> Id")]
        public ParameterDefinition(TypeIdentifier ptr, Identifier var)
        {
            _id = var;
            _type = ptr;
            constant = false;
            REF = false;
        }
        
       public override bool Resolve(ResolveContext rc)
        {

            return _type.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _type = (TypeIdentifier)_type.DoResolve(rc);
            ParameterType = _type.Type;
            Modifiers mods = constant? Modifiers.Const: Modifiers.NoModifier;
            mods |= REF? Modifiers.Ref: 0;
            ParameterName = new ParameterSpec(rc.CurrentNamespace,_id.Name, rc.CurrentMethod, ParameterType, Location,4,mods );
            return this;
        }
 
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
    
	
}