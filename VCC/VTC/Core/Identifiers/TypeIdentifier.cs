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
        ArrayVariableDefinition _avd;
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
        [Rule(@"<Type>     ::= <Base> <Pointers> <Array>")]
        public TypeIdentifier(BaseTypeIdentifier tbase, TypePointer pointers,ArrayVariableDefinition avd)
        {
            _base = tbase;
            _pointers = pointers;
            _avd = avd;
        }
        [Rule(@"<Type>     ::= <Name> ~'::' <Base> <Pointers> <Array>")]
        public TypeIdentifier(NameIdentifier ns, BaseTypeIdentifier tbase, TypePointer pointers,ArrayVariableDefinition avd)
        {
            ni = ns;
            _base = tbase;
            _pointers = pointers;
            _avd = avd;
        }

        [Rule(@"<Type>     ::= <Base>")]
        public TypeIdentifier(BaseTypeIdentifier tbase)
        {
            _base = tbase;

        }
        [Rule(@"<Type>     ::= <Name> ~'::' <Base>")]
        public TypeIdentifier(NameIdentifier ns, BaseTypeIdentifier tbase)
        {
            ni = ns;
            _base = tbase;

        }
        [Rule(@"<Type>     ::= <Base> <Array>")]
        public TypeIdentifier(BaseTypeIdentifier tbase, ArrayVariableDefinition avd)
        {
            _base = tbase;

            _avd = avd;
        }
        [Rule(@"<Type>     ::= <Name> ~'::' <Base> <Array>")]
        public TypeIdentifier(NameIdentifier ns, BaseTypeIdentifier tbase, ArrayVariableDefinition avd)
        {
            ni = ns;
            _base = tbase;

            _avd = avd;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return base.DoFlowAnalysis(fc);
        }
       public override bool Resolve(ResolveContext rc)
        {
            if (_avd != null)
                _avd.Resolve(rc);
            _base.Resolve(rc);
            return true;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_pointers != null)
                _pointers = (TypePointer)_pointers.DoResolve(rc);

            if (_avd != null)
                _avd = (ArrayVariableDefinition)_avd.DoResolve(rc);
         
            if (ni != null)
            {

                if (ni.Name == "global")
                    ResolveContext.Report.Error(0, Location, "Global namespace cannot be used for access");

                Namespace ns = rc.Resolver.ResolveNS(ni.Name);

                if (Namespace.Default == ns)
            
                    // check child
                    ns = rc.Resolver.ResolveNS(rc.CurrentNamespace.Name + "::" + ni.Name);


                if (Namespace.Default == ns)
                    ResolveContext.Report.Error(0, Location, "Unknown namespace");
               

                

                rc.CreateNewState();
                rc.CurrentNamespace = ns;

                _base = (BaseTypeIdentifier)_base.DoResolve(rc);
                
                rc.RestoreOldState();
            }
            else _base = (BaseTypeIdentifier)_base.DoResolve(rc);
            Type = _base.Type;
            if (_pointers != null)
                Type = _pointers.CreateType(Type, _pointers);
            
            if (_avd != null)
                Type = _avd.CreateArrayType(Type);
            return this;
        }

    }
   
	
	
}