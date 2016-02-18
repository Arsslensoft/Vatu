﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core.Expressions
{
  public  class DelegateExpressions : MethodExpression
    {
      TypeToken _type;
      Block _fbd;
      [Rule("<Value> ::= ~delegate <Type> <Block> ")]
      public DelegateExpressions(TypeToken tok, Block fbd) 
      {
          _type = tok;
          _fbd = fbd;

      }
      MethodSpec _method;
      List<ParameterSpec> parameters;
      public override SimpleToken DoResolve(ResolveContext rc)
      {
          ccvh = new CallingConventionsHandler();
          _type = (TypeToken)_type.DoResolve(rc);
          Type = _type.Type;
          MethodSpec old = rc.CurrentMethod;
      
              rc.CreateNewState();
           
          Label name= rc.DefineLabel( LabelType. ANONYMOUS_METHOD_EXPR);
          name = new Label("Autogenerated$_" + name.Name);
          rc.CurrentMethod = new MethodSpec(rc.CurrentNamespace, name.Name, Modifiers.NoModifier, _type.Type, CallingConventions.Default, new List<TypeSpec>().ToArray(), Location);
          ResolveChildContext(rc, rc.CurrentMethod, true);

           Method = rc.CurrentMethod;

           if (Method.memberType is ArrayTypeSpec)
               ResolveContext.Report.Error(45, Location, "return type must be non array type " + Method.MemberType.ToString() + " is user-defined type.");
          

           Parameters = resolve.AnonymousParameters;
              parameters = Method.Parameters;
              int last_param = 4;
              // Calling Convention
              ccvh.SetParametersIndex(ref parameters,  CallingConventions.Default, ref last_param);

              Method.LastParameterEndIdx = (ushort)last_param;
              Method.Parameters = parameters;
    
          List<TypeSpec> tp = new List<TypeSpec>();
          foreach(Expr e in Parameters)
              tp.Add(e.Type);
         // Method.Signature = new MemberSignature(rc.CurrentNamespace, name.Name, tp.ToArray(), Location);

          rc.RestoreOldState();
          rc.CurrentMethod = old;
          return this;
      }
      public override bool Resolve(ResolveContext rc)
      {
          return true;
      }

      ResolveContext resolve;
      void ResolveChildContext(ResolveContext rc, MethodSpec ms, bool importlocals = false)
      {

          ResolveContext childctx = rc.CreateAsChild(rc.Imports, rc.CurrentNamespace, ms);
          if (importlocals)
              childctx.Resolver.KnownLocalVars.AddRange(rc.Resolver.KnownLocalVars);

          childctx.AnonymousParameterIdx = 4;
          childctx.AnonymousParameters = new List<Expr>();
          childctx.CurrentGlobalScope |= ResolveScopes.AnonymousMethod;
          _fbd = (Block)_fbd.DoResolve(childctx);

          resolve = childctx;
       
          foreach(VarSpec v in rc.Resolver.KnownLocalVars)
            resolve.Resolver.KnownLocalVars.Remove(v);


          //rc.UpdateFather(childctx);

      }
      bool declared = false;
      void EmitDeclareAnonymous(EmitContext ec)
      {
      

          Label mlb = ec.DefineLabel(Method.Signature.ToString());
          mlb.Method = true;
          ec.MarkLabel(mlb);
          ec.EmitComment("Anonymous Method: Name = " + Method.Name);
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


              _fbd.Emit(ec);

          ec.EmitComment("return label");
          // Return Label
          ec.MarkLabel(ec.DefineLabel(Method.Signature + "_ret"));
          ec.EmitComment("destroy stackframe");
          ec.EmitInstruction(new Leave());
          // ret
          ccvh.EmitDecl(ec, ref parameters, CallingConventions.Default);
      
      }
      public override bool Emit(EmitContext ec)
      {
          if (!declared)
          {
              EmitContext sec = new EmitContext(new AsmContext(ec.ag.AssemblerWriter));
              sec.CurrentResolve = resolve;

              EmitDeclareAnonymous(sec);
              ec.ag.AnonymousInstructions.AddRange(sec.ag.AnonymousInstructions);
              ec.ag.DataMembers.AddRange(sec.ag.DataMembers);
              ec.ag.ConstantDataMembers.AddRange(sec.ag.ConstantDataMembers);
              ec.ag.DefaultInstructions.AddRange(sec.ag.DefaultInstructions);
              ec.ag.Instructions.AddRange(sec.ag.Instructions);
              ec.ag.Externals.AddRange(sec.ag.Externals);
              ec.ag.Globals.AddRange(sec.ag.Globals);
              ec.ag.InitInstructions.AddRange(sec.ag.InitInstructions);
              declared = true;
          }
          return base.Emit(ec);
      }
      public override bool EmitToStack(EmitContext ec)
      {
          if (!declared)
          {
              EmitContext sec = new EmitContext(new AsmContext(ec.ag.AssemblerWriter));
              sec.CurrentResolve = resolve;

              EmitDeclareAnonymous(sec);
              ec.ag.AnonymousInstructions.AddRange(sec.ag.AnonymousInstructions);
              ec.ag.DataMembers.AddRange(sec.ag.DataMembers);
              ec.ag.ConstantDataMembers.AddRange(sec.ag.ConstantDataMembers);
              ec.ag.DefaultInstructions.AddRange(sec.ag.DefaultInstructions);
              ec.ag.Instructions.AddRange(sec.ag.Instructions);
              ec.ag.Externals.AddRange(sec.ag.Externals);
              ec.ag.Globals.AddRange(sec.ag.Globals);
              ec.ag.InitInstructions.AddRange(sec.ag.InitInstructions);
              declared = true;
          }

          return base.EmitToStack(ec);
      }
      public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
      {
          if (!declared)
          {
              EmitContext sec = new EmitContext(new AsmContext(ec.ag.AssemblerWriter));
              sec.CurrentResolve = resolve;

              EmitDeclareAnonymous(sec);
              ec.ag.AnonymousInstructions.AddRange(sec.ag.Instructions);
              declared = true;
          }

          return base.EmitBranchable(ec, truecase, v);
      }
      public override string CommentString()
      {
          return "Delegate Method : "+Method.Name;
      }
    }
}
