using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class MethodDeclaration : Declaration
    {
        internal MethodSpec method;
        Modifiers mods = Modifiers.Private;
        CallingConventions ccv = CallingConventions.StdCall;
        CallingConventionsHandler ccvh;
        FunctionBodyDefinition _fbd;
        
        Specifiers specs;
        public List<ParameterSpec> Parameters;

  
        MethodIdentifier _id;
        [Rule(@"<Func Decl> ::= <Func ID> <Func Body>")]
        public MethodDeclaration(MethodIdentifier id, FunctionBodyDefinition fbd)
        {
            _name = id.Id;
            _id = id;
 
            _fbd = fbd;
        }

    
   
       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _id.Resolve(rc);

            if (_fbd._pdef != null)
                ok &= _fbd._pdef.Resolve(rc);
            if (_fbd._b != null)
                ok &= _fbd._b.Resolve(rc);
            return ok;
        }
       public override SimpleToken DoResolve(ResolveContext rc)
        {
            ccvh = new CallingConventionsHandler();
            _id = (MethodIdentifier)_id.DoResolve(rc);
            ccv = _id.CV;
            mods = _id.Mods;
            specs = _id.Specs;
            _fbd = (FunctionBodyDefinition)_fbd.DoResolve(rc);
            Parameters = _fbd.Parameters;
           
            base._type = _id.TType;


            if ((specs & Specifiers.Entry) == Specifiers.Entry && CompilerContext.EntryPointFound)
                ResolveContext.Report.Error(0, Location, "Entry point already defined");
            else if ((specs & Specifiers.Entry) == Specifiers.Entry)
                CompilerContext.EntryPointFound = true;

            if ((specs & Specifiers.Variadic) == Specifiers.Variadic && ccv != CallingConventions.Cdecl && ccv != CallingConventions.VatuSysCall)
                 ResolveContext.Report.Error(0, Location, "Variadic functions can only be used with cdecl & syscall calling conventions");

            if (_fbd._ext != null && !_fbd._ext.Static)
                _fbd.ParamTypes.Insert(0, _fbd._ext.ExtendedType);
            else if (rc.IsInClass)
                _fbd.ParamTypes.Insert(0, rc.CurrentType);

        if (_fbd._ext != null && !_fbd._ext.Static)
            method = new MethodSpec(rc.CurrentNamespace, _fbd._ext.ExtendedType.NormalizedName + "$_" + _id.Name, mods, _id.TType.Type, ccv, _fbd.ParamTypes.ToArray(), this._id.Location);
            else if(rc.IsInClass)
            method = new MethodSpec(rc.CurrentNamespace, rc.CurrentType.NormalizedName + "$_" + _id.Name, mods, _id.TType.Type, ccv, _fbd.ParamTypes.ToArray(), this._id.Location);
            else
            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods, _id.TType.Type, ccv, _fbd.ParamTypes.ToArray(), this._id.Location);

            // reserve first param for extension
            if (_fbd._ext != null && !_fbd._ext.Static)
            {
                ParameterSpec thisps = new ParameterSpec(rc.CurrentNamespace, "this", method, _fbd._ext.ExtendedType, Location, 4, Modifiers.Ref);
                Parameters.Insert(0, thisps);
            }
            else if(rc.IsInClass)
            {
                ParameterSpec thisps = new ParameterSpec(rc.CurrentNamespace, "this", method, rc.CurrentType, Location, 4);
                Parameters.Insert(0, thisps);
            }
            // Calling Convention
            int last_param = 4;
            ccvh.SetParametersIndex(ref Parameters, ccv,ref last_param);
            if (ccv == CallingConventions.FastCall)
                ccvh.ReserveFastCall(rc, Parameters);
            else if (ccv == CallingConventions.VatuSysCall && _id.CCV != null)
            {
                method.VSCDescriptor = _id.CCV.Descriptor;
                method.VSCInterrupt = _id.CCV.Interrupt;

                if (Parameters.Count > 0)
                    ResolveContext.Report.Error(0, Location, "Vatu system call requires pure variadic function, parameters definition is not allowed");

            }

            method.Parameters = Parameters;


            if ((ccv == CallingConventions.FastCall) && Parameters.Count >= 1 && Parameters[0].MemberType.Size > 2)
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1");
            else if ((ccv == CallingConventions.FastCall ) && Parameters.Count >= 2 && (Parameters[0].MemberType.Size > 2 || Parameters[1].MemberType.Size > 2))
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1,2");
             // variadic
              method.IsVariadic = (specs & Specifiers.Variadic) == Specifiers.Variadic;

          
                MethodSpec m = null;
                rc.Resolver.TryResolveMethod(method.Signature.ToString(), ref m);
                if (m != null && (m.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate method signature");
                else if (m == null)
                {
                //    if (rc.CurrentType == null || (rc.CurrentType != null && !(rc.CurrentType is ClassTypeSpec)))
                       rc.KnowMethod(method);
                }
                method.LastParameterEndIdx = (ushort)last_param;
            rc.CurrentMethod = method;
            if (method.IsVariadic) // reserve local variable index for variadic
            {
                VarSpec paramsv = new VarSpec(rc.CurrentNamespace, "params", method, BuiltinTypeSpec.Pointer, Location, 0, Modifiers.Const, false);
                rc.KnowVar(paramsv);
            }


            if (specs == Specifiers.Isolated && method.MemberType != BuiltinTypeSpec.Void)
                ResolveContext.Report.Error(45, Location, "only void methods can be isolated.");

            if (method.memberType is ArrayTypeSpec)
                ResolveContext.Report.Error(45, Location, "return type must be non array type " + method.MemberType.ToString() + " is user-defined type.");
          
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
            if ((specs & Specifiers.Entry) == Specifiers.Entry)
                ec.SetEntry(method.Signature.ToString());


            Label mlb = ec.DefineLabel(method.Signature.ToString());
            mlb.Method = true;
            ec.MarkLabel(mlb);
            ec.EmitComment("Method: Name = " + method.Name + ", EntryPoint = " + (specs == Specifiers.Entry));
            // create stack frame
            ec.EmitComment("create stackframe");
            ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
            ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });
            if ((specs & Specifiers.Isolated) == Specifiers.Isolated)
                ec.EmitInstruction(new Pushad());
            // allocate variables

            ushort size = 0;
            List<VarSpec> locals = ec.CurrentResolve.GetLocals();
            foreach (VarSpec v in locals)
                size += (ushort)(v.memberType.IsArray ? v.memberType.GetSize(v.memberType) : v.MemberType.Size);

            // fast call
            if (ccv == CallingConventions.FastCall)
            {
                if (Parameters.Count >= 2)
                {
                    size += 4;
                    ccvh.EmitFastCall(ec, 2);
                }
                else if (Parameters.Count == 1)
                {
                    size += 2;
                    ccvh.EmitFastCall(ec, 1);
                }
            }
          

            // Variadic Parameter Start Assign
            if (method.IsVariadic )
            {
                ec.EmitComment("Variadic Parameter Offset Copy");
                ushort off = 4;
                if (Parameters.Count > 0)
                    off = (ushort)GetNextIndex(Parameters[Parameters.Count - 1]);
                if(ccv != CallingConventions.VatuSysCall)
                     ec.EmitInstruction(new Lea() { DestinationReg = EmitContext.SI, SourceReg = EmitContext.BP, SourceDisplacement = off, SourceIsIndirect = true });
               
                ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, DestinationIsIndirect = true, DestinationDisplacement = -2, SourceReg = EmitContext.SI});
            }
          
            if (size != 0)         // no allocation
                ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });
            //EMit params
            if (Parameters.Count > 0)
            {
                ec.EmitComment("Parameters Definitions");
                foreach (ParameterSpec par in Parameters)
                    ec.EmitComment("Parameter " + par.Name + " @BP" + par.StackIdx);

            }
            if (method.IsVariadic)
                ec.EmitComment("Variadic Pointer Address @BP-2");

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
            // entry infinite loop
            if (specs == Specifiers.Entry)
            {
                ec.EmitComment("entry infinite loop");
                ec.EmitInstruction(new Jump() { DestinationLabel = method.Signature + "_ret" });
            }
            // Destroy Stack Frame
            if ((specs & Specifiers.Isolated) == Specifiers.Isolated)
                ec.EmitInstruction(new Popad());

            ec.EmitComment("destroy stackframe");
            ec.EmitInstruction(new Leave());
            // ret
            ccvh.EmitDecl(ec, ref Parameters, ccv);
            return true;
        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
          
            if (specs == Specifiers.Entry)
                fc.MarkAsUsed(method);
            else
                 fc.AddNew(method);




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
