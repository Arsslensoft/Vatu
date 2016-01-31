using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class MethodPrototypeDeclaration : Declaration
    {
        FunctionExtensionDefinition ext;
        MethodSpec method;
        CallingConventions ccv = CallingConventions.StdCall;
        Modifiers mods = Modifiers.Private;
        Specifiers specs = Specifiers.NoSpec;
        Stack<ParameterSpec> Params { get; set; }
        public List<ParameterSpec> Parameters { get; set; }


        MethodIdentifier _id; 
        ParameterListDefinition _pal; 
        TypeIdentifierListDefinition _tdl;
   
        [Rule(@" <Func Proto> ::= <Func ID> ~'(' <Types> ~')' <Func Ext> ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id, TypeIdentifierListDefinition tdl, FunctionExtensionDefinition _ext)
        {
            _id = id;
            _tdl = tdl;
            ext = _ext;
        }
        [Rule(@"<Func Proto> ::= <Func ID> ~'(' <Params> ~')' <Func Ext> ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id, ParameterListDefinition pal, FunctionExtensionDefinition _ext)
        {
            _id = id;
            _pal = pal;
            ext = _ext;
        }
        [Rule(@"<Func Proto> ::= <Func ID> ~'(' ~')' <Func Ext> ~';'")]
        public MethodPrototypeDeclaration(MethodIdentifier id, FunctionExtensionDefinition _ext)
        {
            ext = _ext;
            _id = id;
            _pal = null;

        }



       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _id.Resolve(rc);

            if (_pal != null)
                ok &= _pal.Resolve(rc);

            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            _id = (MethodIdentifier)_id.DoResolve(rc);
            specs = _id.Specs;
            ccv = _id.CV;
            mods = _id.Mods;
            mods |= Modifiers.Prototype;
   
            base._type = _id.TType;
            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods | Modifiers.Prototype, _id.TType.Type, ccv, null, this.loc);
            Params = new Stack<ParameterSpec>();
            Parameters = new List<ParameterSpec>();
            List<TypeSpec> tp = new List<TypeSpec>();

            if (ext != null && ext.IsExtended)
                ext = (FunctionExtensionDefinition)ext.DoResolve(rc);
            else ext = null;
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
                        ParameterSpec p = new ParameterSpec("param_" + paid, method, par._id.Type, par.loc, 4);
                        Parameters.Add(p);
                        Params.Push(p);
                        tp.Add(p.MemberType);
                    }
                    par = par._nextid;
                    paid++;
                }
            }
            if (ccv == CallingConventions.FastCall && Parameters.Count >= 1 && Parameters[0].MemberType.Size > 2)
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1");
            else if (ccv == CallingConventions.FastCall && Parameters.Count >= 2 && (Parameters[0].MemberType.Size > 2 || Parameters[1].MemberType.Size > 2))
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1 or 2");


            rc.CurrentMethod = method;


            if (!method.MemberType.IsBuiltinType)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type " + method.MemberType.ToString() + " is user-defined type.");
            if (ext != null && !ext.Static)
                tp.Insert(0, ext.ExtendedType);
            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods | Modifiers.Prototype, _id.TType.Type, ccv, tp.ToArray(), this.loc);
            method.IsVariadic = (specs & Specifiers.Variadic) == Specifiers.Variadic;
            method.Parameters = Parameters;
            rc.CurrentMethod = method;
            // extension
            if (ext != null)
            {
                // insert this
                if (!ext.Static)
                {
                   
                    ParameterSpec thisps = new ParameterSpec("this", method, ext.ExtendedType, loc, 4);
                    Parameters.Insert(0, thisps);
                    method.Parameters = Parameters;
                }

            if (!rc.Extend(ext.ExtendedType, method, ext.Static))
                    ResolveContext.Report.Error(45, Location, "Another method with same signature has already extended this type.");
            
            }
            else rc.KnowMethod(method);


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
