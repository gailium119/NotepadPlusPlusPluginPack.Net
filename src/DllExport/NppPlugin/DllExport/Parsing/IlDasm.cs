using System;
using System.Globalization;
using System.IO;
using System.Security.Permissions;

namespace NppPlugin.DllExport.Parsing
{
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public sealed class IlDasm : IlToolBase
	{
		public IlDasm(IServiceProvider serviceProvider, IInputValues inputValues)
			: base(serviceProvider, inputValues)
		{
		}

		public int Run()
		{
			return IlParser.RunIlTool(arguments: string.Format(CultureInfo.InvariantCulture, "/quoteallnames /unicode /nobar{2}\"/out:{0}.il\" \"{1}\"", Path.Combine(base.TempDirectory, base.InputValues.FileName), base.InputValues.InputFileName, base.InputValues.EmitDebugSymbols ? " /linenum " : " "), installPath: base.InputValues.SdkPath, toolFileName: "ildasm.exe", requiredPaths: null, workingDirectory: null, settingsName: "ILDasmPath", toolLoggingCode: DllExportLogginCodes.IlDasmLogging, verboseLoggingCode: DllExportLogginCodes.VerboseToolLogging, notifier: base.Notifier, timeout: base.Timeout);
		}
	}
}
