namespace ELFSharp.ELF.Sections
{
	public enum SpecialSectionIndex : ushort
    {
        Undefined = 0,
        LORESERVE = 0xFF00,
        HIPROC = 0xFF1F,
        AFTER = 0xff01,
		Absolute = 0xFFF1,
		Common = 0xFFF2,
		HIRESERVE = 0xFFFF,
        Defined = 1
      
     
      
	}
}