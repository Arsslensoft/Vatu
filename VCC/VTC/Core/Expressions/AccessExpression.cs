using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class AccessExpression : VariableExpression
    {
        public static RegistersEnum LastUsed;
        public static void SetNext()
        {
            if (LastUsed == RegistersEnum.SI)
                LastUsed = RegistersEnum.DI;
            else LastUsed = RegistersEnum.SI;
        }


        bool IsExpr { get; set; }
        bool IsByAdr { get; set; }
        int AccessIndex { get; set; }
        TypeSpec MemberT { get; set; }
        MemberSpec RootVar = null;
        AccessExpression Parent = null;

        public bool IsByIndex { get; set; }

        public AccessExpression(MemberSpec ms, AccessExpression parent)
            : base(ms)
        {

            if (parent != null && parent.IsByAdr)
                Parent = parent;




            IsByAdr = false;
            IsExpr = false;

            Type = ms.MemberType;
        }
        /// <summary>
        /// ByVal ccess operator
        /// </summary>
        /// <param name="ms"></param>
        public AccessExpression(MemberSpec ms, AccessExpression parent, bool adr = false, int index = 0, TypeSpec mem = null)
            : base(ms)
        {
            if (adr)
            {
                RootVar = ms;
                SetNext();
                variable = new RegisterSpec(mem, LastUsed, Location, index);

            }
            if (parent != null)
                Parent = parent;
            IsByAdr = adr;
            IsExpr = false;
            AccessIndex = index;
            MemberT = mem;
            Type = mem != null ? mem : ms.MemberType;
        }

        public VariableExpression Left { get; set; }
        public Expr Right { get; set; }
        public AccessOp Operator { get; set; }
        public AccessExpression(VariableExpression left, Expr right, AccessOp op)
            : base(left.variable)
        {
            IsByIndex = true;
            IsExpr = true;
            Left = left;
            Right = right;
            Operator = op;
            Type = left.Type.BaseType;
        }


        public void EmitIndirections(EmitContext ec)
        {
            if (Parent == null && RootVar != null && variable is RegisterSpec)
            {
                RootVar.EmitToStack(ec);
                ec.EmitPop((variable as RegisterSpec).Register);
            }

            else if (Parent != null && !Parent.IsByIndex)
            {
                if (Parent.Parent != null)
                    Parent.EmitIndirections(ec);

                if (IsByAdr)
                {

                    Parent.variable.EmitToStack(ec);
                    ec.EmitPop((variable as RegisterSpec).Register);
                }
            }
            else if (Parent != null) // by index
            {
                Parent.EmitToStack(ec);
                ec.EmitPop((variable as RegisterSpec).Register);
            }
        }
        public override bool Emit(EmitContext ec)
        {
            ec.EmitComment("Indirections ");
            EmitIndirections(ec);

            if (!IsExpr && !IsByAdr)
                return base.Emit(ec);
            else if (IsByAdr)
            {
                if (variable is VarSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is FieldSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is ParameterSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is RegisterSpec)
                    variable.EmitToStack(ec);
                return true;
            }
            else
                return Operator.Emit(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitComment("Indirections ");
            EmitIndirections(ec);
            if (!IsExpr && !IsByAdr)
                return base.EmitFromStack(ec);
            else if (IsByAdr)
            {

                if (variable is VarSpec)
                    variable.ValueOfStackAccess(ec, AccessIndex, MemberT);
                else if (variable is FieldSpec)
                    variable.ValueOfStackAccess(ec, AccessIndex, MemberT);
                else if (variable is ParameterSpec)
                    variable.ValueOfStackAccess(ec, AccessIndex, MemberT);
                else if (variable is RegisterSpec)
                    variable.EmitFromStack(ec);
                return true;

            }
            else
                return Operator.EmitFromStack(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitComment("Indirections ");
            EmitIndirections(ec);


            if (!IsExpr && !IsByAdr)
                return base.EmitToStack(ec);
            else if (IsByAdr)
            {

                if (variable is VarSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is FieldSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is ParameterSpec)
                    variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is RegisterSpec)
                    variable.EmitToStack(ec);
                return true;
            }
            else
                return Operator.EmitToStack(ec);
        }
        public override string CommentString()
        {
            if (!IsExpr)
                return base.CommentString();
            else
                return Operator.CommentString();
        }

        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            EmitToStack(ec);
            ec.EmitPop(EmitContext.A);
            ec.EmitInstruction(new Compare() { DestinationReg = EmitContext.A, SourceValue = (ushort)1 });

            ec.EmitBooleanBranch(v, truecase, ConditionalTestEnum.Equal, ConditionalTestEnum.NotEqual);
            return true;
        }
    }
}
