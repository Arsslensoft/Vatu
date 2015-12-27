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
            Register = RegistersEnum.AX;
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
            if (!Left.Type.IsPointer && !Left.Type.IsArray)
                ResolveContext.Report.Error(51, Location, "Indexed access is only allowed on pointers and arrays");
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
                        if (IsByte)
                            vr.StackIdx += Index;
                        else
                            vr.StackIdx += 2 * Index;

                        return new AccessExpression(vr);
                    }
                    else if (ve.variable is FieldSpec)
                    {
                        FieldSpec v = (FieldSpec)ve.variable;
                        FieldSpec vr = new FieldSpec(v.NS, v.Name, v.Modifiers, v.MemberType.BaseType, Location);
                        vr.FieldOffset = v.FieldOffset;
                        if (IsByte)
                            vr.FieldOffset += Index;
                        else
                            vr.FieldOffset += 2 * Index;

                        return new AccessExpression(vr);
                    }

                }

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
            if (Index == -1)
            {
                if (IsByte)
                {
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitPop(RegistersEnum.DI, 16);
                    ec.EmitInstruction(new Add() { SourceReg = RegistersEnum.DI, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 8, true);
                }
                else
                {
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitPop(RegistersEnum.DI, 16);
                    ec.EmitInstruction(new Add() { SourceReg = RegistersEnum.DI, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 16, true);
                }
            }
            else
            {
                if (IsByte)
                {
                    Left.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitInstruction(new Add() { SourceValue = (uint)Index, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 8, true);
                }
                else
                {
                    Left.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitInstruction(new Add() { SourceValue =(uint)Index * 2, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 16, true);
                }
            }
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (Index == -1)
            {
                if (IsByte)
                {
                    ec.EmitPop(Register.Value);
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitPop(RegistersEnum.DI, 16);
                    ec.EmitInstruction(new Add() { SourceReg = RegistersEnum.DI, DestinationReg = RegistersEnum.SI });
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 8, DestinationIsIndirect = true, SourceReg = ec.GetLow(Register.Value) });
                }
                else
                {
                    ec.EmitPop(Register.Value);
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitPop(RegistersEnum.DI, 16);
                    ec.EmitInstruction(new Add() { SourceReg = RegistersEnum.DI, DestinationReg = RegistersEnum.SI });
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 16, DestinationIsIndirect = true, SourceReg = Register.Value });
                }
            }
            else
            {
                if (IsByte)
                {
                    ec.EmitPop(Register.Value);
                    Left.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitInstruction(new Add() { SourceValue = (uint)Index, DestinationReg = RegistersEnum.SI });
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 8, DestinationIsIndirect = true, SourceReg = ec.GetLow( Register.Value) });
                }
                else
                {
                    ec.EmitPop(Register.Value);
                    Left.EmitToStack(ec);
                    ec.EmitPop(RegistersEnum.SI, 16);
                    ec.EmitInstruction(new Add() { SourceValue = (uint)Index * 2, DestinationReg = RegistersEnum.SI });
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 16, DestinationIsIndirect = true, SourceReg = Register.Value });
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
            tp = rc.Resolver.TryResolveType(lv.Name);
            if (tp != null && tp.IsEnum)
            {
     
                enumval = rc.Resolver.TryResolveEnumValue(rv.Name);
                return true;
            }
            return false;

        }

        TypeMemberSpec tmp = null;
        public bool ResolveStructMember(ResolveContext rc, MemberSpec mem, ref TypeMemberSpec tmp, VariableExpression lv, VariableExpression rv)
        {

            if (!mem.MemberType.IsPointer)
            {
                if (!mem.MemberType.IsStruct)
                    ResolveContext.Report.Error(15, Location, "'.' operator allowed only with struct based types");
                else
                {
                    StructTypeSpec stp = (StructTypeSpec)mem.MemberType;
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
            }
            else ResolveContext.Report.Error(17, Location, "Cannot use '.' operator with pointer types use '->' instead");


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
                bool ok = ResolveEnumValue(rc, lv, rv);
                if (ok)
                    return new AccessExpression(enumval);

                // struct member
                if (!ok)
                {
                    struct_var = lv.variable;
                    if (struct_var != null)
                    {
                        ok = ResolveStructMember(rc, struct_var, ref tmp, lv, rv);
                        rv.variable = tmp;

                        if (struct_var is VarSpec)
                        {
                            VarSpec v = (VarSpec)struct_var;

                            VarSpec dst = new VarSpec(v.NS, v.Name, v.MethodHost, tmp.MemberType, Location, v.Modifiers);
                            dst.StackIdx = v.StackIdx + tmp.Index;
                            return new AccessExpression(dst);

                        }
                        else if (struct_var is FieldSpec)
                        {
                            FieldSpec v = (FieldSpec)struct_var;

                            FieldSpec dst = new FieldSpec(v.NS, v.Name, v.Modifiers, tmp.MemberType, Location);
                            dst.FieldOffset = v.FieldOffset + tmp.Index;
                            return new AccessExpression(dst);
                        }
                        else if (struct_var is ParameterSpec)
                        {
                            ParameterSpec v = (ParameterSpec)struct_var;

                            ParameterSpec dst = new ParameterSpec(v.Name, v.MethodHost, tmp.MemberType, v.IsConstant, Location, v.Modifiers);

                            dst.StackIdx = v.StackIdx + tmp.Index;

                            return new AccessExpression(dst);
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
        {
            _op = AccessOperator.ByAddress;
        }

    }


}
