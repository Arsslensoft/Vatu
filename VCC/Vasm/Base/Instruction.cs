using System;
using System.Collections.Generic;
using System.IO;

namespace Vasm {
  public abstract class Instruction : BaseAssemblerElement {
      /// <summary>
      /// Cache for the default mnemonics.
      /// </summary>
      public static Dictionary<Type, string> defaultMnemonicsCache = new Dictionary<Type,string>();
      public bool Emit = true;
    protected string mMnemonic;
    public string Mnemonic {
      get { return mMnemonic; }
    }

    protected int mMethodID;
    public int MethodID {
      get { return mMethodID; }
    }

    protected int mAsmMethodIdx;
    public int AsmMethodIdx {
      get { return mAsmMethodIdx; }
    }

    public override void WriteText(AsmContext ec, AssemblyWriter aOutput)
    {
      aOutput.Write(mMnemonic);
    }

   

    protected Instruction( string mnemonic=null) {
      
        mMnemonic = mnemonic;
        if (mMnemonic == null)
        {
            var type = GetType();
            mMnemonic = GetDefaultMnemonic(type);
        }
    }

        /// <summary>
        /// Gets default operation mnemonic for given type.
        /// </summary>
        /// <param name="type">Type for which gets default mnemonics.</param>
        /// <returns>Default mnemonics for the type.</returns>
        private static string GetDefaultMnemonic(Type type)
        {
            string xMnemonic;
            if (defaultMnemonicsCache.TryGetValue(type, out xMnemonic))
            {
                return xMnemonic;
            }

            var xAttribs = type.GetCustomAttributes(typeof(OpCodeAttribute), false);
            if (xAttribs != null && xAttribs.Length > 0)
            {
                var xAttrib = (OpCodeAttribute)xAttribs[0];
                xMnemonic = xAttrib.Mnemonic;
            }
            else
            {
                xMnemonic = string.Empty;
            }

            defaultMnemonicsCache.Add(type, xMnemonic);
            return xMnemonic;
        }

    public override ulong? ActualAddress {
      get {
        // TODO: for now, we dont have any data alignment
        return StartAddress;
      }
    }

    public override void UpdateAddress(AsmContext ec, ref ulong aAddress)
    {
      base.UpdateAddress(ec, ref aAddress);
    }

    public override bool IsComplete( AsmContext aAssembler) {
      throw new NotImplementedException("Method not implemented for instruction " + this.GetType().FullName.Substring(typeof(Instruction).Namespace.Length + 1));
    }

    public override void WriteData(AsmContext aAssembler, Stream aOutput)
    {
      throw new NotImplementedException("Method not implemented for instruction " + this.GetType().FullName.Substring(typeof(Instruction).Namespace.Length + 1));
    }
  }
}