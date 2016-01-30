using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    public class ByIndexOperator : AccessOp, IEmitAddress
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
            if (!((expr is ArrayConstant) || (expr is StringConstant)))
                Index = int.Parse(expr.GetValue().ToString());
        }
       public override bool Resolve(ResolveContext rc)
        {
            return Left.Resolve(rc) && Right.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
     
            rc.Resolver.TryResolveMethod(Left.Type.NormalizedName + "_IndexedAccess", ref OvlrdOp, new TypeSpec[2] { Left.Type, BuiltinTypeSpec.UInt });
            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;

            if (!Left.Type.IsPointer && !Left.Type.IsArray && !TypeChecker.BitAccessible(Left.Type) && !TypeChecker.ByteAccessible(Left.Type))
                ResolveContext.Report.Error(51, Location, "Indexed access is only allowed on pointers, arrays and builtin bit/byte accessible types");
            else if (TypeChecker.BitAccessible(Left.Type))
            {
                if (Left is VariableExpression && Right.Type.IsNumeric)
                {
                    VariableExpression ve = (VariableExpression)Left;
                    return new BitAccessExpression(ve.variable, Right);
                }
                else
                    ResolveContext.Report.Error(51, Location, "Bit Indexed access is only allowed on variables");

            }
            else if (TypeChecker.ByteAccessible(Left.Type))
            {
                if (Left is VariableExpression && Right.Type.IsNumeric)
                {
                    VariableExpression ve = (VariableExpression)Left;
                    return new ByteAccessExpression(ve.variable, Right, ve, rc);
                }
                else
                    ResolveContext.Report.Error(51, Location, "Byte Indexed access is only allowed on variables");

            }
            else if (OvlrdOp != null)
                return new AccessExpression(Left as VariableExpression, Right, this);


            else if (Left.Type.IsArray)
            {
                //if(Left.Type.IsMultiDimensionArray)
                //    ((Left as AccessExpression).Operator as ByIndexOperator) 
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
                        VarSpec vr = new VarSpec(v.NS, v.Name, v.MethodHost, v.MemberType.BaseType, Location, v.FlowIndex, v.Modifiers);
                        vr.StackIdx = v.StackIdx;

                        vr.StackIdx += Left.Type.BaseType.GetSize(Left.Type.BaseType) * Index;


                        return new AccessExpression(vr, (Left is AccessExpression) ? (Left as AccessExpression) : null,Left.position);
                    }
                    else if (ve.variable is FieldSpec)
                    {
                        FieldSpec v = (FieldSpec)ve.variable;
                        FieldSpec vr = new FieldSpec(v.NS, v.Name, v.Modifiers, v.MemberType.BaseType, Location);
                        vr.FieldOffset = v.FieldOffset;
                        vr.IsIndexed = true;
                        vr.FieldOffset += Left.Type.BaseType.GetSize(Left.Type.BaseType) * Index;

                        return new AccessExpression(vr, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);
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
            return new AccessExpression(Left as VariableExpression, Right, this);
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
                    ec.EmitPush(RegistersEnum.SI, 16, !Left.Type.IsMultiDimensionArray);
                }
                else
                {
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(EmitContext.A, 16);
                    ec.EmitPop(RegistersEnum.SI, 16);

                    ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.C, Size = 16, SourceValue = (ushort)Left.Type.BaseType.GetSize(Left.Type.BaseType) });
                    ec.EmitInstruction(new Multiply() { DestinationReg = EmitContext.C, Size = 80 });
                    ec.EmitInstruction(new Add() { SourceReg = EmitContext.A, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 16, !Left.Type.IsMultiDimensionArray);
                }
            }
            else
            {

                Left.EmitToStack(ec);
                ec.EmitPop(RegistersEnum.SI, 16);
                ec.EmitInstruction(new Add() { SourceValue = (ushort)((ushort)Index * (uint)Left.Type.BaseType.GetSize(Left.Type.BaseType)), DestinationReg = RegistersEnum.SI });
                ec.EmitPush(RegistersEnum.SI, 16, !Left.Type.IsMultiDimensionArray);

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
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 8, DestinationIsIndirect = !Left.Type.IsMultiDimensionArray, SourceReg = ec.GetLow(Register.Value) });
                }
                else
                {


                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(EmitContext.A, 16);
                    ec.EmitPop(RegistersEnum.SI, 16);

                    ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.C, Size = 16, SourceValue = (ushort)Left.Type.BaseType.GetSize(Left.Type.BaseType) });
                    ec.EmitInstruction(new Multiply() { DestinationReg = EmitContext.C, Size = 80 });
                    ec.EmitInstruction(new Add() { SourceReg = EmitContext.A, DestinationReg = RegistersEnum.SI });

                    ec.EmitPop(RegistersEnum.DI);
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 16, DestinationIsIndirect = !Left.Type.IsMultiDimensionArray, SourceReg = RegistersEnum.DI });
                }
            }
            else
            {


                Left.EmitToStack(ec);
                ec.EmitPop(RegistersEnum.SI, 16);
                ec.EmitInstruction(new Add() { SourceValue = (ushort)((ushort)Index * (ushort)Left.Type.BaseType.GetSize(Left.Type.BaseType)), DestinationReg = RegistersEnum.SI });
                if (IsByte)
                {
                    ec.EmitPop(Register.Value);
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 8, DestinationIsIndirect = !Left.Type.IsMultiDimensionArray, SourceReg = ec.GetLow(Register.Value) });
                }
                else
                {
                    ec.EmitPop(RegistersEnum.DI);
                    ec.EmitInstruction(new Mov() { DestinationReg = RegistersEnum.SI, Size = 16, DestinationIsIndirect = !Left.Type.IsMultiDimensionArray, SourceReg = RegistersEnum.DI });
                }
            }
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
        public bool LoadEffectiveAddress(EmitContext ec)
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
                    ec.EmitPush(RegistersEnum.SI,16);
                }
                else
                {
                    Left.EmitToStack(ec);
                    Right.EmitToStack(ec);
                    ec.EmitPop(EmitContext.A, 16);
                    ec.EmitPop(RegistersEnum.SI, 16);

                    ec.EmitInstruction(new Mov() { DestinationReg = EmitContext.C, Size = 16, SourceValue = (ushort)Left.Type.BaseType.GetSize(Left.Type.BaseType) });
                    ec.EmitInstruction(new Multiply() { DestinationReg = EmitContext.C, Size = 80 });
                    ec.EmitInstruction(new Add() { SourceReg = EmitContext.A, DestinationReg = RegistersEnum.SI });
                    ec.EmitPush(RegistersEnum.SI, 16);
                }
            }
            else
            {

                Left.EmitToStack(ec);
                ec.EmitPop(RegistersEnum.SI, 16);
                ec.EmitInstruction(new Add() { SourceValue = (ushort)((ushort)Index * (uint)Left.Type.BaseType.GetSize(Left.Type.BaseType)), DestinationReg = RegistersEnum.SI });
                ec.EmitPush(RegistersEnum.SI, 16);

            }
            return true;
        }
       
    }
}
