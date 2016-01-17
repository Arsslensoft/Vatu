using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    public class ByIndexOperator : AccessOp
    {
      
        public ByIndexOperator()
        {
            IsByte = true;
            Index = -1;
            Register = RegistersEnum.DX;
            _op = AccessOperator.ByIndex;
        }
        public bool IsByte { get; set; }
        public int Index { get; set; }
        void GetIndex(ConstantExpression expr)
        {
           if(!((expr is  ArrayConstant) ||( expr is StringConstant)))
            Index = int.Parse(expr.GetValue().ToString());
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {

            OvlrdOp = rc.Resolver.TryResolveMethod(Left.Type.NormalizedName + "_IndexedAccess", new TypeSpec[2] { Left.Type, BuiltinTypeSpec.UInt });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;

            if (!Left.Type.IsPointer && !Left.Type.IsArray)
                ResolveContext.Report.Error(51, Location, "Indexed access is only allowed on pointers and arrays");

            else if(OvlrdOp != null)
                return new AccessExpression(Left as VariableExpression, Right, this);
            else if (Left.Type.IsArray)
            {
                IsByte = Left.Type.BaseType.Size != 2;
           
                if (Left is VariableExpression && Right is ConstantExpression)
                {
                    VariableExpression ve = (VariableExpression)Left;
                    ConstantExpression ce = (ConstantExpression)Right;
                    GetIndex(ce);
                    if (Index < 0)
                        ResolveContext.Report.Error(50, ce.Location, "Invalid array index");
                   if (ve.variable is VarSpec)
                    {
                        VarSpec v = (VarSpec)ve.variable;
                        VarSpec vr = new VarSpec(v.NS, v.Name, v.MethodHost, v.MemberType.BaseType, Location, v.Modifiers);
                        vr.StackIdx = v.StackIdx;
                     
                        vr.StackIdx += Left.Type.BaseType.Size*Index;
                       

                       return new AccessExpression(vr,(Left is AccessExpression)?(Left as AccessExpression):null);
                    }
                    else if (ve.variable is FieldSpec)
                    {
                        FieldSpec v = (FieldSpec)ve.variable;
                        FieldSpec vr = new FieldSpec(v.NS, v.Name, v.Modifiers, v.MemberType.BaseType, Location);
                        vr.FieldOffset = v.FieldOffset;
                        vr.IsIndexed = true;
                       vr.FieldOffset  += Left.Type.BaseType.Size*Index;
                     
                        return new AccessExpression(vr,(Left is AccessExpression)?(Left as AccessExpression):null);
                    }
                    
                }
                else return new AccessExpression(Left as VariableExpression, Right, this);

            }
            else
            {
              
                IsByte = Left.Type.BaseType.Size != 2;
                if (Left is VariableExpression && Right is ConstantExpression)
                {
                    VariableExpression ve = (VariableExpression)Left;
                    ConstantExpression ce = (ConstantExpression)Right;
                    GetIndex(ce);
                    if (Index < 0)
                        ResolveContext.Report.Error(50, ce.Location, "Invalid array index");
                }
            }
            return new AccessExpression(Left as VariableExpression, Right , this);
        }
        public override bool Resolve(ResolveContext rc)
        {
            return Left.Resolve(rc) && Right.Resolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            if (OvlrdOp != null)
                return base.EmitOverrideOperatorValue(ec);
            
            if (Index == -1)
            {
                if (IsByte)
                {
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitPop(RegistersEnum.DI, 16);
                    ec.EmitInstruction(new Add() { SourceReg = RegistersEnum.DI, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 16, true);
                }
                else
                {
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(EmitContext.A, 16);
                    ec.EmitPop(RegistersEnum.SI, 16);
         
                    ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.C, Size = 16, SourceValue = (ushort)Left.Type.BaseType.Size });
                    ec.EmitInstruction(new Multiply() { DestinationReg = EmitContext.C, Size = 80 });
                    ec.EmitInstruction(new Add() { SourceReg = EmitContext.A, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 16, true);
                }
            }
            else
            {
               
                    Left.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitInstruction(new Add() { SourceValue =(ushort)( (ushort)Index * (uint)Left.Type.BaseType.Size), DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 16, true);
              
            }
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                base.EmitOverrideOperatorAddress(ec);
                ec.EmitPop(RegistersEnum.DI);
                ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 16, DestinationIsIndirect = true, SourceReg = RegistersEnum.DI });
                return true;
            }
            if (Index == -1)
            {
                if (IsByte)
                {
          
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitPop(RegistersEnum.DI, 16);
                    ec.EmitInstruction(new Add() { SourceReg = RegistersEnum.DI, DestinationReg = RegistersEnum.SI });
                   
                    ec.EmitPop(Register.Value);
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 8, DestinationIsIndirect = true, SourceReg = ec.GetLow(Register.Value) });
                }
                else
                {
                

                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(EmitContext.A, 16);
                    ec.EmitPop(RegistersEnum.SI, 16);
                
                    ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.C, Size = 16, SourceValue = (ushort)Left.Type.BaseType.Size });
                    ec.EmitInstruction(new Multiply() { DestinationReg = EmitContext.C ,Size = 80});
                    ec.EmitInstruction(new Add() { SourceReg = EmitContext.A, DestinationReg = RegistersEnum.SI });
                  
                    ec.EmitPop(RegistersEnum.DI);
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 16, DestinationIsIndirect = true, SourceReg = RegistersEnum.DI });
                }
            }
            else
            {
              
                  
                    Left.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitInstruction(new Add() { SourceValue =(ushort)( (ushort)Index * (ushort)Left.Type.BaseType.Size), DestinationReg = RegistersEnum.SI });
                    if (IsByte)
                    {
                        ec.EmitPop(Register.Value);
                        ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 8, DestinationIsIndirect = true, SourceReg = ec.GetLow(Register.Value) });
                    }
                    else
                    {
                        ec.EmitPop(RegistersEnum.DI);
                        ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 16, DestinationIsIndirect = true, SourceReg = RegistersEnum.DI });
                    }
            }
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
    }
    [Terminal("::")]
    public class ByNameOperator : AccessOp
    {
        public ByNameOperator()
        {
            Register = RegistersEnum.AX;
            _op = AccessOperator.ByName;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return base.DoResolve(rc);
        }
        public override bool Emit(EmitContext ec)
        {
            return Right.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            return Right.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Right.EmitToStack(ec);
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            return Right.EmitToRegister(ec, rg);
        }
    }
    [Terminal(".")]
    public class ByValueOperator : AccessOp
    {
        public override int Offset
        {
            get { return index; }
        }
        public override MemberSpec Member
        {
            get { return (enumval != null) ? enumval : struct_var; }
        }
        public ByValueOperator()
        {
            _op = AccessOperator.ByValue;
            Register = RegistersEnum.AX;
        }
      

        // enum
        TypeSpec tp;
        EnumMemberSpec enumval;
        // Struct
        MemberSpec struct_var;
        int index = -1;
        // *p
        public bool ResolveEnumValue(ResolveContext rc, VariableExpression lv, VariableExpression rv)
        {
            rc.Resolver.TryResolveType(lv.Name,ref tp);
            if (tp != null && tp.IsEnum)
            {
     
                enumval = rc.Resolver.TryResolveEnumValue(rv.Name);
                return true;
            }
            return false;

        }
        public bool ResolveExtension(ResolveContext rc)
        {
            rc.CurrentExtensionLookup = Left.Type;
            rc.StaticExtensionLookup = false;
            rc.ExtensionVar = Left;

            Right = (Expr)Right.DoResolve(rc);
            if (Right is VariableExpression && (Right as VariableExpression).variable == null)
            {

                ResolveContext.Report.Error(0, Location, "Unresolved extended field");
                rc.ExtensionVar = null;
                rc.CurrentExtensionLookup = null;
                return false;
            }
            else if (Right is MethodExpression && (Right as MethodExpression).Method == null)
            {
            
                rc.ExtensionVar = null;
                rc.CurrentExtensionLookup = null;
                return false;
            }

            rc.ExtensionVar = null;
            rc.CurrentExtensionLookup = null;

      
            return true;
        }
        TypeMemberSpec tmp = null;
        public bool ResolveStructOrUnionMember(ResolveContext rc, MemberSpec mem, ref TypeMemberSpec tmp, VariableExpression lv, VariableExpression rv)
        {

            if (!mem.MemberType.IsPointer)
            {
                if (!mem.MemberType.IsForeignType)
                    return false;
                else if (mem.MemberType.IsStruct)
                {
                    StructTypeSpec stp = (StructTypeSpec)mem.MemberType;
                    tmp = stp.ResolveMember(rv.Name);
                    if (tmp == null)
                        return false;
                    else
                    {
                        // Resolve
                        index = tmp.Index;
                        return true;
                    }

                }
                else
                {
                    UnionTypeSpec stp = (UnionTypeSpec)mem.MemberType;
                    tmp = stp.ResolveMember(rv.Name);
                    if (tmp == null)
                        return false;
                    else
                    {
                        // Resolve

                        index = 0;
                        return true;
                    }

                }
            }
            else return false;


            return false;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            // Check if left is type
        
            if (Left is VariableExpression && Right is MethodExpression)
            {
                if (!ResolveExtension(rc))
                    ResolveContext.Report.Error(0, Location, "Unresolved extended method");
                else return Right;

            }
            else  if (Left is VariableExpression && Right is VariableExpression)
            {


                VariableExpression lv = (VariableExpression)Left;
                VariableExpression rv = (VariableExpression)Right;
                // enum
                bool ok = ResolveEnumValue(rc, lv, rv);
                if (ok)
                    return new AccessExpression(enumval,null);

                // struct member
                if (!ok)
                {
                    struct_var = lv.variable;

                    if (struct_var != null)
                    {
                        ok = ResolveStructOrUnionMember(rc, struct_var, ref tmp, lv, rv);
                        rv.variable = tmp;
                        if (!ok)
                        {
                
                       // field extension
                        if (!ResolveExtension(rc))
                            ResolveContext.Report.Error(0, Location, "Unresolved extended field or member");
                        else return Right;
                   

                    
                        }
                        if (struct_var is VarSpec)
                        {
                            VarSpec v = (VarSpec)struct_var;

                            VarSpec dst = new VarSpec(v.NS, v.Name, v.MethodHost, tmp.MemberType, Location, v.Modifiers);
                            dst.StackIdx = v.StackIdx + index;
                            return new AccessExpression(dst, Left as AccessExpression);

                        }
                        else if (struct_var is RegisterSpec)
                        {
                            RegisterSpec v = (RegisterSpec)struct_var;

                            RegisterSpec dst = new RegisterSpec(tmp.MemberType, v.Register, Location,0);
                            dst.RegisterIndex = v.RegisterIndex + index;
                            return new AccessExpression(dst,Left as AccessExpression);

                        }
                        else if (struct_var is FieldSpec)
                        {
                            FieldSpec v = (FieldSpec)struct_var;

                            FieldSpec dst = new FieldSpec(v.NS, v.Name, v.Modifiers, tmp.MemberType, Location);
                            dst.FieldOffset = v.FieldOffset + index;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null);
                        }
                        else if (struct_var is ParameterSpec)
                        {
                            ParameterSpec v = (ParameterSpec)struct_var;

                            ParameterSpec dst = new ParameterSpec(v.Name, v.MethodHost, tmp.MemberType, Location,v.InitialStackIndex, v.Modifiers);

                            dst.StackIdx = v.StackIdx + index;

                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null);
                        }
                        this.CommonType = tmp.MemberType;
                    }
                   
                }

                // error
                if (tp == null && struct_var == null)
                    ResolveContext.Report.Error(18, Location, "Undefined access reference " + lv.Name);
                if (!ok)
                    ResolveContext.Report.Error(19, Location, "Unresolved member access " + lv.Name + "." + rv.Name);

            }

            else ResolveContext.Report.Error(20, Location, "'.' operator accepts only variable expressions at both sides (left,right)");

            throw new Exception("Failed to create variable");
        }




    }

  

    [Terminal("->")]
    public class ByAddressOperator : AccessOp
    {
        public ByAddressOperator()
        {   Register = RegistersEnum.AX;
            _op = AccessOperator.ByAddress;
        }
         public override int Offset
        {
            get { return index; }
        }
        public override MemberSpec Member
        {
            get { return struct_var; }
        }
      
      

     
        // Struct
        MemberSpec struct_var;
        int index = -1;
   

        TypeMemberSpec tmp = null;
        public bool ResolveStructOrUnionMember(ResolveContext rc, MemberSpec mem, ref TypeMemberSpec tmp, VariableExpression lv, VariableExpression rv)
        {

            if (mem.MemberType.IsPointer )
            {
                if (!mem.MemberType.IsForeignType)
                    ResolveContext.Report.Error(15, Location, "'->' operator allowed only with struct union based types");
                else if(mem.MemberType.IsStruct)
                {
                    StructTypeSpec stp = null;
                    if(Left is AccessExpression && (Left as AccessExpression).IsByIndex)
                       stp= (StructTypeSpec)mem.MemberType.BaseType.BaseType;
                    else stp = (StructTypeSpec)mem.MemberType.BaseType;

                    tmp = stp.ResolveMember(rv.Name);
                    if (tmp == null)
                        ResolveContext.Report.Error(16, Location, rv.Name + " is not defined in struct " + stp.Name);
                    else
                    {
                        // Resolve
                        index = tmp.Index;
                        return true;
                    }

                }
                else
                {
                    UnionTypeSpec stp =null;
                    if (Left is AccessExpression && (Left as AccessExpression).IsByIndex)
                        stp = (UnionTypeSpec)mem.MemberType.BaseType.BaseType;
                    else stp = (UnionTypeSpec)mem.MemberType.BaseType;
                    tmp = stp.ResolveMember(rv.Name);
                    if (tmp == null)
                        ResolveContext.Report.Error(16, Location, rv.Name + " is not defined in union " + stp.Name);
                    else
                    {
                        // Resolve

                        index = 0;
                        return true;
                    }

                }
            }
            else ResolveContext.Report.Error(17, Location, "Cannot use '->' operator with non pointer types use '.' instead");


            return false;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            // Check if left is type
            if (Left is VariableExpression && Right is VariableExpression)
            {
          


                VariableExpression lv = (VariableExpression)Left;
                VariableExpression rv = (VariableExpression)Right;
                // enum
                bool ok = false;
          

                // struct member
                if (!ok)
                {
                    struct_var = lv.variable;
                    if (struct_var != null)
                    {
                        ok = ResolveStructOrUnionMember(rc, struct_var, ref tmp, lv, rv);
                        rv.variable = tmp;
                        CommonType = tmp.MemberType;
                        if (struct_var is VarSpec)
                        {
                            VarSpec dst = (VarSpec)struct_var;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, true, index, tmp.MemberType);

                        }
                        else if (struct_var is RegisterSpec)
                        {
                            RegisterSpec dst = (RegisterSpec)struct_var;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, true, index, tmp.MemberType);

                        }
                        else if (struct_var is FieldSpec)
                        {
                            FieldSpec dst = (FieldSpec)struct_var;


                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, true, index, tmp.MemberType);
                        }
                        else if (struct_var is ParameterSpec)
                        {
                            ParameterSpec dst = (ParameterSpec)struct_var;


                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, true, index, tmp.MemberType);
                        }
                        this.CommonType = tmp.MemberType;
                    }
                }

                // error
                if ( struct_var == null)
                    ResolveContext.Report.Error(18, Location, "Undefined access reference " + lv.Name);
                if (!ok)
                    ResolveContext.Report.Error(19, Location, "Unresolved member access " + lv.Name + "." + rv.Name);

            }

            else ResolveContext.Report.Error(20, Location, "'->' operator accepts only variable expressions at both sides (left,right)");

            throw new Exception("Failed to create variable");
        }

    }


}
