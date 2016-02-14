using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class DestructorPrototypeDeclaration : Declaration
    {

        internal MethodSpec method;
        CallingConventions ccv = CallingConventions.StdCall;
        Modifiers mods = Modifiers.Private;
        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }


         Modifier mod;
        ParameterListDefinition _pal; 
        TypeIdentifierListDefinition _tdl;

        [Rule(@" <Destructor Proto> ::= <Mod>  ~'~'Id ~'(' <Types> ~')'  ~';'")]
        public DestructorPrototypeDeclaration(Modifier m, Identifier id, TypeIdentifierListDefinition tdl)
        {
            _name = id;
            _tdl = tdl;
            mod = m;
        }
        [Rule(@"<Destructor Proto> ::= <Mod>  ~'~'Id ~'(' <Params> ~')' ~';'")]
        public DestructorPrototypeDeclaration(Modifier m, Identifier id, ParameterListDefinition pal)
        {
            _name = id;
            _pal = pal;
            mod = m;
        }
        [Rule(@"<Destructor Proto> ::= <Mod> ~'~'Id ~'(' ~')' ~';'")]
        public DestructorPrototypeDeclaration(Modifier m, Identifier id)
        {
            _name = id;
            _pal = null;
            mod = m;
        }



       public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;

            if (_pal != null)
                ok &= _pal.Resolve(rc);

            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            mod = (Modifier)mod.DoResolve(rc);

            ccv = CallingConventions.Default;
            mods = mod.ModifierList;
           mods |= Modifiers.Prototype;
   
            method = new MethodSpec(rc.CurrentNamespace, _name.Name+"_$dtor", mods | Modifiers.Prototype,BuiltinTypeSpec.Void, ccv, null, this.Location);
            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            List<TypeSpec> tp = new List<TypeSpec>();

         

            if (_pal != null)
            {
               
                _pal = (ParameterListDefinition)_pal.DoResolve(rc);
                ParameterListDefinition par = _pal;
                while (par != null)
                {
                    if (par._id != null)
                    {
                        Params.Push(par._id.ParameterName);
                        Parameters.Add(par._id.ParameterName);
                        tp.Add(par._id.ParameterName.MemberType);
                    }
                    par = par._nextid;
                }

            }
            else if (_tdl != null)
            {
       
                _tdl = (TypeIdentifierListDefinition)_tdl.DoResolve(rc);
                TypeIdentifierListDefinition par = _tdl;
                int paid = 0;
                while (par != null)
                {
                    if (par._id != null)
                    {
                        ParameterSpec p = new ParameterSpec(rc.CurrentNamespace, "param_" + paid, method, par._id.Type, par.Location, 4);
                        Parameters.Add(p);
                        Params.Push(p);
                        tp.Add(p.MemberType);
                    }
                    par = par._nextid;
                    paid++;
                }
            }
      

            rc.CurrentMethod = method;


        
                tp.Insert(0, rc.CurrentType);
            method = new MethodSpec(rc.CurrentNamespace, _name.Name+"_$dtor", mods | Modifiers.Prototype,BuiltinTypeSpec.Void, ccv, tp.ToArray(), this.Location);
        
            method.Parameters = Parameters;

        
            rc.CurrentMethod = method;

            ParameterSpec thisps = new ParameterSpec(rc.CurrentNamespace, "this", method, rc.CurrentType, Location, 4);
                    Parameters.Insert(0, thisps);
                    method.Parameters = Parameters;


                    rc.KnowMethod(method);
       
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            fc.AddNew(method);

           
            return base.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            if ((mods & Modifiers.Extern) == Modifiers.Extern)
                 ec.DefineExtern(method);
            return true;
        }
    }
}
