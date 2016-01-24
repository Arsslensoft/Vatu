namespace ELFSharp.ELF.Sections
{
	public enum SymbolType
	{
		NotSpecified=0,
		Object=1,
		Function=2,
		Section=3,
		File=4,
		ProcessorSpecific = 13,
        HPROC = 15
	}
}