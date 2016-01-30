using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	
	 public class ScalarTypeIdentifier : TypeToken
    {
       TypeToken _type;

      
        [Rule(@"<Scalar>   ::= byte")]
        [Rule(@"<Scalar>   ::= int")]
        [Rule(@"<Scalar>   ::= uint")]
        [Rule(@"<Scalar>   ::= sbyte")]
        [Rule(@"<Scalar>   ::= bool")]
        [Rule(@"<Scalar>   ::= void")]
        [Rule(@"<Scalar>   ::= string")]
        [Rule(@"<Scalar>   ::= pointer")]
        [Rule(@"<Scalar>   ::= float")]
        public ScalarTypeIdentifier(TypeToken type)
        {
          
          _type = type;
        }
      
   
       public override bool Resolve(ResolveContext rc)
        {
            _type.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
           _type = (TypeToken)_type.DoResolve(rc);
           Type = _type.Type;
            return  this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _type.DoFlowAnalysis(fc);
        }
        
    }
 
	
}