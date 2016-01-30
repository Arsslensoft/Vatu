using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
 public class FunctionBodyDefinition : Definition
    {
       public Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }
        public List<TypeSpec> ParamTypes { get; set; }

        public ParameterListDefinition _pdef;
        public Block _b;
        public FunctionExtensionDefinition _ext;

      [Rule(@"<Func Body>  ::= ~'(' <Params>  ~')' <Func Ext>  <Block>")]
      public FunctionBodyDefinition(ParameterListDefinition pdef, FunctionExtensionDefinition ext,Block b)
      {
          _pdef = pdef;
          _b = b;
          _ext = ext;
      }
      [Rule(@"<Func Body>  ::= ~'('  ~')' <Func Ext> <Block>")]
      public FunctionBodyDefinition( FunctionExtensionDefinition ext,Block b)
      {
          _pdef = null;
          _b = b;
          _ext = ext;
      }

       public override bool Resolve(ResolveContext rc)
        {
        
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_ext != null && _ext.IsExtended)
                _ext = (FunctionExtensionDefinition)_ext.DoResolve(rc);
            else _ext = null;
       
            ParamTypes = new List<TypeSpec>();

            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            if (_pdef != null)
            {
             
                _pdef = (ParameterListDefinition)_pdef.DoResolve(rc);
                ParameterListDefinition par = _pdef;
                while (par != null)
                {
                    if (par._id != null)
                    {
                        Params.Push(par._id.ParameterName);
                        Parameters.Add(par._id.ParameterName);
                        ParamTypes.Add(par._id.ParameterName.MemberType);
                    }
                    par = par._nextid;
                }
            }
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
    }

	
}