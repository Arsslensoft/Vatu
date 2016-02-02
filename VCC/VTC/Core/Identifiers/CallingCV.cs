using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VTC.Core
{
	 public class CallingCV : SimpleToken
    {
        public CallingConventions CallingConvention { get; set; }
        public ushort Interrupt { get; set; }
        public ushort Descriptor { get; set; }
   
        protected SimpleToken _mod;
        [Rule(@"<CallCV>      ::= stdcall")]
        [Rule(@"<CallCV>      ::= fastcall")]
        [Rule(@"<CallCV>      ::= cdecl")]
        [Rule(@"<CallCV>      ::= default")]
        [Rule(@"<CallCV>      ::= pascal")]
        public CallingCV(SimpleToken mod)
        {
            _mod = mod;

        }

       [Rule(@"<CallCV>      ::= vsyscall ~'(' <Integral Const> ~',' <Integral Const> ~')'")]
        public CallingCV(SimpleToken mod,Literal inter, Literal desc)
        {
            _mod = mod;
            Descriptor = ushort.Parse(desc.Value.GetValue().ToString());
            Interrupt = ushort.Parse(inter.Value.GetValue().ToString());
        }
       public override bool Resolve(ResolveContext rc)
        {
            return base.Resolve(rc);
        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
           if (_mod.Name == "fastcall")
               CallingConvention = CallingConventions.FastCall;
            else if (_mod.Name == "cdecl")
               CallingConvention = CallingConventions.Cdecl;
           else if (_mod.Name == "default")
               CallingConvention = CallingConventions.Default;
           else if (_mod.Name == "pascal")
               CallingConvention = CallingConventions.Pascal;
           else if (_mod.Name == "vsyscall")
               CallingConvention = CallingConventions.VatuSysCall;
           else CallingConvention = CallingConventions.StdCall;
            return this;
        }
      
    }
   
	
	
	
}