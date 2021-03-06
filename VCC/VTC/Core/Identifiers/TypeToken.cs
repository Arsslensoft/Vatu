using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	
    [Terminal("void")]
    [Terminal("byte")]
    [Terminal("sbyte")]
    [Terminal("int")]
    [Terminal("uint")]
    [Terminal("string")]
    [Terminal("bool")]
    [Terminal("pointer")]
    [Terminal("float")]
    [Terminal("type")]
    public class TypeToken : SimpleToken
    {
       protected TypeSpec _ts;
        public TypeSpec Type
        {
            get
            {
                if (_ts != null && _ts.IsTypeDef)
                    return _ts.GetTypeDefBase(_ts);
                else return _ts;
            }
            set
            {
                _ts = value;
            }
        }
        public TypeToken()
        {
          //  Location = CompilerContext.TranslateLocation(position);

        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {

            return FlowState.Valid;
        }
       public override bool Resolve(ResolveContext rc)
        {

            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
             rc.Resolver.TryResolveType(this.symbol.Name,ref _ts);
             if (_ts == null)
                 ResolveContext.Report.Error(0, Location, "Unresolved type");
            return this;
        }
    }
   
	
	
}