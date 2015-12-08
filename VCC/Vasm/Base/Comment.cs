using System;
using System.Linq;

namespace Vasm
{
    public class Comment : Instruction
    {
        public readonly string Text;

        public Comment( AsmContext aAssembler, string aText )
            : base() //HACK
        {
          if (aText.StartsWith(";")) {
            aText = aText.TrimStart(';').TrimStart();
          }
          Text = String.Intern(aText);
        }
      
        public override void WriteText(AsmContext aAssembler, AssemblyWriter aOutput)
        {
            aOutput.Write( "; " );
            aOutput.Write( Text );
        }

        public override void UpdateAddress( AsmContext aAssembler, ref ulong aAddress )
        {
            base.UpdateAddress( aAssembler, ref aAddress );
        }

        public override bool IsComplete(AsmContext aAssembler)
        {
            return true;
        }
    }
}
