using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	 public class TypeIdentifier : TypeToken
    {
        BaseTypeIdentifier _base;
        TypePointer _pointers;
        NameIdentifier ni;
        [Rule(@"<Type>     ::= <Base> <Pointers>")]
        public TypeIdentifier(BaseTypeIdentifier tbase, TypePointer pointers)
        {
            _base = tbase;
            _pointers = pointers;
        }
        [Rule(@"<Type>     ::= <Name> ~'::' <Base> <Pointers>")]
        public TypeIdentifier(NameIdentifier ns,BaseTypeIdentifier tbase, TypePointer pointers)
        {
            ni = ns;
            _base = tbase;
            _pointers = pointers;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
       public override bool Resolve(ResolveContext rc)
        {
           
            _base.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_pointers != null)
                _pointers = (TypePointer)_pointers.DoResolve(rc);
            if (ni != null)
            {
                Namespace ns = new Namespace(ni.Name);
                rc.CurrentScope |= ResolveScopes.AccessOperation;
                Namespace lastns = rc.CurrentNamespace;
                rc.CurrentNamespace = ns;
                _base = (BaseTypeIdentifier)_base.DoResolve(rc);
                rc.CurrentNamespace = lastns;
                rc.CurrentScope &= ~ResolveScopes.AccessOperation;
            }
            else _base = (BaseTypeIdentifier)_base.DoResolve(rc);
            Type = _base.Type;
            if (_pointers != null)
            {
          
                for (int i = 0; i < _pointers.PointerCount; i++)
                    Type = Type.MakePointer();
            }
            return this;
        }

    }
   
	
	
}