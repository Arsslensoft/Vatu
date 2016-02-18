using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class PropertyDeclaration : Declaration
    {
        internal PropertySpec property;
        Modifiers mods = Modifiers.Private;
        CallingConventions ccv = CallingConventions.StdCall;
        CallingConventionsHandler ccvh;
        GetterDefinition _getter;
        SetterDefinition _setter;
        public ResolveContext GetterResolve { get; set; }
        public ResolveContext SetterResolve { get; set; }



        Specifiers specs;


        List<ParameterSpec> GetterParameters = new List<ParameterSpec>();
        List<ParameterSpec> SetterParameters = new List<ParameterSpec>();
        MethodIdentifier _id;
        [Rule(@"<Property Decl> ::= <Func ID>  ~'{' <Getter Decl> <Setter Decl> ~'}'")]
        public PropertyDeclaration(MethodIdentifier id, GetterDefinition getter,SetterDefinition setter)
        {
            _name = id.Id;
            _id = id;
            _getter = getter;
            _setter = setter;
        
        }
        [Rule(@"<Property Decl> ::= <Func ID>  ~'{' <Getter Decl> ~'}'")]
        public PropertyDeclaration(MethodIdentifier id, GetterDefinition getter)
        {
            _name = id.Id;
            _id = id;
            _getter = getter;
            _setter = null;

        }
        [Rule(@"<Property Decl> ::= <Func ID>  ~'{' <Setter Decl> ~'}'")]
        public PropertyDeclaration(MethodIdentifier id,  SetterDefinition setter)
        {
            _name = id.Id;
            _id = id;
            _getter = null;
            _setter = setter;

        }
        [Rule(@"<Property Decl> ::= <Func ID>  ~'{' <Setter Decl> <Getter Decl> ~'}'")]
        public PropertyDeclaration(MethodIdentifier id, SetterDefinition setter, GetterDefinition getter)
        {
            _name = id.Id;
            _id = id;
            _getter = getter;
            _setter = setter;

        }

       public override bool Resolve(ResolveContext rc)
        {
            bool ok = _id.Resolve(rc);

            if(_setter != null)
                ok &= _setter.Resolve(rc);
            if (_getter != null)
                ok &= _getter.Resolve(rc);

            return ok;
        }

       void ResolveChildContext(ResolveContext rc, MethodSpec ms, bool importlocals = false,bool setter = false)
       {
           
           ResolveContext childctx = rc.CreateAsChild(rc.Imports, rc.CurrentNamespace, ms);
           if (importlocals)
               childctx.Resolver.KnownLocalVars.AddRange(rc.Resolver.KnownLocalVars);

           if (setter)
           {
               _setter = (SetterDefinition)_setter.DoResolve(childctx);
               SetterResolve = childctx;
           }
           else
           {
               _getter = (GetterDefinition)_getter.DoResolve(childctx);
               GetterResolve = childctx;
           }

           //rc.UpdateFather(childctx);
                      
       }
     public override SimpleToken DoResolve(ResolveContext rc)
        {
            ccvh = new CallingConventionsHandler();
            _id = (MethodIdentifier)_id.DoResolve(rc);
            ccv = _id.CV;
            mods = _id.Mods;
            specs = _id.Specs;
          
           
           
            base._type = _id.TType;


            if ( specs !=  Specifiers.NoSpec)
                ResolveContext.Report.Error(0, Location, "Specifiers are not allowed for properties");
        

        
            //if (rc.IsInClass)
            //    property = new PropertySpec(rc.CurrentNamespace, rc.CurrentType.NormalizedName + "$_" + _id.Name, mods, _id.TType.Type, Location);
            //else
                property = new PropertySpec(rc.CurrentNamespace, _id.Name, mods, _id.TType.Type, Location);

                MethodSpec old = rc.CurrentMethod;
     if(_setter != null){
         _setter.CreateSetter(rc,property);
         SetterParameters.Add(new ParameterSpec(rc.CurrentNamespace, "value", _setter.Setter, _id.TType.Type, Location, 4, mods));

         int last_param = 4;
         // Calling Convention
         ccvh.SetParametersIndex(ref SetterParameters, ccv, ref last_param);
         if (ccv == CallingConventions.FastCall)
             ccvh.ReserveFastCall(rc, SetterParameters);
         else if (ccv == CallingConventions.VatuSysCall)
             ResolveContext.Report.Error(0, Location, "Vatu system call is not allowed for properties");
         if (property.Setter.memberType is ArrayTypeSpec)
             ResolveContext.Report.Error(45, Location, "return type must be non array type " + property.Setter.MemberType.ToString() + " is user-defined type.");
          


         property.Setter.Parameters = SetterParameters;
         property.Setter.LastParameterEndIdx = (ushort)last_param;
//         rc.CurrentMethod = property.Setter;

         ResolveChildContext(rc, property.Setter, false, true);

              }

     if (_getter != null)
     {
         _getter.CreateGetter(rc, property);
         int last_param = 4;
         // Calling Convention
         ccvh.SetParametersIndex(ref GetterParameters, ccv, ref last_param);
         if (ccv == CallingConventions.FastCall)
             ccvh.ReserveFastCall(rc, GetterParameters);
         else if (ccv == CallingConventions.VatuSysCall)
             ResolveContext.Report.Error(0, Location, "Vatu system call is not allowed for properties");
         if (property.Getter.memberType is ArrayTypeSpec)
             ResolveContext.Report.Error(45, Location, "return type must be non array type " + property.Getter.MemberType.ToString() + " is user-defined type.");
          

         property.Getter.Parameters = GetterParameters;
         property.Getter.LastParameterEndIdx = (ushort)last_param;
         //rc.CurrentMethod = property.Getter;
         ResolveChildContext(rc, property.Getter);
         
     }

     rc.CurrentMethod = old;
     if (_getter != null && _setter != null && ((_setter.AutoGenerated && !_getter.AutoGenerated) || (_getter.AutoGenerated && !_setter.AutoGenerated)))
         ResolveContext.Report.Error(0, Location, "Both getter & setter must have the same kind (auto-generated, not)");

     if ((_setter != null && _getter != null && _getter.AutoGenerated && _setter.AutoGenerated) || (_setter == null && _getter.AutoGenerated) || (_getter == null && _setter.AutoGenerated))
     {
         FieldSpec fs = new FieldSpec(rc.CurrentNamespace, "Autogenerated_$field$_"+property.Signature.Signature, Modifiers.Private, _id.TType.Type, Location);
         property.AutoBoundField = fs;
     }

     rc.KnowField(property);
   
           //if(rc.IsInClass)
           // {
           //     ParameterSpec thisps = new ParameterSpec(rc.CurrentNamespace, "this", method, rc.CurrentType, Location, 4);
           //     Parameters.Insert(0, thisps);

           // }
    

            return this;
        }
   
        int GetNextIndex(ParameterSpec p)
 {
    int paramidx = (p.MemberType.Size == 1) ? 2 : p.MemberType.Size;

     if (p.MemberType.Size != 1 && p.MemberType.Size % 2 != 0)
         paramidx++;

     return paramidx + p.StackIdx;
 }
        public bool PopAllToRegister(EmitContext ec, RegistersEnum rg, int size, int offset = 0)
        {

            int s = size / 2;


            for (int i = 0; i < s; i++)
                ec.EmitInstruction(new Pop() { DestinationReg = rg, DestinationDisplacement = offset + 2 * i, DestinationIsIndirect = true, Size = 16 });
            if (size % 2 != 0)
            {
                ec.EmitPop(RegistersEnum.DX);
                ec.EmitInstruction(new Mov() { DestinationReg = rg, DestinationDisplacement = offset - 1 + size, DestinationIsIndirect = true, Size = 8, SourceReg = RegistersEnum.DL });

            }
            return true;
        }
         void EmitGetter(EmitContext ec)
         {
             Label mlb = ec.DefineLabel(property.Getter.Signature.ToString());
             mlb.Method = true;
             ec.MarkLabel(mlb);
             ec.EmitComment("Property: Name = " + property.Getter.Name);
             // create stack frame
             ec.EmitComment("create stackframe");
             ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
             ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });

             ushort size = 0;
             List<VarSpec> locals = ec.CurrentResolve.GetLocals();
             foreach (VarSpec v in locals)
                 size += (ushort)(v.memberType.IsArray ? v.memberType.GetSize(v.memberType) : v.MemberType.Size);
             if (size != 0)         // no allocation
                 ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });

             if (locals.Count > 0)
             {
                 ec.EmitComment("Local Vars Definitions");
                 foreach (VarSpec v in locals)
                     ec.EmitComment("Local " + v.Name + " @BP" + v.VariableStackIndex);
             }

          


             if (_getter.AutoGenerated)
             {
                 property.AutoBoundField.EmitToStack(ec);
                 if (!(property.Getter.memberType.IsFloat && !property.Getter.memberType.IsPointer))
                 {
                     int ret_size = property.Getter.memberType.GetSize(property.Getter.memberType);
                     if (ret_size <= 2)
                         ec.EmitPop(EmitContext.A);
                     else
                         PopAllToRegister(ec, EmitContext.BP, ret_size, property.Getter.LastParameterEndIdx);
                 }
             }
             else
                 _getter.Block.Block.Emit(ec);

             ec.EmitComment("return label");
             // Return Label
             ec.MarkLabel(ec.DefineLabel(property.Getter.Signature + "_ret"));
             ec.EmitComment("destroy stackframe");
             ec.EmitInstruction(new Leave());
             // ret
             ccvh.EmitDecl(ec, ref GetterParameters, ccv);

         }
         void EmitSetter(EmitContext ec)
         {
             Label mlb = ec.DefineLabel(property.Setter.Signature.ToString());
             mlb.Method = true;
             ec.MarkLabel(mlb);
             ec.EmitComment("Property: Name = " + property.Setter.Name +", AutoGenerated = "+_setter.AutoGenerated);
             // create stack frame
             ec.EmitComment("create stackframe");
             ec.EmitInstruction(new Push() { DestinationReg = EmitContext.BP, Size = 80 });
             ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.BP, SourceReg = EmitContext.SP, Size = 80 });

             ushort size = 0;
             List<VarSpec> locals = ec.CurrentResolve.GetLocals();
             foreach (VarSpec v in locals)
                 size += (ushort)(v.memberType.IsArray ? v.memberType.GetSize(v.memberType) : v.MemberType.Size);
             if (size != 0)         // no allocation
                 ec.EmitInstruction(new Sub() { DestinationReg = EmitContext.SP, SourceValue = size, Size = 80 });

             if (locals.Count > 0)
             {
                 ec.EmitComment("Local Vars Definitions");
                 foreach (VarSpec v in locals)
                     ec.EmitComment("Local " + v.Name + " @BP" + v.VariableStackIndex);
             }


             ec.EmitComment("Block");

             if (_setter.Block.IsSemi)
             {
                 SetterParameters[0].EmitToStack(ec);
                 property.AutoBoundField.EmitFromStack(ec);
             }
             else
                 _setter.Block.Block.Emit(ec);

             ec.EmitComment("return label");
             // Return Label
             ec.MarkLabel(ec.DefineLabel(property.Setter.Signature + "_ret"));
             ec.EmitComment("destroy stackframe");
             ec.EmitInstruction(new Leave());
             // ret
             ccvh.EmitDecl(ec, ref SetterParameters, ccv);

         }
        public override bool Emit(EmitContext ec)
        {
            if ((mods & Modifiers.Extern) == Modifiers.Extern)
            {
             if(property.Getter != null)
                 ec.DefineGlobal(property.Getter);

             if (property.Setter != null)
                 ec.DefineGlobal(property.Setter);
            }
            if (_getter != null)
            {
                EmitContext sec = new EmitContext(new AsmContext(ec.ag.AssemblerWriter));
                sec.CurrentResolve = GetterResolve;
                EmitGetter(ec);
                ec.ag.Instructions.AddRange(sec.ag.Instructions);
            }

            if (_setter != null)
            {
                EmitContext sec = new EmitContext(new AsmContext(ec.ag.AssemblerWriter));
                sec.CurrentResolve = SetterResolve;

                EmitSetter(sec);
                ec.ag.Instructions.AddRange(sec.ag.Instructions);
            }
            if (property.AutoBoundField != null)
                ec.EmitDataWithConv(property.AutoBoundField.Signature.ToString(), new byte[property.AutoBoundField.MemberType.Size], property.AutoBoundField);

            return true;
        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
          
      




            fc.LookForUnreachableBrace = !fc.NoReturnCheck;
           fc.NoReturnCheck =  _type.Type.Equals(BuiltinTypeSpec.Void);
           FlowState fs = FlowState.Valid;
            if ( _setter != null)
                fs = _setter.DoFlowAnalysis(fc);
       


            if (!fs.Reachable.IsUnreachable && !fc.NoReturnCheck)
                fc.ReportNotAllCodePathsReturns(Location);

            if (_getter != null)
                fs = _getter.DoFlowAnalysis(fc);

            if (!fs.Reachable.IsUnreachable && !fc.NoReturnCheck)
                fc.ReportNotAllCodePathsReturns(Location);
            fc.LookForUnreachableBrace = false;
            return fs;
        }

       
    }
}
