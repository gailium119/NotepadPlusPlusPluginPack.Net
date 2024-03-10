using System.IO;
using Mono.Cecil;

namespace NppPlugin.DllExport
{
	internal interface IExportAssemblyInspector
	{
		IInputValues InputValues { get; }

		AssemblyExports ExtractExports();

		AssemblyExports ExtractExports(AssemblyDefinition assemblyDefinition);

		AssemblyExports ExtractExports(string fileName);

		AssemblyExports ExtractExports(AssemblyDefinition assemblyDefinition, ExtractExportHandler exportFilter);

		AssemblyExports ExtractExports(string fileName, ExtractExportHandler exportFilter);

		AssemblyBinaryProperties GetAssemblyBinaryProperties(string assemblyFileName);

		AssemblyDefinition LoadAssembly(string fileName);

		bool SafeExtractExports(string fileName, Stream stream);
	}
}
