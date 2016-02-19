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



        public bool ResolveExtension(ResolveContext rc)
        {
            // back up 
            rc.CreateNewState();

            rc.CurrentExtensionLookup = Left.Type.BaseType;
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

        // Struct
        MemberSpec struct_var;
        int index = -1;


        TypeMemberSpec tmp = null;
        public bool ResolveStructOrUnionMember(ResolveContext rc, TypeSpec mem, ref TypeMemberSpec tmp,  VariableExpression rv)
        {

            if (mem.IsPointer)
            {
                if (!mem.IsForeignType && !mem.IsClass)
                    ResolveContext.Report.Error(15, Location, "'->' operator allowed only with struct union based types");
                else if (mem.IsStruct)
                {
                    StructTypeSpec stp = null;
                  
                        if (Left is AccessExpression && (Left as AccessExpression).IsByIndex)
                            stp = (StructTypeSpec)mem.BaseType.BaseType;
                        else stp = (StructTypeSpec)mem.BaseType;
                 

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
                else if(mem.IsUnion)
                {
                    UnionTypeSpec stp = null;
          
                        if (Left is AccessExpression && (Left as AccessExpression).IsByIndex)
                            stp = (UnionTypeSpec)mem.BaseType.BaseType;
                        else stp = (UnionTypeSpec)mem.BaseType;
                  
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
                else return false;
            }
            else ResolveContext.Report.Error(17, Location, "Cannot use '->' operator with non pointer types use '.' instead");


            return false;
        }
        public bool ResolveClassMember(ResolveContext rc, TypeSpec mem, ref TypeMemberSpec tmp, VariableExpression rv)
        {

            if (mem.IsPointer)
            {
                if (!mem.IsClass && !mem.IsForeignType)
                    ResolveContext.Report.Error(15, Location, "'->' operator allowed only with class based types");
                else
                {
                    ClassTypeSpec stp = null;
               
                        if (Left is AccessExpression && (Left as AccessExpression).IsByIndex)
                            stp = (ClassTypeSpec)mem.BaseType.BaseType;
                       else stp = (ClassTypeSpec)mem.BaseType;
                    
                    tmp = stp.ResolveMember(rv.Name);
                    if (tmp == null)
                        ResolveContext.Report.Error(16, Location, rv.Name + " is not defined in class " + stp.Name);
                    else
                    {
                        // Resolve
                        index = tmp.Index;
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
            if (Left is VariableExpression && Right is MethodExpression)
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
                bool ok = false;


                // struct member
                if (!ok)
                {
                    struct_var = lv.variable;
                    if (struct_var != null)
                    {
                        ok = ResolveStructOrUnionMember(rc, struct_var.memberType, ref tmp,  rv);

                        // class member
                        if(!ok)
                        {
                            ok = ResolveClassMember(rc, struct_var.memberType, ref tmp, rv);
                            if (ok)
                            {
                                CommonType = tmp.MemberType;
                                if (struct_var is VarSpec)
                                {
                                    VarSpec dst = (VarSpec)struct_var;
                                    AccessExpression classind = new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, 0, Left.Type);
                                    AccessExpression atmp = new AccessExpression(classind.variable, classind, lv.position, true, index, tmp.MemberType);
                      
                                    return atmp;
                                }
                                else if (struct_var is RegisterSpec)
                                {
                                    RegisterSpec dst = (RegisterSpec)struct_var;
                                    AccessExpression classind = new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, 0, Left.Type);
                                    AccessExpression atmp = new AccessExpression(classind.variable, classind, lv.position, true, index, tmp.MemberType);

                                    return atmp;
                                }
                                else if (struct_var is FieldSpec)
                                {
                                    FieldSpec dst = (FieldSpec)struct_var;


                                    AccessExpression classind = new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, 0, Left.Type);
                                    AccessExpression atmp = new AccessExpression(classind.variable, classind, lv.position, true, index, tmp.MemberType);

                                    return atmp;
                                }
                                else if (struct_var is ParameterSpec)
                                {
                                    ParameterSpec dst = (ParameterSpec)struct_var;

                                    AccessExpression classind = new AccessExpression(dst, (Left is AccessExpression) ? (Left as AccessExpression) : null, lv.position, true, 0, Left.Type);
                                    AccessExpression atmp = new AccessExpression(classind.variable, classind, lv.position, true, index, tmp.MemberType);

                                    return atmp;
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
            else if (IsLeftValueOf(rc) && Right is VariableExpression)
            {
                VariableExpression rv = (VariableExpression)Right;
                Expr valueofexpr = ((Left as UnaryOperation)._op as ValueOfOp).Right;
                bool ok = ResolveStructOrUnionMember(rc, Left.Type, ref tmp, rv);
                AccessExpression vae = new AccessExpression(true, valueofexpr, null, Left.position); // value of 
                // class member
                if (!ok)
                {
                    ok = ResolveClassMember(rc, Left.Type, ref tmp, rv);
                    if (ok)
                    {
                        this.CommonType = tmp.MemberType;

                        AccessExpression.SetNext();
                        RegisterSpec dst = new RegisterSpec((Left.Type.BaseType.IsReference) ? Left.Type.BaseType.MakeReference() : Left.Type.BaseType, AccessExpression.LastUsed, Location, 0, true);
                        dst.RegisterIndex = 0;
                        dst.InitialRegisterIndex = 0;
                        AccessExpression classind = new AccessExpression(dst, vae, Left.position, true, 0, tmp.MemberType);

                    
                        AccessExpression atmp = new AccessExpression(classind.variable, classind, Left.position, true, index, tmp.MemberType);

                        return atmp;
                    }

                }
                else
                {
                    AccessExpression.SetNext();
                    RegisterSpec dst = new RegisterSpec((Left.Type.IsReference) ? tmp.MemberType.MakeReference() : tmp.MemberType, AccessExpression.LastUsed, Location, 0, true);
                    dst.RegisterIndex = index;
                    dst.InitialRegisterIndex = 0;
                    return new AccessExpression(dst, vae, Left.position, true, index, tmp.MemberType);


                }

            }
            else ResolveContext.Report.Error(20, Location, "'->' operator accepts only variable expressions at both sides (left,right)");

            throw new Exception("Failed to create variable");
        }
        public bool IsLeftValueOf(ResolveContext rc)
        {
            return Left is UnaryOperation && (Left as UnaryOperation)._op is ValueOfOp;
        }

    }


}
