using System;

namespace NppPlugin.DllExport
{
	[CLSCompliant(true)]
	public enum CpuPlatform
	{
		None,
		X86,
		X64,
		ARM,
		ARM64,
		Itanium,
		AnyCpu
	}
}
