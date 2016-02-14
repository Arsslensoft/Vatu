using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class Declaration : DeclarationToken
    {
        public Declaration BaseDeclaration { get; set; }
        TypeSpec _ts;
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

        protected Identifier _name;
        protected TypeToken _type;
        public bool IsTypeDef { get { return (BaseDeclaration is StructDeclaration) || (BaseDeclaration is TypeDefDeclaration) || (BaseDeclaration is EnumDeclaration); } }
        public bool IsStruct { get { return (BaseDeclaration is StructDeclaration); } }
        public bool IsUnion { get { return (BaseDeclaration is UnionDeclaration); } }
        public bool IsInClass { get { return (BaseDeclaration is ClassDeclaration); } }
        public TypeToken TypeTok
        {
            get { return _type; }
        }
        public Identifier Identifier
        {
            get { return _name; }
        }


        public Declaration()
        {

        }
       
    
        [Rule(@" <Decl>  ::= <ASM Decl>")]
        [Rule(@"<Decl>  ::= <Var Decl>  ")]
        [Rule(@"<Decl>  ::= <Types Decl>")]
        [Rule(@"<Decl>  ::= <Method Decl>")]
        [Rule(@"<Decl>  ::= <Prototypes Decl>")]

        [Rule(@"<Types Decl> ::= <Union Decl>")]
        [Rule(@"<Types Decl>  ::= <Struct Decl>")]
        [Rule(@"<Types Decl>  ::= <Enum Decl>")]
        [Rule(@"<Types Decl> ::= <Typedef Decl> ")]
        [Rule(@"<Types Decl> ::=  <Delegate Decl> ")]
        public Declaration(Declaration decl)
        {
            BaseDeclaration = decl;
        }

       public override bool Resolve(ResolveContext rc)
        {
            if (BaseDeclaration != null)
                return BaseDeclaration.Resolve(rc);
            return base.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (BaseDeclaration != null)
                return BaseDeclaration.DoResolve(rc);
            else return base.DoResolve(rc);
        }
   
      
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (BaseDeclaration != null)
                return BaseDeclaration.DoFlowAnalysis(fc);

            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            if (BaseDeclaration != null)
                return BaseDeclaration.Emit(ec);

            return base.Emit(ec);
        }

    }
}
