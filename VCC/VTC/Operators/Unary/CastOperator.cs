using VTC.Base.GoldParser.Parser;
using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
	public class CastOperator : Expr
    {
        bool nofix = true;
        bool to16 = false;
        bool tosign = false;
        protected MethodSpec OvlrdOp;
        protected TypeIdentifier _type;
        protected Expr _target;
        protected LoadEffectiveAddressOp FloatAdr;
        #region CAST OVERLOADING
        public virtual bool EmitOverrideOperatorFromStack(EmitContext ec)
        {
 
            ec.EmitComment("Override Cast Operator : " + " (" + _type.Type.Name + ")" + _target.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperator(EmitContext ec)
        {
            _target.EmitToStack(ec);
            ec.EmitComment("Override Cast Operator : " + " (" + _type.Type.Name + ")" + _target.CommentString());
            ec.EmitCall(OvlrdOp);
            ec.EmitPush(EmitContext.A);
            return true;
        }
        public virtual bool EmitOverrideOperatorBranchable(EmitContext ec, Label truecase, bool v, ConditionalTestEnum cond, ConditionalTestEnum acond)
        {
            _target.EmitToStack(ec);
            ec.EmitComment("Override Cast Operator : " + " (" + _type.Type.Name + ")" + _target.CommentString());
            ec.EmitCall(OvlrdOp);



            if (_type.Type.Size == 1)
                ec.EmitInstruction(new Compare() { DestinationReg = ec.GetLow(EmitContext.A), SourceValue = EmitContext.TRUE, Size = 80 });
            else
                ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = EmitContext.TRUE, Size = 80 });

            if (v)
                ec.EmitInstruction(new ConditionalJump() { Condition = cond, DestinationLabel = truecase.Name });
            else
                ec.EmitInstruction(new ConditionalJump() { Condition = acond, DestinationLabel = truecase.Name });

            return true;
        }
        #endregion
        public Expr Target
        {
            get { return _target; }
        }
        [Rule(@"<Op Unary> ::= ~'(' <Type> ~')' <Op Unary>")]
        public CastOperator(TypeIdentifier id, Expr target)
        {
            _target = target;
            _type = id;

        }
        PolymorphicClassExpression GetClassInheritancePolymorphicExpr()
        {
            MemberSpec struct_var = null;

            if (_target is VariableExpression)
                struct_var = (_target as VariableExpression).variable;
            else return null;

            if (struct_var is VarSpec)
            {
                VarSpec v = (VarSpec)struct_var;

                VarSpec dst = new VarSpec(v.NS, v.Name, v.MethodHost, Type, Location, v.FlowIndex, v.Modifiers, true);
                dst.VariableStackIndex = v.VariableStackIndex + CastOffset;
                dst.InitialStackIndex = dst.VariableStackIndex;
                return new PolymorphicClassExpression(dst, CastOffset,Type, _target.position);

            }
            else if (struct_var is RegisterSpec)
            {
                RegisterSpec v = (RegisterSpec)struct_var;

                RegisterSpec dst = new RegisterSpec(Type, v.Register, Location, 0, true);
                dst.RegisterIndex = v.RegisterIndex + CastOffset;
                dst.InitialRegisterIndex = dst.RegisterIndex;
                return new PolymorphicClassExpression(dst, CastOffset, Type, _target.position);
            }
            else if (struct_var is FieldSpec)
            {
                FieldSpec v = (FieldSpec)struct_var;

                FieldSpec dst = new FieldSpec(v.NS, v.Name, v.Modifiers, Type, Location, true);
                dst.FieldOffset = v.FieldOffset + CastOffset;
                dst.InitialFieldIndex = dst.FieldOffset;
                return new PolymorphicClassExpression(dst, CastOffset, Type, _target.position);
            }
            else if (struct_var is ParameterSpec)
            {
                ParameterSpec v = (ParameterSpec)struct_var;

                ParameterSpec dst = new ParameterSpec(v.NS, v.Name, v.MethodHost, Type, Location, v.InitialStackIndex, v.Modifiers, true);

                dst.StackIdx = v.StackIdx + CastOffset;

                return new PolymorphicClassExpression(dst, CastOffset, Type, _target.position);
            }
            return null;
        }
        AccessExpression GetInheritanceAccessExpr()
        {
            MemberSpec struct_var = null;

            if (_target is VariableExpression)
                struct_var = (_target as VariableExpression).variable;
            else return null;

            if (struct_var is VarSpec)
            {
                VarSpec v = (VarSpec)struct_var;

                VarSpec dst = new VarSpec(v.NS, v.Name, v.MethodHost, Type, Location, v.FlowIndex, v.Modifiers, true);
                dst.VariableStackIndex = v.VariableStackIndex + CastOffset;
                dst.InitialStackIndex = dst.VariableStackIndex;
                return new AccessExpression(dst, (_target is AccessExpression) ? (_target as AccessExpression) : null, _target.position);

            }
            else if (struct_var is RegisterSpec)
            {
                RegisterSpec v = (RegisterSpec)struct_var;

                RegisterSpec dst = new RegisterSpec(Type, v.Register, Location, 0, true);
                dst.RegisterIndex = v.RegisterIndex + CastOffset;
                dst.InitialRegisterIndex = dst.RegisterIndex;
                return new AccessExpression(dst, (_target is AccessExpression) ? (_target as AccessExpression) : null, _target.position);

            }
            else if (struct_var is FieldSpec)
            {
                FieldSpec v = (FieldSpec)struct_var;

                FieldSpec dst = new FieldSpec(v.NS, v.Name, v.Modifiers, Type, Location, true);
                dst.FieldOffset = v.FieldOffset + CastOffset;
                dst.InitialFieldIndex = dst.FieldOffset;
                return new AccessExpression(dst, (_target is AccessExpression) ? (_target as AccessExpression) : null, _target.position);
            }
            else if (struct_var is ParameterSpec)
            {
                ParameterSpec v = (ParameterSpec)struct_var;

                ParameterSpec dst = new ParameterSpec(v.NS, v.Name, v.MethodHost, Type, Location, v.InitialStackIndex, v.Modifiers, true);

                dst.StackIdx = v.StackIdx + CastOffset;

                return new AccessExpression(dst, (_target is AccessExpression) ? (_target as AccessExpression) : null, _target.position);
            }
            return null;
        }
       public override bool Resolve(ResolveContext rc)
        {

            return _type.Resolve(rc) && _target.Resolve(rc);
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _type = (TypeIdentifier)_type.DoResolve(rc);
            Type = _type.Type;
            _target = (Expr)_target.DoResolve(rc);


            rc.Resolver.TryResolveMethod("Op_explicit_Cast_" + _type.Type.NormalizedName, ref OvlrdOp, new TypeSpec[1] { _target.Type });
      

            if (rc.CurrentMethod == OvlrdOp)
                OvlrdOp = null;
            if (OvlrdOp == null)
            {
                // simple fix typedef typedef => type
                if (TypeDefFix())
                {
                    _target.Type = Type;
                    return Target;
                }
                else if (PointerFix() || ReferenceFix() || TemplateFix())
                    return Target;
                else if (InheritanceFix())
                {
                    if (Type.IsClass)
                    {
                        PolymorphicClassExpression pe = GetClassInheritancePolymorphicExpr();
                        if (pe == null)
                            ResolveContext.Report.Error(0, Location, "Can't cast to inherited class");
                        else return pe;
                    }
                    else
                    {
                        AccessExpression ae = GetInheritanceAccessExpr();
                        if (ae == null)
                            ResolveContext.Report.Error(0, Location, "Can't cast to inherited struct");
                        else return ae;
                    }
                    return this;
                }
                else if (Type.Equals(_target.Type))
                    return _target;
       
                else if (FloatFix())
                {
                    if (!float_to_int)
                    {
                        LoadEffectiveAddressOp lea = new LoadEffectiveAddressOp();
                        lea.position = FloatAdr.position;
                        _target = new UnaryOperation(_target, lea);

                        _target = (Expr)_target.DoResolve(rc);
                    }
                }
                else if (ConstantFix(rc))
                    return Target;

                else if (_target is CastOperator) // cast under cast
                    return Target;

                else if (!WordToByteFix() && !ByteToWordFix())
                    ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
            }
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (OvlrdOp != null)
                fc.MarkAsUsed(OvlrdOp);
            return _target.DoFlowAnalysis(fc);
        }
        public override string CommentString()
        {
            return "Cast " + _target.CommentString() + " to " + Type.ToString();
        }
        public override bool Emit(EmitContext ec)
        {
            return EmitToStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (OvlrdOp != null)
                return EmitOverrideOperator(ec);

            if (float_fix)
            {
                if (float_to_int)
                    Conversion.EmitConvertFloatTo16(ec, _target);
                else Conversion.EmitConvert16ToFloat(ec, _target);
                return true;
            }
            if (nofix)
            {
                ec.EmitComment(CommentString());
                return _target.EmitToStack(ec);
            }
            else
            {
                _target.EmitToStack(ec);
                // cast

                RegistersEnum dst = RegistersEnum.AX;
                ec.EmitPop(dst); // take target
                ec.EmitComment(CommentString());
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, dst);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, dst);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, dst);
                ec.EmitPush(dst);

         
                return true;

            }
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            if (OvlrdOp != null)
            {
                EmitOverrideOperator(ec);
                ec.EmitPop(rg);
                return true;
            }
            if (float_fix)
            {
                if (float_to_int)
                    Conversion.EmitConvertFloatTo16(ec, _target);
                else Conversion.EmitConvert16ToFloat(ec, _target);
                ec.EmitPop(rg); // take target

            }
            if (nofix)
                return _target.EmitToRegister(ec, rg);
            else
            {
                _target.EmitToRegister(ec, rg);
                // cast
                RegistersEnum src = rg;

                ec.EmitPop(src); // take target
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, src);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, src);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, src);



                return true;
            }
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (OvlrdOp != null)
            {
                EmitOverrideOperatorFromStack(ec);
                return _target.EmitFromStack(ec);
            }
            if (float_fix)
            {
                if (float_to_int)
                    Conversion.EmitConvertFloatTo16Stack(ec, _target);
                else Conversion.EmitConvert16ToFloatStack(ec, _target);

                return true;
            }
            if (nofix)
                return _target.EmitFromStack(ec);
            else
            {

                // cast
                RegistersEnum src = RegistersEnum.AX;

                ec.EmitPop(src); // take target
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, src);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, src);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, src);

                ec.EmitPush(src);



                return _target.EmitFromStack(ec);
            }
        }
        bool float_to_int = false;
        bool float_fix = false;
        bool FloatFix()
        {
            if (_target.Type.IsFloat && !_target.Type.IsPointer) // float -> integer
            {
                if (Type.BuiltinType == BuiltinTypes.Int || Type.BuiltinType == BuiltinTypes.UInt)
                {
                    float_fix = true;
                    float_to_int = false;
                    return true;
                }
                else return false;
            }
            else if (Type.BuiltinType == BuiltinTypes.UInt || Type.BuiltinType == BuiltinTypes.Int) // integer->float
            {
                if (_target.Type.IsFloat && !_target.Type.IsPointer)
                {
                    float_fix = true;
                    float_to_int = true;
                    return true;
                }
                else return false;
            }
            else return false;
        }
        bool ByteToWordFix()
        {
            if (_target.Type.BuiltinType == BuiltinTypes.Byte || _target.Type.BuiltinType == BuiltinTypes.SByte) // byte or sbyte => int or uint
            {
                if (Type.BuiltinType == BuiltinTypes.UInt)
                {
                    nofix = false;
                    to16 = true;
                    tosign = false;
                    return true;
                }
                else if (Type.BuiltinType == BuiltinTypes.Int)
                {
                    nofix = false;
                    to16 = true;
                    tosign = true;
                    return true;
                }
                else ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
            }
            return false;
        }
        bool WordToByteFix(){
             if (_target.Type.BuiltinType == BuiltinTypes.Int || _target.Type.BuiltinType == BuiltinTypes.UInt) // int or uint => byte or sbyte
                {
                    if (Type.BuiltinType == BuiltinTypes.Byte)
                    {
                        nofix = false;
                        to16 = false;
                        tosign = false;
                        return true;
                    }
                    else if (Type.BuiltinType == BuiltinTypes.SByte)
                    {
                        nofix = false;
                        to16 = false;
                        tosign = true;
                        return true;
                    }
                    else if (Type.BuiltinType == BuiltinTypes.Int || Type.BuiltinType == BuiltinTypes.UInt)
                    {
                        nofix = true;
                          return true;
                    }

                    else ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
                }
            return false;
        }
        bool ConstantFix(ResolveContext rc)
        {
            if (_target is ConstantExpression) // convert const
            {
                bool c = false;
                try
                {
                    _target = ((ConstantExpression)_target).ConvertExplicitly(rc, Type, ref c);
                }
                catch
                {
                    c = false;
                }
                if (!c)
                    ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
                else return true;
            }
            return false;
        }
        bool ReferenceFix()
        {
            if (_target.Type is ReferenceTypeSpec && Type.Equals(_target.Type.BaseType))
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
            else return false;
        }
        bool PointerFix()
        {
            if (Type.IsDelegate && (_target.Type.Equals(BuiltinTypeSpec.UInt) || _target.Type.Equals(BuiltinTypeSpec.Pointer)))
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
            else if (_target.Type.IsDelegate && (Type.Equals(BuiltinTypeSpec.UInt) || Type.Equals(BuiltinTypeSpec.Pointer)))
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
            else if (Type.IsPointer && (_target.Type.Equals(BuiltinTypeSpec.UInt) || _target.Type.Equals(BuiltinTypeSpec.Pointer)))
                {
                    _target.Type = Type;
                    nofix = true;
                    return true;
                }
            else if (_target.Type.IsPointer && (Type.Equals(BuiltinTypeSpec.UInt) || Type.Equals(BuiltinTypeSpec.Pointer)))
                {
                    _target.Type = Type;
                    nofix = true;
                    return true;
                }
             
                else if (_target.Type.Equals(BuiltinTypeSpec.Pointer) && Type.Equals(BuiltinTypeSpec.UInt))
                {
                    _target.Type = BuiltinTypeSpec.UInt;
                    nofix = true;
                    return true;
                }
                else if (_target.Type.Equals(BuiltinTypeSpec.UInt) && Type.Equals(BuiltinTypeSpec.Pointer))
                {
                    _target.Type = BuiltinTypeSpec.Pointer;
                    
                    nofix = true;
                    return true;
                }

            else if (Type.IsPointer && _target.Type.IsPointer)
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
          
            return false;
        }

        bool TypeDefFix()
        {
            TypeSpec a=Type.GetTypeDefBase(Type),b=_target.Type.GetTypeDefBase(_target.Type);
            
            if (a .Equals(b))
                return true;
            else return false;
        }
        bool TemplateFix()
        {
            if (Type.IsTemplate && _target.Type.IsTemplate && (Type as TemplateTypeSpec).Size == (_target.Type as TemplateTypeSpec).Size)
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
            else if ( _target.Type.IsTemplate && Type.Size ==_target.Type.Size)
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
            else if (Type.IsTemplate && Type.Size == _target.Type.Size)
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
            else if (_target.Type.IsForeignType && Type.IsForeignType && (Type.IsTemplateBased || _target.Type.IsTemplateBased) && Type.Size == _target.Type.Size)
            {
                _target.Type = Type;
                nofix = true;
                return true;
            }
            
            

            return false;
        }
        bool ispointer = false;
        int CastOffset = 0;
        bool GetInheritedStructIndex(StructTypeSpec st,StructTypeSpec cmp,ref int off)
        {
           
            foreach (StructTypeSpec s in st.Inherited)
            {
                if (s.Equals(cmp))
                    return true;
                else
                {
                    // check child inheritance
                    int rel = 0; // relative offset for child struct
                    if (GetInheritedStructIndex(s, cmp, ref rel))
                    {
                        off += rel;
                        return true;
                    }
                    else off += s.Size;
                    
                }
            }
            return false;
        }

        bool GetInheritedClassIndex(ClassTypeSpec st, TypeSpec cmp, ref int off)
        {

            foreach (TypeSpec s in st.Inherited)
            {
                if (s.Equals(cmp))
                    return true;
                else
                {
                    // check child inheritance
                    int rel = 0; // relative offset for child struct

                    if (s is ClassTypeSpec && GetInheritedClassIndex(s as ClassTypeSpec, cmp, ref rel))
                    {
                        off += rel;
                        return true;
                    }
                    else off += s.GetAllocSize(s);

                }
            }
            return false;
        }
        bool InheritanceFix()
        {
            CastOffset = 0;
            if ( _target is VariableExpression && _target.Type.IsStruct && Type.IsStruct && GetInheritedStructIndex(_target.Type as StructTypeSpec, Type as StructTypeSpec, ref CastOffset))
            {
                ispointer = _target.Type.IsPointer;
                return true;
            }
            else if (_target is VariableExpression && _target.Type.IsClass && Type.IsClass && GetInheritedClassIndex(_target.Type as ClassTypeSpec, Type as ClassTypeSpec, ref CastOffset))
            {
                ispointer = _target.Type.IsPointer;
                return true;
            }
            return false;
        }
    }
  
	
}