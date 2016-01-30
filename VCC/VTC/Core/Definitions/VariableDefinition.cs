using bsn.GoldParser.Semantic;
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
        public MemberSpec FieldOrLocal { get; set; }
        public TypeMemberSpec Member { get; set; }
        public int FlowVarIndex = 0;
        public bool IsAssigned = false;
        public bool IsAbstract = false;
        public int ArraySize { get; set; }
        public ArrayVariableDefinition _avd;
        public Identifier _id;
        public Expr expr;

        [Rule(@"<Var>      ::= Id <Array>")]
        public VariableDefinition(Identifier id, ArrayVariableDefinition avd)
        {
            expr = null;
            _id = id;
            _avd = avd;
        }
        [Rule(@"<Var>      ::= Id")]
        public VariableDefinition(Identifier id)
        {
            expr = null;
            _id = id;
            _avd = null;
        }
        [Rule(@"<Var>      ::= Id <Array> ~'=' <Op If> ")]
        public VariableDefinition(Identifier id, ArrayVariableDefinition avd, Expr ifexpr)
        {
            expr = ifexpr;
            _id = id;
            _avd = avd;
        }
        [Rule(@"<Var>      ::= Id ~'=' <Op If> ")]
        public VariableDefinition(Identifier id, Expr ifexpr)
        {
            expr = ifexpr;
            _id = id;
            _avd = null;
        }

       public override bool Resolve(ResolveContext rc)
        {
            //TODO:ARRAY SUPPORT
            bool ok = true;
            if (_avd != null)
               ok &= _avd.Resolve(rc);
            if (expr != null)
                ok &= expr.Resolve(rc);
            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            ArraySize = -1;
            if (_avd != null)
            {
                
                _avd = (ArrayVariableDefinition)_avd.DoResolve(rc);
                if (_avd != null)
                    ArraySize = _avd.Size;
            }
           
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
            if (_avd != null)
                ok &= _avd.Emit(ec);

            if (expr != null)
            {
                ok &= expr.Emit(ec);
         // TODO
            }
            return ok;
        }
    }

}