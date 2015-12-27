using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
    #region Unary Operators

    public class ValueOfOp : UnaryOp
    {
        MemberSpec ms;
        public ValueOfOp()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ValueOf;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (!Right.Type.IsPointer)
                ResolveContext.Report.Error(53, Location, "Value of operator cannot be used with non pointer types");
            // VOF
            if (Right is VariableExpression)
                ms = (Right as VariableExpression).variable;
            Right.Type = Right.Type.BaseType;
            if(Right.Type != null)
            CommonType = Right.Type.BaseType;
            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ms is VarSpec)
                ms.ValueOf(ec);
            else if (ms is FieldSpec)
                ms.ValueOf(ec);
            else if (ms is ParameterSpec)
                ms.ValueOf(ec);

            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (ms is VarSpec)
                ms.ValueOf(ec);
            else if (ms is FieldSpec)
                ms.ValueOf(ec);
            else if (ms is ParameterSpec)
                ms.ValueOf(ec);
            return true;
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            if (ms is VarSpec)
                ms.ValueOfStack(ec);
            else if (ms is FieldSpec)
                ms.ValueOfStack(ec);
            else if (ms is ParameterSpec)
                ms.ValueOfStack(ec);
            return true;
        }
        public override string CommentString()
        {
            return "*" ;
        }
    }
    public class LoadEffectiveAddressOp : UnaryOp
    {
        MemberSpec ms;
        public LoadEffectiveAddressOp()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.AddressOf;
        }
    
        public override SimpleToken DoResolve(ResolveContext rc)
        {
       
                // LEA
            if (Right is VariableExpression)
                ms = (Right as VariableExpression).variable;
            else
                ResolveContext.Report.Error(54, Location, "Address Of Operator does not support non variable expressions");
                CommonType = Right.Type.MakePointer();
                return this;
         
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ms is VarSpec)
                ms.LoadEffectiveAddress(ec);
            else if (ms is FieldSpec)
                ms.LoadEffectiveAddress(ec);
            else if (ms is ParameterSpec)
                ms.LoadEffectiveAddress(ec);

            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (ms is VarSpec)
                ms.LoadEffectiveAddress(ec);
            else if (ms is FieldSpec)
                ms.LoadEffectiveAddress(ec);
            else if (ms is ParameterSpec)
                ms.LoadEffectiveAddress(ec);
            return true;
        }
        public override string CommentString()
        {
            return  "&";
        }
    }
    [Terminal("!")]
    public class LogicalNotOperator : UnaryOp
    {
        public LogicalNotOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.LogicalNot;
        }
        AssignExpression ae;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type != BuiltinTypeSpec.Bool)
                ResolveContext.Report.Error(26, Location, "Logical not must be used with boolean type, use ~ instead");

            CommonType = BuiltinTypeSpec.Bool;
            ae = new AssignExpression(Right, new SimpleAssignOperator(), Right);
            ae = (AssignExpression)ae.DoResolve(rc);


            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment("!" + Right.CommentString());
                RegisterExpression.EmitUnaryOperation(ec, new Not(), ((RegisterExpression)Right).Register);
                return true;
            }

         
            Right.EmitToStack(ec);
            ec.EmitComment("!" + Right.CommentString());
            ec.EmitPop(Register.Value);



            ec.EmitInstruction(new Not() { DestinationReg =Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
  


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }
    }
    [Terminal("~")]
    public class OnesComplementOperator : UnaryOp
    {
        public OnesComplementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.OnesComplement;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "OnesComplement must be used with non boolean, pointer types, use ! instead");
            CommonType = Right.Type;

            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment("~" + Right.CommentString());
                RegisterExpression.EmitUnaryOperation(ec, new Not(), ((RegisterExpression)Right).Register);
                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("~" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Not() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
         


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }

    }
  
    [Terminal("£")]
    public class ZeroTestOperator : UnaryOp
    {
        public ZeroTestOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ZeroTest;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(32, Location, "Zero Operators must be used with non boolean, pointer types");
         
            CommonType = BuiltinTypeSpec.Bool;


            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment("£" + Right.CommentString());
              
                ec.EmitInstruction(new Compare() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 0, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
                ec.EmitPush(((RegisterExpression)Right).Register);
             
                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("£" + Right.CommentString());
            ec.EmitPop(Register.Value);


            ec.EmitInstruction(new Compare() { DestinationReg = Register.Value, SourceValue = 0, Size = 80 });
            ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.Zero, ConditionalTestEnum.NotZero);
            ec.EmitPush(Register.Value);
       


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }

    }
    [Terminal("$")]
    public class ParityTestOperator : UnaryOp
    {
        public ParityTestOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.ParityTest;
        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(33, Location, "Parity Operators must be used with non boolean, pointer types");
         
            CommonType = BuiltinTypeSpec.Bool;

            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment("$" + Right.CommentString());

                ec.EmitInstruction(new Test() { DestinationReg = ((RegisterExpression)Right).Register, SourceValue = 1, Size = 80 });
                ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
                ec.EmitPush(((RegisterExpression)Right).Register);

                return true;
            }
            Right.EmitToStack(ec);
            ec.EmitComment("$" + Right.CommentString());
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new Test() { DestinationReg = Register.Value, SourceValue = 1, Size = 80 });
            ec.EmitBoolean(ec.GetLow(Register.Value), ConditionalTestEnum.ParityEven, ConditionalTestEnum.ParityOdd);
            ec.EmitPush(Register.Value);
     


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            return Emit(ec);
        }

    }

    [Terminal("--")]
    public class DecrementOperator : UnaryOp
    {
        public DecrementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.PostfixDecrement;
        }
        AssignExpression ae;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "Unary operator must be used with non boolean, pointer types, use ! instead");

            ae = new AssignExpression(Right, new SimpleAssignOperator(), Right);
            ae = (AssignExpression)ae.DoResolve(rc);
            CommonType = Right.Type;
            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment( Right.CommentString()+"--");
                RegisterExpression.EmitUnaryOperation(ec, new Dec(), ((RegisterExpression)Right).Register,false);
                return true;
            }
      
            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "-- ");
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new Dec() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);


            ae.EmitFromStack(ec);
         //   ec.EmitPush(ec.FirstRegister());
        


            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "--");
                RegisterExpression.EmitUnaryOperation(ec, new Dec(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "-- ");
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new Dec() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
            ec.EmitPush(Register.Value);
            ae.EmitFromStack(ec);
          
        
            return true;
        }
    }
    [Terminal("++")]
    public class IncrementOperator : UnaryOp
    {
        public IncrementOperator()
        {
            Register = RegistersEnum.AX;
            Operator = UnaryOperator.PostfixIncrement;
        }
        AssignExpression ae;
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (Right.Type == BuiltinTypeSpec.Bool && Right.Type.IsBuiltinType && !Right.Type.IsPointer)
                ResolveContext.Report.Error(25, Location, "Unary operator must be used with non boolean, pointer types, use ! instead");


            ae = new AssignExpression(Right, new SimpleAssignOperator(), Right);
            ae = (AssignExpression)ae.DoResolve(rc);
            CommonType = Right.Type;
            if (Right is RegisterExpression)
                RegisterOperation = true;

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override bool Emit(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "++");
                RegisterExpression.EmitUnaryOperation(ec, new INC(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "++ ");
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new INC() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);


            ae.EmitFromStack(ec);
            //   ec.EmitPush(ec.FirstRegister());



            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (RegisterOperation)
            {
                ec.EmitComment(Right.CommentString() + "++");
                RegisterExpression.EmitUnaryOperation(ec, new INC(), ((RegisterExpression)Right).Register, false);
                return true;
            }

            Right.EmitToStack(ec);
            ec.EmitComment(Right.CommentString() + "++ ");
            ec.EmitPop(Register.Value);

            ec.EmitInstruction(new INC() { DestinationReg = Register.Value, Size = 80 });
            ec.EmitPush(Register.Value);
            ec.EmitPush(Register.Value);
            ae.EmitFromStack(ec);


            return true;
        }
    }

    // TODO FIX SIZEOF/CAST

    public class CastOperator : Expr
    {
        bool nofix = true;
        bool to16 = false;
        bool tosign = false;

        protected TypeIdentifier _type;
        protected Expr _target;
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

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _type = (TypeIdentifier)_type.DoResolve(rc);
            Type = _type.Type;
            _target = (Expr)_target.DoResolve(rc);

            // simple fix typedef typedef => type
            if (Type.IsTypeDef && Type.GetTypeDefBase(Type) == _target.Type)
                Type = _target.Type;
            else if (_target.Type.IsTypeDef && _target.Type.GetTypeDefBase(_target.Type) == Type) // simple fix (type => typedef)
                _target.Type = Type;
            else if (Type.IsPointer && _target.Type == BuiltinTypeSpec.UInt)
            {
                _target.Type = Type;
                nofix = false;
            }
            else if (_target.Type.IsPointer && Type == BuiltinTypeSpec.UInt)
            {
                Type = _target.Type;
                nofix = false;
            }
            else if (_target is ConstantExpression) // convert const
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
                else return _target;
            }
            else if (_target is CastOperator) // cast under cast
                _target = ((CastOperator)_target).Target;
            else if (_target.Type.BuiltinType == BuiltinTypes.Byte || _target.Type.BuiltinType == BuiltinTypes.SByte) // byte or sbyte => int or uint
            {
                if (Type.BuiltinType == BuiltinTypes.UInt)
                {
                    nofix = false;
                    to16 = true;
                    tosign = false;
                }
                else if (Type.BuiltinType == BuiltinTypes.Int)
                {
                    nofix = false;
                    to16 = true;
                    tosign = true;
                }
                else ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
            }
            else if (_target.Type.BuiltinType == BuiltinTypes.Int || _target.Type.BuiltinType == BuiltinTypes.UInt) // int or uint => byte or sbyte
            {
                if (Type.BuiltinType == BuiltinTypes.Byte)
                {
                    nofix = false;
                    to16 = false;
                    tosign = false;
                }
                else if (Type.BuiltinType == BuiltinTypes.SByte)
                {
                    nofix = false;
                    to16 = false;
                    tosign = true;
                }
                else ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);
            }
            else
                ResolveContext.Report.Error(27, Location, "Invalid cast " + Type.Name + " to " + _target.Type.Name);

            return this;
        }
        public override bool Resolve(ResolveContext rc)
        {

            return _type.Resolve(rc) && _target.Resolve(rc);
        }
        public override string CommentString()
        {
            return "Cast " + _target.CommentString() + " to " + Type.ToString();
        }
        public override bool Emit(EmitContext ec)
        {

            if (nofix)
            {
                ec.EmitComment(CommentString());
                return _target.Emit(ec);
            }
            else
            {
                _target.Emit(ec);
                // cast
                RegistersEnum src = RegistersEnum.AX;

                ec.EmitPop(src); // take target
                ec.EmitComment(CommentString());
                if (to16 && tosign)
                    Conversion.EmitConvert8To16Signed(ec, src);
                else if (to16 && !tosign)
                    Conversion.EmitConvert8To16Unsigned(ec, src);
                else if (!to16)
                    Conversion.EmitConvert16To8(ec, src);
                ec.EmitPush(src);

             
                return true;

            }
        }
        public override bool EmitToStack(EmitContext ec)
        {

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
    }
    public class SizeOfOperator : Expr
    {
        public ushort Size
        {
            get;
            set;
        }

        private TypeIdentifier _type;
        private Identifier _id;
        private TypePointer _ptr;


        [Rule(@"<Op Unary> ::= ~sizeof ~'(' <Type> ~')'")]
        public SizeOfOperator(TypeIdentifier type)
        {
            _type = type;
        }
        [Rule(@"<Op Unary> ::= ~sizeof ~'(' Id <Pointers> ~')'")]
        public SizeOfOperator(Identifier type, TypePointer tp)
        {
            _ptr = tp;
            _id = type;

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            if (_type != null)
            {
                _type = (TypeIdentifier)_type.DoResolve(rc);
                Type = _type.Type;
                Size = (ushort)Type.Size;
            }

            if (_id != null && _ptr != null)
            {
                Type = rc.Resolver.TryResolveVar(_id.Name).MemberType;
                _ptr = (TypePointer)_ptr.DoResolve(rc);
                Size = (ushort)Type.Size;
                if (_ptr.PointerCount > 0)
                    Size = 2;
                Type = PointerTypeSpec.MakePointer(Type, _ptr.PointerCount);

            }

            return new UIntConstant(Size,Location) ;
        }
        public override bool Resolve(ResolveContext rc)
        {
            Size = 0;
            return true;
        }
     /*   public override bool Emit(EmitContext ec)
        {
            RegistersEnum acc = ec.GetNextRegister();
            ec.EmitInstruction(new Mov() { DestinationReg = acc, SourceValue = (uint)this.Size, Size = 16 });
            return true;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (uint)this.Size, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (uint)this.Size, Size = 16 });
            return true;
        }

        public override string CommentString()
        {
            return "sizeOf(" + Type.Name + ")";
        }*/
    }
    #endregion
}
