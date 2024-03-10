using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport
{
	public static class Utilities
	{
		public static readonly string DllExportAttributeAssemblyName = "NppPlugin.DllExport.Metadata";

		public static readonly string DllExportAttributeFullName = "NppPlugin.DllExport.DllExportAttribute";

		public static MethodInfo GetMethodInfo<TResult>(Expression<Func<TResult>> expression)
		{
			return ((MethodCallExpression)expression.Body).Method;
		}

		public static string GetSdkPath(Version frameworkVersion)
		{
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework", false))
			{
				if (registryKey == null)
				{
					return null;
				}
				string text = registryKey.GetValue("sdkInstallRootv" + frameworkVersion.ToString(2), "").NullSafeToString();
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
				return null;
			}
		}

		public static string GetSdkPath()
		{
			return GetSdkPath(new Version(RuntimeEnvironment.GetSystemVersion().NullSafeTrimStart('v')));
		}

		internal static IExportAssemblyInspector CreateAssemblyInspector(IInputValues inputValues)
		{
			return new ExportAssemblyInspector(inputValues);
		}

		public static int GetCoreFlagsForPlatform(CpuPlatform cpu)
		{
			if (cpu != CpuPlatform.X86)
			{
				return 0;
			}
			return 2;
		}


        public static CpuPlatform ToCpuPlatform(string platformTarget)
        {
            if (!string.IsNullOrEmpty(platformTarget))
            {
                switch (platformTarget.NullSafeToLowerInvariant())
                {
                    case "anycpu":
                    case "any cpu":
                        return CpuPlatform.AnyCpu;

                    case "x86":
                    case "x32":
                    case "win32":
                        return CpuPlatform.X86;

                    case "x64":
                        return CpuPlatform.X64;

                    case "ia64":
                        return CpuPlatform.Itanium;
                    case "arm":
                    case "arm32":
                    case "armv7":
                        return CpuPlatform.ARM;
                    case "arm64":
                    case "armv8":
                        return CpuPlatform.ARM64;

                }
            }
            throw new ArgumentException(string.Format(Resources.Unknown_cpu_platform_0_, (object)platformTarget), "platformTarget");
        }

        public static T TryInitialize<T>(this T instance, Action<T> call) where T : IDisposable
		{
			try
			{
				call(instance);
				return instance;
			}
			catch (Exception)
			{
				instance.Dispose();
				throw;
			}
		}

		public static ValueDisposable<string> CreateTempDirectory()
		{
			return new ValueDisposable<string>(CreateTempDirectoryCore(), delegate(string dir)
			{
				Directory.Delete(dir, true);
			});
		}

		private static string CreateTempDirectoryCore()
		{
			string text = null;
			try
			{
				string tempFileName = Path.GetTempFileName();
				if (!string.IsNullOrEmpty(tempFileName) && File.Exists(tempFileName))
				{
					File.Delete(tempFileName);
				}
				string text2 = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(tempFileName)), Path.GetFileNameWithoutExtension(tempFileName));
				Directory.CreateDirectory(text2);
				text = text2;
				return text;
			}
			catch
			{
				if (!string.IsNullOrEmpty(text) && Directory.Exists(text))
				{
					Directory.Delete(text, true);
				}
				throw;
			}
		}
	}
}
