using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
	 public class StructDefinition : Definition
    {


        public VariableDeclaration _var;
        public StructDefinition _next_sdef;
        public int Size { get; set; }
        public List<TypeMemberSpec> Members { get; set; }


        [Rule(@"<Struct Def>   ::= <Struct Var Decl> <Struct Def>")]
        public StructDefinition(VariableDeclaration var, StructDefinition sdef)
        {
            _var = var;
            _next_sdef = sdef;
            Size = 0;

        }
        [Rule(@"<Struct Def>   ::= <Struct Var Decl>")]
        public StructDefinition(VariableDeclaration var)
        {

            _var = var;
            _next_sdef = null;
            Size = 0;
        }

       public override bool Resolve(ResolveContext rc)
        {

            bool ok = _var.Resolve(rc);
            if (_next_sdef != null)
                ok &= _next_sdef.Resolve(rc);

            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            Members = new List<TypeMemberSpec>();
            _var = (VariableDeclaration)_var.DoResolve(rc);
           
            if (_var != null)
            {

                if (_var.Members[0].MemberType is ArrayTypeSpec)
                    Size += (_var.Members[0].MemberType as ArrayTypeSpec).ArrayCount * _var.Members[0].MemberType.BaseType.Size;
                else
                  Size += _var.Type.Size;
            }

            foreach (TypeMemberSpec sv in _var.Members)
            {   if(!Members.Contains(sv))
                      Members.Add(sv);
            else ResolveContext.Report.Error(0, Location, "Duplicate member declaration");
            }
            if (_next_sdef != null)
            {


                _next_sdef = (StructDefinition)_next_sdef.DoResolve(rc);
                foreach (TypeMemberSpec sv in _next_sdef.Members)
                {
                    if(!Members.Contains(sv))
                       Members.Add(sv);
                    else ResolveContext.Report.Error(0, Location, "Duplicate member declaration");
                }
            }
            return this;
        }
     
        public override bool Emit(EmitContext ec)
        {
            return true;
        }
    }
   
	
}