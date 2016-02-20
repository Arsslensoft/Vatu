using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core.Declarations
{
    public class DelegateDeclaration : Declaration
    {
        public Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters;
        public List<TypeSpec> ParamTypes { get; set; }
        public DelegateTypeSpec TypeName { get; set; }
        CallingConventionsHandler ccvh;
        ParameterListDefinition _tdl;
        CallingCV _ccv;
        TypeToken _ret;
        Modifier _mod;
        [Rule(@"<Delegate Decl>  ::=  <Mod> <CallCV> ~delegate <Type> Id ~'(' <Params>  ~')' ~';' ")]
        public DelegateDeclaration(Modifier mod, CallingCV ccv, TypeToken ret, Identifier id, ParameterListDefinition tid)
        {
            _ccv = ccv;
            _name = id;
            _mod = mod;
            _tdl = tid;
            _ret = ret;
        }
    

        [Rule(@"<Delegate Decl>  ::=  <Mod> <CallCV> ~delegate <Type> Id ~'('  ~')' ~';' ")]
        public DelegateDeclaration(Modifier mod, CallingCV ccv, TypeToken ret,  Identifier id)
        {
            _ccv = ccv;
            _name = id;
            _mod = mod;
            _tdl = null;
            _ret = ret;
        }
        int GetParameterSize(TypeSpec tp, bool reference)
        {
            if (reference)
                return 2;
            else if (tp.IsFloat && tp.IsPointer)
                return 2;
            else
            {
                if (tp.Size != 1 && tp.Size % 2 != 0)
                    return (tp.Size == 1) ? 2 : tp.Size + 1;
                else return (tp.Size == 1) ? 2 : tp.Size;
            }
        }
       public override bool Resolve(ResolveContext rc)
        {


            return true;

        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            ccvh = new CallingConventionsHandler();
            _mod = (Modifier)_mod.DoResolve(rc);
            TypeSpec type = null;
            if (rc.Resolver.TryResolveType(_name.Name, ref type))
                ResolveContext.Report.Error(0, Location, "Duplicate type declaration ");
            TypeName = new DelegateTypeSpec(rc.CurrentNamespace, _name.Name, _ret.Type, new List<TypeSpec>(), _ccv.CallingConvention, _mod.ModifierList,new List<ParameterSpec>(), Location);
       
            rc.KnowType(TypeName);


       
            _ret = (TypeToken)_ret.DoResolve(rc);
            if (_ccv != null)
                _ccv = (CallingCV)_ccv.DoResolve(rc);

            ParamTypes = new List<TypeSpec>();

            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            if (_tdl != null)
            {

                _tdl = (ParameterListDefinition)_tdl.DoResolve(rc);
                ParameterListDefinition par = _tdl;
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

            if (_ret.Type is ArrayTypeSpec)
                ResolveContext.Report.Error(45, Location, "return type must be non array type " + _ret.Type.ToString() + " is user-defined type.");
          
            if (_ccv != null && _ccv.CallingConvention == CallingConventions.VatuSysCall)
                ResolveContext.Report.Error(0, Location, "Vatu system call can't be implemented using delegates");

            int last_param = 4;
            ccvh.SetParametersIndex(ref Parameters, _ccv.CallingConvention, ref last_param);

            DelegateTypeSpec NT = new DelegateTypeSpec(rc.CurrentNamespace, _name.Name, _ret.Type, ParamTypes, _ccv.CallingConvention, _mod.ModifierList, Parameters, Location);
     NT.LastParameterIdx = last_param;
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
