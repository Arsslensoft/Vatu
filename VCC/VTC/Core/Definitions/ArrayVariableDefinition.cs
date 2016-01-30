using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{

	public class ArrayVariableDefinition : Definition
    {
        public int Size { get; set; }
        public int Dimension { get; set; }
        ArrayVariableDefinition _nextdef;
        [Rule(@"<Array>    ::= ~'[' <Integral Const>  ~']' <Array>")]
        public ArrayVariableDefinition(Literal expr, ArrayVariableDefinition avd)
        {
            _nextdef = avd;
            _expr = expr;
        }
        Expr _expr;
        [Rule(@"<Array>    ::= ~'[' <Integral Const> ~']'")]
        public ArrayVariableDefinition(Literal expr)
        {
            _expr = expr;
        }

        [Rule(@"<Array>    ::= ~'[]'")]
        public ArrayVariableDefinition()
        {
            _expr = null;
        }
       public override bool Resolve(ResolveContext rc)
        {
      if (_expr != null)
            return _expr.Resolve(rc);
      return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            Dimension = 1;
            bool conv = false;
            if (_expr != null)
                _expr = (ConstantExpression)_expr.DoResolve(rc);


            if (_nextdef != null)
            {
              
                _nextdef = (ArrayVariableDefinition)_nextdef.DoResolve(rc);
                Dimension += _nextdef.Dimension;
            }


            if (_expr != null && _expr is ConstantExpression)
                Size = int.Parse((((ConstantExpression)_expr).ConvertImplicitly(rc, BuiltinTypeSpec.Int, ref conv)).GetValue().ToString());
            else Size = 0;

            if (Size < 0)
                ResolveContext.Report.Error(47, Location, "Invalid array size");
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            return true;
        }

        public ArrayTypeSpec CreateArrayType(TypeSpec root)
        {
            if (_nextdef == null)
                return new ArrayTypeSpec(root.NS, root, Size);
            else
                return new ArrayTypeSpec(root.NS, _nextdef.CreateArrayType(root), Size);
          
        }
    }
   
}