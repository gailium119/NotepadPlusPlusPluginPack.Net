namespace NppPlugin.DllExport
{
	public interface IInputValues
	{
		CpuPlatform Cpu { get; set; }

		bool EmitDebugSymbols { get; set; }

		string LeaveIntermediateFiles { get; set; }

		string FileName { get; set; }

		string FrameworkPath { get; set; }

		string InputFileName { get; set; }

		string KeyContainer { get; set; }

		string KeyFile { get; set; }

		string OutputFileName { get; set; }

		string RootDirectory { get; set; }

		string SdkPath { get; set; }

		string MethodAttributes { get; set; }

		string LibToolPath { get; set; }

		string DllExportAttributeFullName { get; set; }

		string DllExportAttributeAssemblyName { get; set; }

		string LibToolDllPath { get; set; }

		AssemblyBinaryProperties InferAssemblyBinaryProperties();

		void InferOutputFile();
	}
}
