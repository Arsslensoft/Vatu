using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Declarations
{
    public class DelegateDeclaration : Declaration
    {
        public DelegateTypeSpec TypeName { get; set; }

        TypeIdentifierListDefinition _tdl;
        CallingCV _ccv;
        TypeToken _ret;
        Modifier _mod;
        [Rule(@"<Delegate Decl>  ::=  <Mod> <CallCV> ~delegate <Type> Id ~'(' <Types>  ~')' ~';' ")]
        public DelegateDeclaration(Modifier mod, CallingCV ccv, TypeToken ret, Identifier id, TypeIdentifierListDefinition tid)
        {
            _ccv = ccv;
            _name = id;
            _mod = mod;
            _tdl = tid;
            _ret = ret;
        }
        [Rule(@"<Delegate Decl>  ::=  <Mod> ~typedef <CallCV> ~delegate <Type> ~'(' <Types>  ~')' Id ~';' ")]
        public DelegateDeclaration(Modifier mod, CallingCV ccv, TypeToken ret, TypeIdentifierListDefinition tid, Identifier id)
        {
            _ccv = ccv;
            _name = id;
            _mod = mod;
            _tdl = tid;
            _ret = ret;
        }

        [Rule(@"<Delegate Decl>  ::=  <Mod> <CallCV> ~delegate <Type> Id ~'('  ~')' ~';' ")]
        [Rule(@"<Delegate Decl>  ::=  <Mod> ~typedef <CallCV> ~delegate <Type> ~'('   ~')' Id ~';' ")]
        public DelegateDeclaration(Modifier mod, CallingCV ccv, TypeToken ret,  Identifier id)
        {
            _ccv = ccv;
            _name = id;
            _mod = mod;
            _tdl = null;
            _ret = ret;
        }

       public override bool Resolve(ResolveContext rc)
        {


            return true;

        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _mod = (Modifier)_mod.DoResolve(rc);
            TypeName = new DelegateTypeSpec(rc.CurrentNamespace, _name.Name, _ret.Type, new List<TypeSpec>(), _ccv.CallingConvention, _mod.ModifierList, loc);
       
            rc.KnowType(TypeName);


            List<TypeSpec> tp = new List<TypeSpec>();
            if(_tdl != null)
                 _tdl = (TypeIdentifierListDefinition)_tdl.DoResolve(rc);
            _ret = (TypeToken)_ret.DoResolve(rc);
            if (_ccv != null)
                _ccv = (CallingCV)_ccv.DoResolve(rc);
            if (_tdl != null)
            {
 
                _tdl = (TypeIdentifierListDefinition)_tdl.DoResolve(rc);
                TypeIdentifierListDefinition par = _tdl;
                int paid = 0;
                while (par != null)
                {
                    if (par._id != null)
                        tp.Add(par._id.Type);

                    par = par._nextid;
                    paid++;
                }
            }
            if (_ccv != null && _ccv.CallingConvention == CallingConventions.VatuSysCall)
                ResolveContext.Report.Error(0, Location, "Vatu system call can't be implemented using delegates");
            DelegateTypeSpec NT = new DelegateTypeSpec(rc.CurrentNamespace, _name.Name, _ret.Type, tp, _ccv.CallingConvention, _mod.ModifierList, loc);

            rc.UpdateType(TypeName, NT);

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
