namespace NppPlugin.DllExport.Parsing.Actions
{
	public sealed class ExternalAssemlyDeclaration
	{
		public int InputLineIndex { get; private set; }

		public string AssemblyName { get; private set; }

		public string AliasName { get; private set; }

		public ExternalAssemlyDeclaration(int inputLineIndex, string assemblyName, string aliasName)
		{
			InputLineIndex = inputLineIndex;
			AssemblyName = assemblyName;
			AliasName = aliasName;
		}
	}
}
