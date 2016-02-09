using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC
{
	
   public abstract class MemberSpec : IMember,IEmitter
    {
       protected MemberSignature _sig;
       protected Modifiers _mod;
       protected string _name;
       internal TypeSpec memberType;

       public  bool IsConstant { get { return ((_mod & Modifiers.Const) == Modifiers.Const); } }
       public bool IsReference { get { return ((_mod & Modifiers.Ref) == Modifiers.Ref); } }
       public TypeSpec MemberType
       {
           get
           {
               return memberType;
           }
       }


       public MemberSpec(string name, MemberSignature sig, Modifiers mod, ReferenceKind re)
      
       {
           _mod = mod;
           _name = name;
           _sig = sig;
       }


       public ElementReference Reference { get; set; }
       /// <summary>
       /// Member name
       /// </summary>
       public virtual string Name
       {
           get { return _name; }
       }
       public Namespace NS { get; set; }
       /// <summary>
       /// Member Signature for struct_node_
       /// </summary>
       public MemberSignature Signature
       {
           get { return _sig; }
           protected set
           {
               _sig = value;
           }

       }
        /// <summary>
        /// Member Modifiers
        /// </summary>
       public Modifiers Modifiers
       {
           get { return _mod; }
           set { _mod = value; }

       }

       public bool IsPrivate
       {
           get { return (_mod & Modifiers.Private) != 0; }
       }
       public bool IsExtern
       {
           get { return (_mod & Modifiers.Extern) != 0; }
       }
       public bool IsStatic
       {
           get
           {
               return (_mod & Modifiers.Static) != 0;
           }
       }



       public virtual bool LoadEffectiveAddress(EmitContext ec)
       {
           return true;
       }
       public virtual bool ValueOf(EmitContext ec)
       {
           return true;
       }
       public virtual bool ValueOfStack(EmitContext ec)
       {
           return true;
       }

       public virtual bool ValueOfAccess(EmitContext ec, int off, TypeSpec mem)
       {
           return true;
       }
       public virtual bool ValueOfStackAccess(EmitContext ec, int off, TypeSpec mem)
       {
           return true;
       }
       public virtual bool EmitToStack(EmitContext ec) { return true; }
       public virtual bool EmitFromStack(EmitContext ec) { return true; }
    
    }

   
	
	
}