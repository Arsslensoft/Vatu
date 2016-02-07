using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm.x86;
using VTC.Core;

namespace VTC
{
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
            rc.Resolver.TryResolveType(lv.Name, ref tp);
            if (tp != null && tp.IsEnum)
            {

                enumval = rc.Resolver.TryResolveEnumValue(rv.Name);
                return true;
            }
            return false;

        }
        public bool ResolveExtension(ResolveContext rc)
        {
            // back up 
            TypeSpec oldext = rc.CurrentExtensionLookup;
            Expr extvar = rc.ExtensionVar;
            rc.ResolverStack.Push(new ResolveState(rc.CurrentNamespace, rc.CurrentExtensionLookup, rc.StaticExtensionLookup));


            rc.CurrentExtensionLookup = Left.Type;
            rc.StaticExtensionLookup = false;
            rc.ExtensionVar = Left;

            Right = (Expr)Right.DoResolve(rc);
            if (Right is VariableExpression && (Right as VariableExpression).variable == null)
            {

                ResolveContext.Report.Error(0, Location, "Unresolved extended field");
                rc.ResolverStack.Pop();
                rc.ExtensionVar = extvar;
                rc.CurrentExtensionLookup = oldext;
                return false;
            }
            else if (Right is MethodExpression && (Right as MethodExpression).Method == null)
            {
                rc.ResolverStack.Pop();
                rc.ExtensionVar = extvar;
                rc.CurrentExtensionLookup = oldext;
                return false;
            }
            rc.ResolverStack.Pop();
            rc.ExtensionVar = extvar;
            rc.CurrentExtensionLookup = oldext;


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


        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            // Check if left is type
            if (Left is VariableExpression && Right is ValuePosIdentifier)
            {
                VariableExpression lv = (VariableExpression)Left;
                if (Left.Type.Size == 2)
                    return new ByteAccessExpression(lv.variable, Right);
                else ResolveContext.Report.Error(0, Location, "Cannot perform HIGH or LOW operation on non 16 bits types");

            }
            else if (Left is VariableExpression && Right is MethodExpression)
            {
                if (!ResolveExtension(rc))
                    ResolveContext.Report.Error(0, Location, "Unresolved extended method");
                else return Right;

            }
            else if (Left is VariableExpression && Right is VariableExpression)
            {


                VariableExpression lv = (VariableExpression)Left;
                VariableExpression rv = (VariableExpression)Right;
                // enum
                bool ok = ResolveEnumValue(rc, lv, rv);
                if (ok)
                    return new AccessExpression(enumval, null, Left.position);

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

                            VarSpec dst = new VarSpec(v.NS, v.Name, v.MethodHost, tmp.MemberType, Location,v.FlowIndex, v.Modifiers,true);
                            dst.StackIdx = v.StackIdx + index;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);

                        }
                        else if (struct_var is RegisterSpec)
                        {
                            RegisterSpec v = (RegisterSpec)struct_var;

                            RegisterSpec dst = new RegisterSpec(tmp.MemberType, v.Register, Location, 0,true);
                            dst.RegisterIndex = v.RegisterIndex + index;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);

                        }
                        else if (struct_var is FieldSpec)
                        {
                            FieldSpec v = (FieldSpec)struct_var;

                            FieldSpec dst = new FieldSpec(v.NS, v.Name, v.Modifiers, tmp.MemberType, Location,true);
                            dst.FieldOffset = v.FieldOffset + index;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);
                        }
                        else if (struct_var is ParameterSpec)
                        {
                            ParameterSpec v = (ParameterSpec)struct_var;

                            ParameterSpec dst = new ParameterSpec(v.Name, v.MethodHost, tmp.MemberType, Location, v.InitialStackIndex, v.Modifiers,true);

                            dst.StackIdx = v.StackIdx + index;

                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);
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
}
