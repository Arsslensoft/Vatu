using System;
using System.Linq;

namespace Vasm
{
    public class Comment : Instruction
    {
        public readonly string Text;

        public Comment( EmitContext aAssembler, string aText )
            : base() //HACK
        {
          if (aText.StartsWith(";")) {
            aText = aText.TrimStart(';').TrimStart();
          }
          Text = String.Intern(aText);
        }
      
        public override void WriteText(EmitContext aAssembler, AssemblyWriter aOutput)
        {
            aOutput.Write( "; " );
            aOutput.Write( Text );
        }

        public override void UpdateAddress( EmitContext aAssembler, ref ulong aAddress )
        {
            base.UpdateAddress( aAssembler, ref aAddress );
        }

        public override bool IsComplete(EmitContext aAssembler)
        {
            return true;
        }
    }
}
