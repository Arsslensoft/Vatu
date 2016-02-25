using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	
	
	public class BaseTypeIdentifier : TypeToken
    {
        TypeToken _typeid;
        Identifier _ident;
        TemplateIdentifier _tident;
        TypeIdentifierListDefinition _tidl;
        Expr exp;

         [Rule(@"<Base>     ::= ~'@'Id")]
        [Rule(@"<Base>     ::= ~typeof Id")]  
        public BaseTypeIdentifier(Identifier type)
        {
            _ident = type;
        }
         [Rule(@"<Base>     ::= TemplateId")]
         public BaseTypeIdentifier(TemplateIdentifier type)
         {
             _tident = type;
         }

            [Rule(@"<Base>     ::= ~'@'Id ~'<' <Types> ~'>'")]
         public BaseTypeIdentifier(Identifier type, TypeIdentifierListDefinition tidl)
         {
             _ident = type;
             _tidl = tidl;
         }
            [Rule(@"<Base>     ::=  ~'<' <Types> ~'>'")]
            public BaseTypeIdentifier( TypeIdentifierListDefinition tidl)
            {
          

                _tidl = tidl;
            } 
        [Rule(@"<Base> ::= ~typeof ~'(' <Expression> ~')'")]
        public BaseTypeIdentifier(Expr op)
        {
            exp = op;
        }
        [Rule(@"<Base>     ::= <Scalar>")]
        public BaseTypeIdentifier(ScalarTypeIdentifier type)
        {
            _typeid = type;
        }

     
     
       public override bool Resolve(ResolveContext rc)
        {
            if (_typeid != null)
              _typeid.Resolve(rc);

            if (_ident != null)
               rc.Resolver.TryResolveType(_ident.Name,ref _ts);


            if (_tident != null)
                rc.Resolver.TryResolveType(_tident.Name, ref _ts);
            return base.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_ident != null)
            {
                if (_tidl == null)
                    rc.Resolver.TryResolveType(_ident.Name, ref _ts);
                else
                {
                    _tidl = (TypeIdentifierListDefinition)_tidl.DoResolve(rc);
                    bool alltemplates = _tidl.Types[0] is TemplateTypeSpec;
                    // check if all non templates
                    foreach (TypeSpec t in _tidl.Types)
                    {
                        if ( !alltemplates && t is TemplateTypeSpec  || (alltemplates && !(t is TemplateTypeSpec)) )
                            ResolveContext.Report.Error(0, Location, t.Name + " is a template, a template can't define a template");
                    }
                    if (alltemplates)
                    {
                        rc.Resolver.TryResolveType(_ident.Name, ref _ts);
                        return this;
                    }
                    MemberSignature msig = new MemberSignature(rc.CurrentNamespace, _ident.Name, _tidl.Types.ToArray(), Location);
                    rc.Resolver.TryResolveType(_ident.Name, ref _ts,msig.NoNamespaceSignature); // the type signature with template is found
                    if (_ts == null)
                    {
                        rc.Resolver.TryResolveType(_ident.Name, ref _ts);
                        if (_ts != null && _ts is StructTypeSpec)
                        {
                            StructTypeSpec st = (_ts as StructTypeSpec);
                            if (st.Templates.Count != _tidl.Types.Count)
                                ResolveContext.Report.Error(0, Location, "Templates, Types count mismatch");
                            else
                            {
                                StructTypeSpec dst = st.CopyWithTemplate(_tidl.Types);
                                if (dst == null)
                                    ResolveContext.Report.Error(0, Location, "Failed to initialize type with current templates definition");
                                else
                                {
                                    rc.Resolver.KnowType(dst);
                                    _ts = dst;
                                    return this;
                                }
                            }
                        }
                        else if (_ts != null && _ts is ClassTypeSpec)
                        {
                            ClassTypeSpec st = (_ts as ClassTypeSpec);
                            if (st.Templates.Count != _tidl.Types.Count)
                                ResolveContext.Report.Error(0, Location, "Templates, Types count mismatch");
                            else
                            {
                                ClassTypeSpec dst = st.CopyWithTemplate(_tidl.Types);
                                if (dst == null)
                                    ResolveContext.Report.Error(0, Location, "Failed to initialize type with current templates definition");
                                else
                                {
                                    rc.Resolver.KnowType(dst);
                                    _ts = dst;
                                    return this;
                                }
                            }
                        }
                        else if (_ts != null && _ts is UnionTypeSpec)
                        {
                            UnionTypeSpec st = (_ts as UnionTypeSpec);
                            if (st.Templates.Count != _tidl.Types.Count)
                                ResolveContext.Report.Error(0, Location, "Templates, Types count mismatch");
                            else
                            {
                                UnionTypeSpec dst = st.CopyWithTemplate(_tidl.Types);
                                if (dst == null)
                                    ResolveContext.Report.Error(0, Location, "Failed to initialize type with current templates definition");
                                else
                                {
                                    rc.Resolver.KnowType(dst);
                                    _ts = dst;
                                    return this;
                                }
                            }
                        }

                    }
                    else return this;
                }
            }
            else if (_tidl != null)
            {
                _tidl = (TypeIdentifierListDefinition)_tidl.DoResolve(rc);
                string type_sig = "";
                if (_tidl.Types.Count == 1 || _tidl.Types.Count > 26)
                    ResolveContext.Report.Error(0, Location, "Type collection count must be greater than 1 and less or equal to 26");
                // check if all non templates
                foreach (TypeSpec t in _tidl.Types)
                    type_sig += "_"+t.GetTypeName(t).Replace("*", "P").Replace("&", "R").Replace("<", "$T_").Replace(">", "_T$");

                rc.Resolver.TryResolveType("Autogen_$TypeCollection" + type_sig, ref _ts);
                if (_ts == null)
                {
                    StructTypeSpec mt = StructTypeSpec.CreateAutoGeneratedStruct(_tidl.Types, type_sig, Location);
                    rc.Resolver.KnowType(mt);
                    _ts = mt;
                }
                return this;
            }


            if (_tident != null)
            {
                if (rc.CurrentMethodName != null)
                    rc.Resolver.TryResolveType(rc.CurrentMethodName+"_"+_tident.Name, ref _ts);
                else rc.Resolver.TryResolveType(_tident.Name, ref _ts);
            }

            if (exp != null)
            {
                exp = (Expr)exp.DoResolve(rc);
                if (exp != null)
                    Type = exp.Type;
             
                else ResolveContext.Report.Error(0, Location, "Unresolved type");
            }
            if (_typeid != null)
            {
                _typeid = (TypeToken)_typeid.DoResolve(rc);
                Type = _typeid.Type;
            }
            if (_ident != null)
            {
                rc.Resolver.TryResolveType(_ident.Name, ref _ts);
                  if(_ts == null)
                      ResolveContext.Report.Error(0, Location, "Unresolved type");
            }
          

            return this;
        }
      
    }
   
	
}