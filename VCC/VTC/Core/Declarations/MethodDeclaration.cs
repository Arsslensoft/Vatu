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
        MethodSpec method;
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

            if ((specs & Specifiers.Variadic) == Specifiers.Variadic && ccv != CallingConventions.Cdecl && ccv != CallingConventions.VeryFastCall)
                 ResolveContext.Report.Error(0, Location, "Variadic functions can only be used with cdecl & syscall calling conventions");

            if (_fbd._ext != null && !_fbd._ext.Static)
                _fbd.ParamTypes.Insert(0, _fbd._ext.ExtendedType);

            method = new MethodSpec(rc.CurrentNamespace, _id.Name, mods, _id.TType.Type, ccv, _fbd.ParamTypes.ToArray(), this.loc);

            // reserve first param for extension
            if (_fbd._ext != null && !_fbd._ext.Static)
            {
                ParameterSpec thisps = new ParameterSpec("this", method, _fbd._ext.ExtendedType, loc, 4, Modifiers.Ref);
                Parameters.Insert(0, thisps);
            }

            // Calling Convention
            ccvh.SetParametersIndex(ref Parameters, ccv);
            if (ccv == CallingConventions.FastCall)
                ccvh.ReserveFastCall(rc, Parameters);
            else if (ccv == CallingConventions.VeryFastCall)
                ccvh.ReserveVFastCall(rc, Parameters);

            method.Parameters = Parameters;


            if ((ccv == CallingConventions.FastCall || ccv == CallingConventions.VeryFastCall) && Parameters.Count >= 1 && Parameters[0].MemberType.Size > 2)
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1");
            else if ((ccv == CallingConventions.FastCall || ccv == CallingConventions.VeryFastCall) && Parameters.Count >= 2 && (Parameters[0].MemberType.Size > 2 || Parameters[1].MemberType.Size > 2))
                ResolveContext.Report.Error(9, Location, "Cannot use fast call with struct or union parameter at index 1 or 2");
            else if (ccv == CallingConventions.VeryFastCall && Parameters.Count >= 3 && (Parameters[0].MemberType.Size > 2 || Parameters[1].MemberType.Size > 2 || Parameters[2].MemberType.Size > 2))
                ResolveContext.Report.Error(9, Location, "Cannot use very fast call with struct or union parameter at index 1 , 2 or 3");
            else if (ccv == CallingConventions.VeryFastCall && Parameters.Count >= 4 && (Parameters[0].MemberType.Size > 2 || Parameters[1].MemberType.Size > 2 || Parameters[2].MemberType.Size > 2 || Parameters[3].MemberType.Size > 2))
                ResolveContext.Report.Error(9, Location, "Cannot use very fast call with struct or union parameter at index 1, 2, 3 or 4");
                 
             // variadic
              method.IsVariadic = (specs & Specifiers.Variadic) == Specifiers.Variadic;

            if (_fbd._ext == null)
            {
                MethodSpec m = null;
                rc.Resolver.TryResolveMethod(method.Signature.ToString(), ref m);
                if (m != null && (m.Modifiers & Modifiers.Prototype) == 0)
                    ResolveContext.Report.Error(9, Location, "Duplicate method signature");
                else if (m == null)
                    rc.KnowMethod(method);
            }else {
            // extension
            
              if (!rc.Extend(_fbd._ext.ExtendedType, method, _fbd._ext.Static))
                    ResolveContext.Report.Error(45, Location, "Another method with same signature has already extended this type.");
           
            }

            rc.CurrentMethod = method;
            if (method.IsVariadic) // reserve local variable index for variadic
            {
                VarSpec paramsv = new VarSpec(rc.CurrentNamespace, "params", method, BuiltinTypeSpec.Pointer, Location, 0, Modifiers.Const, false);
                rc.KnowVar(paramsv);
            }


            if (specs == Specifiers.Isolated && method.MemberType != BuiltinTypeSpec.Void)
                ResolveContext.Report.Error(45, Location, "only void methods can be isolated.");

            if (!method.MemberType.IsBuiltinType)
                ResolveContext.Report.Error(45, Location, "return type must be builtin type " + method.MemberType.ToString() + " is user-defined type.");
          
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
            else if (ccv == CallingConventions.VeryFastCall)
            {
                if (Parameters.Count >= 4)
                {
                    size += 8;
                    ccvh.EmitVFastCall(ec, 4);
                }
                else if (Parameters.Count >= 3)
                {
                    size += 6;
                    ccvh.EmitVFastCall(ec, 3);
                }
                else if (Parameters.Count >= 2)
                {
                    size += 4;
                    ccvh.EmitVFastCall(ec, 2);
                }
                else if (Parameters.Count == 1)
                {
                    size += 2;
                    ccvh.EmitVFastCall(ec, 1);
                }
            }
            // Variadic Parameter Start Assign
            if (method.IsVariadic)
            {
                ec.EmitComment("Variadic Parameter Offset Copy");
                ushort off = 4;
                if (Parameters.Count > 0)
                    off = (ushort)GetNextIndex(Parameters[Parameters.Count - 1]);

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
                    ec.EmitComment("Local " + v.Name + " @BP" + v.StackIdx);
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
            fc.CodePathReturn.PathLocation = _id.Location;
            if (specs == Specifiers.Entry)
                fc.MarkAsUsed(method);
            else
                 fc.AddNew(method);
           
            
            

           fc.NoReturnCheck =  _type.Type.Equals(BuiltinTypeSpec.Void);

            if ( _fbd != null && _fbd._b != null)
                return _fbd._b.DoFlowAnalysis(fc);
            else
                return base.DoFlowAnalysis(fc);
        }

       
    }
}
