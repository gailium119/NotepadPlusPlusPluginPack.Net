using System.Runtime.InteropServices;

namespace NppPlugin.DllExport
{
	public interface IExportInfo
	{
		CallingConvention CallingConvention { get; set; }

		string ExportName { get; set; }

		bool IsStatic { get; }

		bool IsGeneric { get; }

		void AssignFrom(IExportInfo info);
	}
}
