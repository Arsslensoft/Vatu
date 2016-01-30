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
        Expr exp;

         [Rule(@"<Base>     ::= ~'@'Id")]
        [Rule(@"<Base>     ::= ~typeof Id")]  
        public BaseTypeIdentifier(Identifier type)
        {
            _ident = type;
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
           
            return base.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_ident != null)
                rc.Resolver.TryResolveType(_ident.Name, ref _ts);

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