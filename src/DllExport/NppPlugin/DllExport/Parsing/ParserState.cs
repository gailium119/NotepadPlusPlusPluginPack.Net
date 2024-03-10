namespace NppPlugin.DllExport.Parsing
{
	public enum ParserState
	{
		Normal,
		ClassDeclaration,
		Class,
		DeleteExportDependency,
		MethodDeclaration,
		MethodProperties,
		Method,
		DeleteExportAttribute
	}
}
