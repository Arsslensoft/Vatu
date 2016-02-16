using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	public class VariableDefinition : Definition
    {
       internal TypePointer _ptr;
        public MemberSpec FieldOrLocal { get; set; }
        public TypeSpec Type { get; set; }
        public TypeMemberSpec Member { get; set; }
        public int FlowVarIndex = 0;
        public bool IsAssigned = false;
        public bool IsAbstract = false;

        public TypeSpec CreateType(TypeSpec t)
        {
            if (_ptr == null)
                return t;
            else
                return _ptr.CreateType(t, _ptr);
        }
        public Identifier _id;
        public Expr expr;

        //[Rule(@"<Var>      ::= Id <Array>")]
        //public VariableDefinition(Identifier id, ArrayVariableDefinition avd)
        //{
        //    expr = null;
        //    _id = id;
        //    _avd = avd;
        //}
        [Rule(@"<Var>      ::= Id")]
        public VariableDefinition(Identifier id)
        {
            expr = null;
            _id = id;
      
        }
        //[Rule(@"<Var>      ::= Id <Array> ~'=' <Op If> ")]
        //public VariableDefinition(Identifier id, ArrayVariableDefinition avd, Expr ifexpr)
        //{
        //    expr = ifexpr;
        //    _id = id;
        //    _avd = avd;
        //}
        [Rule(@"<Var>      ::= Id ~'=' <Var Init Def>")]
        public VariableDefinition(Identifier id, VariableInitDefinition ifexpr)
        {
            expr = ifexpr;
            _id = id;
      
        }

       public override bool Resolve(ResolveContext rc)
        {
            //TODO:ARRAY SUPPORT
            bool ok = true;
  
            if (expr != null)
                ok &= expr.Resolve(rc);
            return ok;
        }
       public override SimpleToken DoResolve(ResolveContext rc)
        {
          
           
            if (expr != null)
                expr = (Expr)expr.DoResolve(rc);
       
                 

            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (expr != null)
                return expr.DoFlowAnalysis(fc);

            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            bool ok = true;
  

            if (expr != null)
            {
                ok &= expr.Emit(ec);
         // TODO
            }
            return ok;
        }
    }

}