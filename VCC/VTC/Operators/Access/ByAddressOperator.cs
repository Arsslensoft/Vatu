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






    [Terminal("->")]
    public class ByAddressOperator : AccessOp
    {
        public ByAddressOperator()
        {
            Register = RegistersEnum.AX;
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

            if (mem.MemberType.IsPointer)
            {
                if (!mem.MemberType.IsForeignType)
                    ResolveContext.Report.Error(15, Location, "'->' operator allowed only with struct union based types");
                else if (mem.MemberType.IsStruct)
                {
                    StructTypeSpec stp = null;
                    if (Left is AccessExpression && (Left as AccessExpression).IsByIndex)
                        stp = (StructTypeSpec)mem.MemberType.BaseType.BaseType;
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
                    UnionTypeSpec stp = null;
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
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null,lv.position, true,  index, tmp.MemberType);

                        }
                        else if (struct_var is RegisterSpec)
                        {
                            RegisterSpec dst = (RegisterSpec)struct_var;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null,lv.position, true,  index, tmp.MemberType);

                        }
                        else if (struct_var is FieldSpec)
                        {
                            FieldSpec dst = (FieldSpec)struct_var;


                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);
                        }
                        else if (struct_var is ParameterSpec)
                        {
                            ParameterSpec dst = (ParameterSpec)struct_var;


                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);
                        }
                        this.CommonType = tmp.MemberType;
                    }
                }

                // error
                if (struct_var == null)
                    ResolveContext.Report.Error(18, Location, "Undefined access reference " + lv.Name);
                if (!ok)
                    ResolveContext.Report.Error(19, Location, "Unresolved member access " + lv.Name + "." + rv.Name);

            }

            else ResolveContext.Report.Error(20, Location, "'->' operator accepts only variable expressions at both sides (left,right)");

            throw new Exception("Failed to create variable");
        }

    }


}
