using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vasm
{
    public class StructVar
    {
        public string Name { get; set; }
        public bool IsByte { get; set; }
        public int Size { get; set; }

        public bool IsStruct { get; set; }
        public string Type { get; set; }
        public void Emit(AssemblyWriter asmw)
        {
          if(!IsStruct)
            asmw.WriteLine("."+Name + ":    "+(IsByte?"RESB":"RESW") + "    "+Size.ToString());
        


        }
    }
    public class StructElement
    {
        public string Name { get; set; }
        public List<StructVar> Vars { get; set; }

        public StructElement()
        { Vars = new List<StructVar>(); }
        public void Emit(AssemblyWriter asmw)
        {
            asmw.WriteLine("struc   " + Name);
            foreach (StructVar sv in Vars)
                sv.Emit(asmw);
            asmw.WriteLine(".size:");
            asmw.WriteLine("endstruc");
        }

        public void EmitDecl(AssemblyWriter asmw,string varname)
        {
            asmw.WriteLine(varname+": 	ISTRUC "+Name);
            foreach (StructVar sv in Vars)
                asmw.WriteLine("AT "+Name+"."+sv.Name+", "+(sv.IsByte?"DB":"DW")+" 0");
            asmw.WriteLine("IEND");
        }
    }

   
}
