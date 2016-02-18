using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class ConstructorDeclaration : Declaration
    {
      internal  MethodSpec method;
        Modifiers mods = Modifiers.Private;
        CallingConventions ccv = CallingConventions.StdCall;
        FunctionBodyDefinition _fbd;
        CallingConventionsHandler ccvh;
        public List<ParameterSpec> Parameters;

        Modifier mod;
        string name;

        [Rule(@"<Constructor Decl> ::= <Mod> Id <Func Body>")]
        public ConstructorDeclaration(Modifier m,Identifier id, FunctionBodyDefinition fbd)
        {
            _name = id;
            mod = m;
            _fbd = fbd;
        }

    
   
       public override bool Resolve(ResolveContext rc)
        {
            bool ok = true;

            if (_fbd._pdef != null)
                ok &= _fbd._pdef.Resolve(rc);
            if (_fbd._b != null)
                ok &= _fbd._b.Resolve(rc);
            return ok;
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
            ccvh = new CallingConventionsHandler();
            mod = (Modifier)mod.DoResolve(rc);
            ccv = CallingConventions.Default;
            mods = mod.ModifierList;
  
            _fbd = (FunctionBodyDefinition)_fbd.DoResolve(rc);
            Parameters = _fbd.Parameters;
           
          //  base._type.Type = BuiltinTypeSpec.Void;

            if (rc.CurrentType.Name != _name.Name)
                ResolveContext.Report.Error(0, Location, "Constructor's name must match the current type name");

         
                _fbd.ParamTypes.Insert(0,rc.CurrentType);

            method = new MethodSpec(rc.CurrentNamespace, _name.Name+"_$ctor", mods, BuiltinTypeSpec.Void, ccv, _fbd.ParamTypes.ToArray(), this.Location);

            // reserve first param for extension
            if (_fbd._ext != null && !_fbd._ext.Static)
                ResolveContext.Report.Error(0, Location, "Constructors can't extend a type");

            ParameterSpec thisps = new ParameterSpec(rc.CurrentNamespace, "this", method, rc.CurrentType, Location, 4);
            Parameters.Insert(0, thisps);
            int last_param = 4;
            // Calling Convention
            ccvh.SetParametersIndex(ref Parameters, ccv,ref last_param);
        

            method.Parameters = Parameters;

     
            // extension

            MethodSpec m = null;
            rc.Resolver.TryResolveMethod(method.Signature.ToString(), ref m);
            if (m != null && (m.Modifiers & Modifiers.Prototype) == 0)
                ResolveContext.Report.Error(9, Location, "Duplicate method signature");
            else if (m == null)
                rc.KnowMethod(method);

            method.LastParameterEndIdx = (ushort)last_param;
            rc.CurrentMethod = method;
         


          if (_fbd._b != null)
                _fbd._b = (Block)_fbd._b.DoResolve(rc);


         
            return this;
        }
         int GetNextIndex(ParameterSpec p)
 {
    int paramidx = (p.MemberType.Size == 1) ? 2 : p.MemberType.Size;

     if (p.MemberType.Size != 1 && p.MemberType.Size % 2 != 0)
         paramidx++;

     return paramidx + p.StackIdx;
 }
        public override bool Emit(EmitContext ec)
        {
            if ((mods & Modifiers.Extern) == Modifiers.Extern)
                ec.DefineGlobal(method);
          

            Label mlb = ec.DefineLabel(method.Signature.ToString());
            mlb.Method = true;
            ec.MarkLabel(mlb);
            ec.EmitComment("Constructor: Name = " + method.Name );
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });
       
            // allocate variables

            ushort size = 0;
            List<VarSpec> locals = ec.CurrentResolve.GetLocals();
            foreach (VarSpec v in locals)
                size += (ushort)(v.memberType.IsArray ? v.memberType.GetSize(v.memberType) : v.MemberType.Size);

      
  
            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });
            //EMit params
            if (Parameters.Count > 0)
            {
                ec.EmitComment("Parameters Definitions");
                foreach (ParameterSpec par in Parameters)
                    ec.EmitComment("Parameter " + par.Name + " @BP" + par.StackIdx);

            }
     

            if (locals.Count > 0)
            {
                ec.EmitComment("Local Vars Definitions");
                foreach (VarSpec v in locals)
                    ec.EmitComment("Local " + v.Name + " @BP" + v.VariableStackIndex);
            }
            ec.EmitComment("Block");
            // Emit Code
            if (_fbd._b != null)
                _fbd._b.Emit(ec);

            ec.EmitComment("return label");
            // Return Label
            ec.MarkLabel(ec.DefineLabel(method.Signature + "_ret"));

            ec.EmitComment("Set object type");
            ec.EmitPush(Parameters[0].memberType.TypeDescriptor);
            Parameters[0].ValueOfStack(ec);

            ec.EmitComment("Initialize Methods addresses");
            foreach (TypeMemberSpec m in (Parameters[0].memberType as ClassTypeSpec).Members)
            {
                if (m.IsMethod)
                {
                    ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(m.DefaultMethod.Signature.ToString()), Size = 16 });
                    Parameters[0].ValueOfStackAccess(ec, m.Index,m.memberType);
                }
            }
            ec.EmitComment("destroy stackframe");
            ec.EmitInstruction(new Leave());
            // ret
            ccvh.EmitDecl(ec, ref Parameters, ccv);
            return true;
        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
             fc.MarkAsUsed(method);
            fc.LookForUnreachableBrace = !fc.NoReturnCheck;
           fc.NoReturnCheck =  _type.Type.Equals(BuiltinTypeSpec.Void);
           FlowState fs = FlowState.Valid;
            if ( _fbd != null && _fbd._b != null)
                fs =  _fbd._b.DoFlowAnalysis(fc);
            else
                fs = base.DoFlowAnalysis(fc);

            if (!fs.Reachable.IsUnreachable && !fc.NoReturnCheck)
                fc.ReportNotAllCodePathsReturns(Location);

            fc.LookForUnreachableBrace = false;
            return fs;
        }

       
    }
}
