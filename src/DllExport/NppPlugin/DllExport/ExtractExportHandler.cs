using Mono.Cecil;

namespace NppPlugin.DllExport
{
	internal delegate bool ExtractExportHandler(MethodDefinition memberInfo, out IExportInfo exportInfo);
}
