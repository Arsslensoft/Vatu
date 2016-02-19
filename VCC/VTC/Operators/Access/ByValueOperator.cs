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
            rc.CreateNewState();

            rc.CurrentExtensionLookup = Left.Type;
            rc.StaticExtensionLookup = false;
            rc.ExtensionVar = Left;
            if((Right is VariableExpression) || (Right is DeclaredExpression && (Right as DeclaredExpression).Expression is VariableExpression))
                rc.CurrentGlobalScope |= ResolveScopes.VariableExtensionAccess;
            else 
                rc.CurrentGlobalScope |= ResolveScopes.MethodExtensionAccess;

            Right = (Expr)Right.DoResolve(rc);
            if (Right is VariableExpression && (Right as VariableExpression).variable == null)
            {

                ResolveContext.Report.Error(0, Location, "Unresolved extended field");
                rc.RestoreOldState();
                return false;
            }
            else if (Right is MethodExpression && (Right as MethodExpression).Method == null)
            {
            
            rc.RestoreOldState();
                return false;
            }
            rc.RestoreOldState();
            return true;
        }

        TypeMemberSpec tmp = null;
        public bool ResolveStructOrUnionMember(ResolveContext rc, TypeSpec mem, ref TypeMemberSpec tmp, VariableExpression rv)
        {

            if (!mem.IsPointer)
            {
                if (!mem.IsForeignType)
                    return false;
                else if (mem.IsStruct)
                {
                    StructTypeSpec stp = (StructTypeSpec)mem;

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
                else if (mem.IsUnion)
                {
                    UnionTypeSpec stp = (UnionTypeSpec)mem;


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
                else return false;
            }
            else return false;


        }
        public bool ResolveClassMember(ResolveContext rc, TypeSpec mem, ref TypeMemberSpec tmp, VariableExpression rv)
        {

            if (!mem.IsPointer)
            {
                if (!mem.IsClass)
                    return false;
                else 
                {
                    ClassTypeSpec stp = (ClassTypeSpec)mem;
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
                else if (Left.Type.Size == 4 && Left.Type.IsReference)
                    return new WordAccessExpression(lv.variable, Right);
                else ResolveContext.Report.Error(0, Location, "Cannot perform (HIGH, LOW) or (SEGMENT, POINTER) operations on non 16 bits , reference types");

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
                        ok = ResolveStructOrUnionMember(rc, struct_var.memberType, ref tmp,  rv);

                        // class member
                        if (!ok)
                        {
                            ok = ResolveClassMember(rc, struct_var.memberType, ref tmp, rv);
                            if (ok)
                            {
                                this.CommonType = tmp.MemberType;
                                if (struct_var is VarSpec)
                                {
                                    VarSpec dst = (VarSpec)struct_var;
                                    return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);

                                }
                                else if (struct_var is RegisterSpec)
                                {
                                    RegisterSpec dst = (RegisterSpec)struct_var;
                                    return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, index, tmp.MemberType);

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
                            }
                        }
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

                            VarSpec dst = new VarSpec(v.NS, v.Name, v.MethodHost, (v.memberType.IsReference) ? tmp.MemberType.MakeReference() : tmp.MemberType, Location, v.FlowIndex, v.Modifiers, true);
                            dst.VariableStackIndex = v.VariableStackIndex + index;
                            dst.InitialStackIndex = v.InitialStackIndex;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);

                        }
                        else if (struct_var is RegisterSpec)
                        {
                            RegisterSpec v = (RegisterSpec)struct_var;

                            RegisterSpec dst = new RegisterSpec((v.memberType.IsReference) ? tmp.MemberType.MakeReference() : tmp.MemberType, v.Register, Location, 0, true);
                            dst.RegisterIndex = v.RegisterIndex + index;
                            dst.InitialRegisterIndex = v.InitialRegisterIndex;
                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);

                        }
     
                        else if (struct_var is FieldSpec)
                        {
                            FieldSpec v = (FieldSpec)struct_var;

                            FieldSpec dst = new FieldSpec(v.NS, v.Name, v.Modifiers, (v.memberType.IsReference) ? tmp.MemberType.MakeReference() : tmp.MemberType, Location, true);
                            dst.FieldOffset = v.FieldOffset + index;
                            dst.InitialFieldIndex = v.InitialFieldIndex;

                            return new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, Left.position);
                        }
                        else if (struct_var is ParameterSpec)
                        {
                            ParameterSpec v = (ParameterSpec)struct_var;
                           
                            ParameterSpec dst = new ParameterSpec(v.NS, v.Name, v.MethodHost, (v.memberType.IsReference) ? tmp.MemberType.MakeReference() : tmp.MemberType, Location, v.InitialStackIndex, v.Modifiers, true);

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
            else if (IsLeftValueOf(rc) && Right is VariableExpression)
            {
                VariableExpression rv = (VariableExpression)Right;
                Expr valueofexpr = ((Left as UnaryOperation)._op as ValueOfOp).Right;
                bool  ok = ResolveStructOrUnionMember(rc, Left.Type, ref tmp,  rv);
                AccessExpression vae = new AccessExpression(true, valueofexpr, null, Left.position); // value of 
                        // class member
                if (!ok)
                {
                    ok = ResolveClassMember(rc, Left.Type, ref tmp, rv);
                    if (ok)
                    {
                        this.CommonType = tmp.MemberType;

                        AccessExpression.SetNext();
                        RegisterSpec dst = new RegisterSpec((Left.Type.IsReference) ? tmp.MemberType.MakeReference() : tmp.MemberType, AccessExpression.LastUsed, Location, 0, true);
                        dst.RegisterIndex = index;
                        dst.InitialRegisterIndex = 0;
                        return new AccessExpression(dst, vae, Left.position, true, index, tmp.MemberType);
                    }
       
                }
                else
                {
                    AccessExpression.SetNext();
                    RegisterSpec dst = new RegisterSpec((Left.Type.IsReference) ? tmp.MemberType.MakeReference() : tmp.MemberType, AccessExpression.LastUsed, Location, 0, true);
                    dst.RegisterIndex =index;
                    dst.InitialRegisterIndex = 0;
                    return new AccessExpression(dst, vae, Left.position);

                   
                }

        


            }
            else ResolveContext.Report.Error(20, Location, "'.' operator accepts only variable expressions at both sides (left,right)");

            throw new Exception("Failed to create variable");
        }


        public bool IsLeftValueOf(ResolveContext rc)
        {
            return Left is UnaryOperation && (Left as UnaryOperation)._op is ValueOfOp;
        }

        
        //public bool IsLeftValueOfHighDegree(ResolveContext rc)
        //{
        //    return Left is UnaryOperation && (Left as UnaryOperation)._op is ValueOfOp;
        //}
    }
}
