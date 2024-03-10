using System;
using Microsoft.Build.Utilities;

namespace NppPlugin.DllExport.MSBuild
{
	public interface IDllExportTask : IInputValues, IServiceProvider
	{
		TaskLoggingHelper Log { get; }

		bool? SkipOnAnyCpu { get; set; }

		string TargetFrameworkVersion { get; set; }

		string Platform { get; set; }

		string PlatformTarget { get; set; }
	}
}
