﻿using bsn.GoldParser.Parser;
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
        internal MemberSpec RootVar = null;
        internal AccessExpression Parent = null;
        internal AccessExpression Child = null;
        public bool IsByIndex { get; set; }

  
        public override string ToString()
        {
            if (variable != null)
                return variable.Name.ToString();
            else return "NULL";
        }
        public AccessExpression(MemberSpec ms, AccessExpression parent, LineInfo pos)
            : base(ms)
        {
            position = pos;
            // Set Parent & Child
            if (parent != null )
            {
                Parent = parent;
                Parent.Child = this;
            }
            else
            Parent = null;

         



            IsByAdr = false;
            IsExpr = false;
     
            Type = ms.MemberType;
        }
        /// <summary>
        /// ByVal ccess operator
        /// </summary>
        /// <param name="ms"></param>
        public AccessExpression(MemberSpec ms, AccessExpression parent,LineInfo pos, bool adr = false, int index = 0, TypeSpec mem = null)
            : base(ms)
        {
            position = pos;
            if (adr)
            {
                RootVar = ms;
                SetNext();
                variable = new RegisterSpec(mem, LastUsed, Location, index);

            }
            if (parent != null)
            {
                Parent = parent;
                parent.Child = this;
            }
            IsByAdr = adr;
            IsExpr = false;
            AccessIndex = index;
            MemberT = mem;
            Type = mem != null ? mem : ms.MemberType;
        }

        public VariableExpression Left { get; set; }
        public Expr Right { get; set; }
        public ByIndexOperator Operator { get; set; }
        public AccessExpression(VariableExpression left, Expr right, ByIndexOperator op)
            : base(left.variable)
        {
            position = left.position;
            IsByIndex = true;
            IsExpr = true;
            Left = left;
            Right = right;
            Operator = op;
           
            Type = left.Type.BaseType;
         
           
        }

        public void EmitIndirectionsPop(EmitContext ec)
        {
            ec.EmitComment("CURRENT IS " + ToString() + " PARENT IS " + (Parent == null ? "NULL" : Parent.Name) + " ISEXPR = " + IsExpr.ToString() + " ISADR = " + IsByAdr.ToString() + "  ISBYIDX = " + IsByIndex.ToString());

            if (Parent != null) // Go to father
            {
                Parent.EmitIndirectionsPop(ec); // go to first
                // check if is indirection
                if (IsByAdr)
                {
                    Parent.variable.EmitToStack(ec);
                    ec.EmitPop((variable as RegisterSpec).Register);
                    return;
                }
                if (IsByIndex)
                {
                    ec.EmitComment("INDEX INDIRECTION");
                    Parent.variable.EmitToStack(ec);
                    ec.EmitPop((variable as RegisterSpec).Register);

                }
                // or pass
            }
            else if (Parent == null && RootVar != null) // by index indirection
            {
                RootVar.EmitToStack(ec);
                ec.EmitPop((variable as RegisterSpec).Register);
            }
            //else if (Child == null)
            //    // is last
            //    variable.EmitFromStack(ec);


        }
        public void EmitIndirections(EmitContext ec)
        {
           
      //   ec.EmitComment("CURRENT IS " + ToString() + " PARENT IS "+ (Parent==null?"NULL":Parent.Name) + " ISEXPR = "+IsExpr.ToString() + " ISADR = "+IsByAdr.ToString() + "  ISBYIDX = "+IsByIndex.ToString());
         if (Parent != null) // Go to father
            {
                Parent.EmitIndirections(ec); // go to first
                // check if is indirection
                if (IsByAdr)
                {
                    Parent.variable.EmitToStack(ec);
                    ec.EmitPop((variable as RegisterSpec).Register);
                    return;
                }
              
                // or pass
            }
         else if (Parent == null && RootVar != null) // by index indirection
         {
             RootVar.EmitToStack(ec);
             ec.EmitPop((variable as RegisterSpec).Register);
         }
            //else if(Child == null) 
            //    // is last
            //    variable.EmitToStack(ec);
  
            
/*
     if (Parent == null && RootVar != null && variable is RegisterSpec) // indirect
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
           */
        }
        public override bool Emit(EmitContext ec)
        {
            return EmitToStack(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {
            ec.EmitComment("Pop Indirections ");
            EmitIndirectionsPop(ec);
            ec.EmitComment("End Pop Indirections ");
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
            ec.EmitComment("Push Indirections ");
            EmitIndirections(ec);
            ec.EmitComment("End Push Indirections ");

            if (!IsExpr && !IsByAdr) // By Value Access

                return base.EmitToStack(ec);

            else if (IsByAdr)
            {

                if (variable is VarSpec)
                    return variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is FieldSpec)
                    return variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is ParameterSpec)
                    return variable.ValueOfAccess(ec, AccessIndex, MemberT);
                else if (variable is RegisterSpec)
                    return variable.EmitToStack(ec);
                return true;
            }
            else if (IsExpr)
                return Operator.EmitToStack(ec);
            else throw new NotSupportedException();
        }

        public override bool LoadEffectiveAddress(EmitContext ec)
        {
            ec.EmitComment("Indirections ");
            EmitIndirections(ec);
            ec.EmitComment("End Indirections ");
            if (!IsExpr && !IsByAdr) // By Value Access
                return base.LoadEffectiveAddress(ec);
            else if (IsByAdr)
            {

                if (variable is VarSpec)
                    variable.LoadEffectiveAddress(ec);
                else if (variable is FieldSpec)
                    variable.LoadEffectiveAddress(ec);
                else if (variable is ParameterSpec)
                    variable.LoadEffectiveAddress(ec);
                else if (variable is RegisterSpec)
                    variable.LoadEffectiveAddress(ec);
                return true;
            }
            else
                return Operator.LoadEffectiveAddress(ec);

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
