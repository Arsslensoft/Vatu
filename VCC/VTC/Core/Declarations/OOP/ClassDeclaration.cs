using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC.Core
{
    public class ClassDeclaration : Declaration
    {
        public bool AddDefaultCtor = true;
        public bool AddDefaultDtor = true;
        public ClassTypeSpec TypeName { get; set; }
        public int Size { get; set; }
        ClassElementSequence _def;
        InheritanceDefinition _ihd;
        Modifier _mod;
        TemplateDefinition _tdef;
        bool istypedef = false;
        [Rule(@"<Class Decl>  ::= <Mod> ~class Id <Template Def> <Inheritance> ~'{' <Class Element Decl>  ~'}' ")]
        public ClassDeclaration(Modifier mod, Identifier id, TemplateDefinition tdef, InheritanceDefinition ihd, ClassElementSequence sdef)
        {
            _tdef = tdef;
            _mod = mod;
            _name = id;
            _def = sdef;
            Size = 0;
            _ihd = ihd;
        }
   

       
       public override bool Resolve(ResolveContext rc)
        {


            return _def.Resolve(rc);
        }
       void GetMembers(List<TypeMemberSpec> Members)
       {
           foreach (VariableDeclaration _var in _def.VarDecls)
           {
           
                   if (_var != null)
                   {

                       if (_var.Members[0].MemberType is ArrayTypeSpec)
                           Size += (_var.Members[0].MemberType as ArrayTypeSpec).ArrayCount * _var.Members[0].MemberType.BaseType.Size;
                       else
                           Size += _var.Type.Size;
                   }

                   foreach (TypeMemberSpec sv in _var.Members)
                   {
                       if (!Members.Contains(sv))
                           Members.Add(sv);
                   }
               
           }

      
       }
       void GetMethods(List<MethodSpec> methods, List<TypeMemberSpec> Members, int idx)
       {
           foreach (Declaration d in _def.Declarations)
           {
               if (d is ConstructorDeclaration)
                   AddDefaultCtor = false;
               //    methods.Add((d as ConstructorDeclaration).method);
               else if (d is DestructorDeclaration)
                   AddDefaultDtor = false;
               //    methods.Add((d as DestructorDeclaration).method);
             else if (d is ConstructorPrototypeDeclaration)
                   AddDefaultCtor = false;
               //    methods.Add((d as ConstructorPrototypeDeclaration).method);
               else if (d is DestructorPrototypeDeclaration)
                      AddDefaultDtor = false;
               //    methods.Add((d as DestructorPrototypeDeclaration).method);

               if (d is MethodDeclaration)
               {
                   TypeMemberSpec t = CreateMemberForMethod((d as MethodDeclaration).method, idx);
                   if (!Members.Contains(t))
                   {
                       t.Index = idx;
                       idx += 2;
                       Members.Add(t);
                   }
                   else
                   {
                        t = CreateMemberForMethod((d as MethodDeclaration).method,  Members[  Members.IndexOf(t)].Index);
                        Members[Members.IndexOf(t)] = t;
                   }
            methods.Add((d as MethodDeclaration).method);
            methods[methods.Count - 1].ParentClass = TypeName;
          
               }
               else if (d is MethodPrototypeDeclaration)
               {
                   TypeMemberSpec t = CreateMemberForMethod((d as MethodDeclaration).method, idx);
                   if ( !Members.Contains(t))
                   {
                       t.Index = idx;
                       idx += 2;
                       Members.Add(t);
                   }
                   else
                   {
                       t = CreateMemberForMethod((d as MethodPrototypeDeclaration).method, Members[Members.IndexOf(t)].Index);
                       Members[Members.IndexOf(t)] = t;
                   }
                   methods.Add((d as MethodPrototypeDeclaration).method);
                   methods[methods.Count - 1].ParentClass = TypeName;
               
               }

            

           }
       
       }
       public DelegateTypeSpec CreateDelegateForMethod(MethodSpec ms)
       {
           List<TypeSpec> tp = new List<TypeSpec>();
           foreach (ParameterSpec p in ms.Parameters)
               tp.Add(p.memberType);
           DelegateTypeSpec dt = new DelegateTypeSpec(ms.NS, ms.Name + "_caller", ms.MemberType, tp, ms.CallingConvention, ms.Modifiers,ms.Parameters, ms.Signature.Location);

           return dt;
       }
       public TypeMemberSpec CreateMemberForMethod(MethodSpec ms,int idx)
       {
           DelegateTypeSpec dt = CreateDelegateForMethod(ms);
           string name = ms.Name.Split('$')[1].Remove(0,1);
           TypeMemberSpec t = new TypeMemberSpec(ms.NS, name, TypeName, dt, ms.Signature.Location, idx);
           t.IsMethod = true;
           t.Index = idx;
           t.DefaultMethod = ms;
           return t;
       }
 public override SimpleToken DoResolve(ResolveContext rc)
       {
           List<TypeMemberSpec> defmembers = new List<TypeMemberSpec>();
           List<MethodSpec> methods = new List<MethodSpec>();
           List<TypeMemberSpec> methods_members = new List<TypeMemberSpec>();
          List<TypeMemberSpec> members = new List<TypeMemberSpec>();
            List<TemplateTypeSpec> templates = new List<TemplateTypeSpec>();


            _mod = (Modifier)_mod.DoResolve(rc);
            if (_tdef != null && _tdef._tid != null)
            {
                _tdef = (TemplateDefinition)_tdef.DoResolve(rc);
                templates.AddRange(_tdef.Templates);
                foreach (TemplateTypeSpec tts in _tdef.Templates)
                    rc.KnowType(tts);
            }
            else _tdef = null;

            if (_ihd != null)
            {
                _ihd = (InheritanceDefinition)_ihd.DoResolve(rc);
                TypeName = new ClassTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(),_ihd.ParentClass,methods, _ihd.InheritedClass, templates, Location);
            }
            else TypeName = new ClassTypeSpec(rc.CurrentNamespace, _name.Name, new List<TypeMemberSpec>(), null, methods, new List<TypeSpec>(), templates, Location);

            rc.KnowType(TypeName);
            rc.CurrentType = TypeName;
            _def = (ClassElementSequence)_def.DoResolve(rc);
            GetMembers(defmembers);

            TypeName.Members = defmembers;
            rc.Resolver.KnownTypes[rc.Resolver.KnownTypes.IndexOf(TypeName)] = TypeName;
            
            // Type descriptor for a class
            members.Add(new TypeMemberSpec(TypeName.NS,"Type", TypeName,BuiltinTypeSpec.Type, Location,0));


            int idx = 2;
   
            // Copy Members of inherited
            if (_ihd != null)
            {
                foreach (TypeSpec st in _ihd.InheritedClass)
                {
                    if (st is StructTypeSpec)
                    {
                        foreach (TypeMemberSpec m in (st as StructTypeSpec).Members)
                        {

                            TypeMemberSpec newm = new TypeMemberSpec(TypeName.NS, m.Name, TypeName, m.memberType, Location, idx);
                            newm.Index = idx;
                            idx += newm.MemberType.GetSize(newm.MemberType);
                            if (members.Contains(newm))
                                ResolveContext.Report.Error(0, Location, "Duplicate member declaration, " + newm.Name + " is already defined in " + st.Name);

                            members.Add(newm);
                        }
                    }
                    else if (st is ClassTypeSpec)
                    {
                   
                       
                        for(int j = 1; j < (st as ClassTypeSpec).Members.Count;j++)
                          
                        {
                            TypeMemberSpec m = (st as ClassTypeSpec).Members[j];


                            TypeMemberSpec newm = new TypeMemberSpec(TypeName.NS, m.Name, TypeName, m.memberType, Location, idx);
                            newm.Index = idx;
                            idx += newm.MemberType.GetSize(newm.MemberType);
                            if (members.Contains(newm))
                                ResolveContext.Report.Error(0, Location, "Duplicate member declaration, " + newm.Name + " is already defined in " + st.Name);
                     
                              members.Add(newm);
                        }
                        foreach (MethodSpec m in (st as ClassTypeSpec).Methods)
                            methods.Add(m);
                    }
                }
            }

            // Duplicate names check

            int i = 0;
            List<int> tobeupdated = new List<int>();
            TypeSpec ts = null;
            foreach (TypeMemberSpec m in defmembers)
            {

                m.Index = idx;
                idx += m.MemberType.GetSize(m.MemberType);
                // used for recursive declarations
                m.MemberType.GetBase(m.MemberType, ref ts);
                if (ts == TypeName)
                    tobeupdated.Add(i);

                if (members.Contains(m))
                    ResolveContext.Report.Error(0, Location, "Duplicate member declaration, " + m.Name + " is already defined in " + TypeName.Name);
                
                i++;
            }
        
        
   
           


           ClassTypeSpec NewType = null;


           if (_ihd != null)
               NewType = new ClassTypeSpec(rc.CurrentNamespace, _name.Name, defmembers, _ihd.ParentClass, methods, _ihd.InheritedClass, templates, Location);
           else NewType = new ClassTypeSpec(rc.CurrentNamespace, _name.Name, defmembers, null, methods, new List<TypeSpec>(), templates, Location);

           NewType.Modifiers = _mod.ModifierList;
           foreach (int id in tobeupdated)
               defmembers[id].MemberType.MakeBase(ref defmembers[id].memberType, NewType);


           // insert inherited
           defmembers.InsertRange(0, members);
           // Update Size
           NewType.UpdateSize();
           rc.UpdateType(TypeName, NewType);

           // resolve decls for classes
           for (int j = 0; j < _def.Declarations.Count; j++)
               _def.Declarations[j] = (Declaration)_def.Declarations[j].DoResolve(rc);
           // Update Size
           NewType.UpdateSize();
           GetMethods(methods,  defmembers, NewType.ClassSize);
                // update type for last time
       
           // Update Size
           NewType.UpdateSize();
           rc.UpdateType(TypeName, NewType);


           if (templates.Count > 0)
           {
               foreach (TemplateTypeSpec tts in templates)
                   rc.Resolver.KnownTypes.Remove(tts);
           }
           if (AddDefaultCtor) ResolveContext.Report.Error(0, Location, "No constructor found for "+TypeName.NormalizedName);
           if (AddDefaultDtor) ResolveContext.Report.Error(0, Location, "No destructor found for " + TypeName.NormalizedName);
            return this;
        }
        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            return _def.DoFlowAnalysis(fc);
        }
        public override bool Emit(EmitContext ec)
        {
            foreach (Declaration d in _def.Declarations)
            {
                if (!(d is VariableDeclaration))
                    d.Emit(ec);
            }
            //ec.EmitStructDef(TypeName);

            return true;
        }
    }
}
