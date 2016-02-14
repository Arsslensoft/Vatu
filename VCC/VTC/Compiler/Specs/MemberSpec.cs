using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
   /// <summary>
    /// Method Specs
    /// </summary>
    public class MethodSpec : MemberSpec, IEquatable<MethodSpec>
    {
        public ClassTypeSpec ParentClass { get; set; }
        public bool IsVariadic { get; set; }
        public CallingConventions CallingConvention { get; set; }
        public List<ParameterSpec> Parameters { get; set; }
        public bool IsOperator { get; set; }

        public ushort VSCInterrupt { get; set; }
        public ushort VSCDescriptor { get; set; }


        public bool IsPrototype
        {
            get
            {
                return (Modifiers & Modifiers.Prototype) == Modifiers.Prototype;
            }
        }

        public MethodSpec(Namespace ns, string name, Modifiers mods, TypeSpec type,CallingConventions ccv ,TypeSpec[] param,Location loc)
            : base(name, new MemberSignature(ns, name, param,loc), mods ,ReferenceKind.Method)
        {
            IsOperator = false;
            IsVariadic = false;
            NS = ns;
            CallingConvention =ccv;
            memberType = type;
            Parameters = new List<ParameterSpec>();
            VSCInterrupt = 0;
            VSCDescriptor = 0;
        }
        public override string ToString()
        {
            return Signature.ToString();
        }

        public bool Equals(MethodSpec tp)
        {
            return tp.Signature == Signature;
        }

        public bool MatchSignature(MemberSignature msig, string name, TypeSpec[] param,ref bool hastemplate)
        {hastemplate=false;

        if (msig == Signature)
            return true;
        else if (IsVariadic && msig.NoNamespaceSignature.StartsWith(Signature.NoNamespaceSignature))
            return true;
        else if (Name == name)
        {
            if (param == null)
                return true;
            else
            {
                if (param.Length != Parameters.Count)
                    return false;


                // templates match
                Dictionary<char, TypeSpec> templatesdef = new Dictionary<char, TypeSpec>();
                foreach (ParameterSpec p in Parameters)
                    if (p.memberType is TemplateTypeSpec && !templatesdef.ContainsKey((p.memberType as TemplateTypeSpec).Template))
                        templatesdef.Add((p.memberType as TemplateTypeSpec).Template, null);

                if (templatesdef.Count > 0)
                {
                    hastemplate = true;
                   for(int i =0; i < param.Length;i++)
                   {
                       if (Parameters[i].memberType is TemplateTypeSpec && templatesdef[(Parameters[i].memberType as TemplateTypeSpec).Template] == null)
                           templatesdef[(Parameters[i].memberType as TemplateTypeSpec).Template] = param[i];
                       else if ( Parameters[i].memberType is TemplateTypeSpec && templatesdef[(Parameters[i].memberType as TemplateTypeSpec).Template] != param[i])
                           return false;
                       else if (!(Parameters[i].memberType is TemplateTypeSpec) && !Parameters[i].memberType.Equals(param[i]))
                           return false;
                   }
                   return true;
                }
                else return false;

            }
        }
        else return false;
        }
        public bool MatchExtSignature(MemberSignature msig, string name, TypeSpec[] param, ref bool hastemplate)
        {
            hastemplate = false;

            if (msig.ExtensionSignature == Signature.ExtensionSignature)
                return true;
            else if (IsVariadic && msig.NoNamespaceSignature.StartsWith(Signature.NoNamespaceSignature))
                return true;
            else if (Name == name)
            {
                if (param == null)
                    return true;
                else
                {
                    if (param.Length != Parameters.Count)
                        return false;


                    // templates match
                    Dictionary<char, TypeSpec> templatesdef = new Dictionary<char, TypeSpec>();
                    foreach (ParameterSpec p in Parameters)
                        if (p.memberType is TemplateTypeSpec && !templatesdef.ContainsKey((p.memberType as TemplateTypeSpec).Template))
                            templatesdef.Add((p.memberType as TemplateTypeSpec).Template, null);

                    if (templatesdef.Count > 0)
                    {
                        hastemplate = true;
                        for (int i = 0; i < param.Length; i++)
                        {
                            if (Parameters[i].memberType is TemplateTypeSpec && templatesdef[(Parameters[i].memberType as TemplateTypeSpec).Template] == null)
                                templatesdef[(Parameters[i].memberType as TemplateTypeSpec).Template] = param[i];
                            else if (Parameters[i].memberType is TemplateTypeSpec && templatesdef[(Parameters[i].memberType as TemplateTypeSpec).Template] != param[i])
                                return false;
                            else if (!(Parameters[i].memberType is TemplateTypeSpec) && !Parameters[i].memberType.Equals(param[i]))
                                return false;
                        }
                        return true;
                    }
                    else return false;

                }
            }
            else return false;
        }
    }
   
   
	
	
}