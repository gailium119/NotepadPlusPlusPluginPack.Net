using System;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using NppPlugin.DllExport.Parsing;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport
{
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public sealed class DllExportWeaver : HasServiceProvider
	{
		private int _Timeout = 45000;

		public int Timeout
		{
			get
			{
				return _Timeout;
			}
			set
			{
				_Timeout = value;
			}
		}

		internal AssemblyExports Exports { get; set; }

		public IInputValues InputValues { get; set; }

		public DllExportWeaver(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public void Run()
		{
			if (Exports == null)
			{
				IExportAssemblyInspector exportAssemblyInspector = Utilities.CreateAssemblyInspector(InputValues);
				using (GetNotifier().CreateContextName(this, Resources.ExtractExportsContextName))
				{
					Exports = exportAssemblyInspector.ExtractExports();
				}
				using (GetNotifier().CreateContextName(this, Resources.FindDuplicateExportMethodsContextName))
				{
					foreach (DuplicateExports duplicateExportMethod in Exports.DuplicateExportMethods)
					{
						if (duplicateExportMethod.Duplicates.Count <= 0)
						{
							continue;
						}
						StringBuilder stringBuilder = new StringBuilder(200).AppendFormat("{0}.{1}", duplicateExportMethod.UsedExport.ExportedClass.NullSafeCall((ExportedClass ec) => ec.FullTypeName), duplicateExportMethod.UsedExport.Name);
						foreach (ExportedMethod duplicate in duplicateExportMethod.Duplicates)
						{
							stringBuilder.AppendFormat(", {0}.{1}", duplicate.ExportedClass.NullSafeCall((ExportedClass ec) => ec.FullTypeName), duplicate.Name);
						}
					}
				}
			}
			if (Exports.Count == 0)
			{
				return;
			}
			using (GetNotifier().CreateContextName(this, Resources.CreateTempDirectoryContextName))
			{
				using (ValueDisposable<string> valueDisposable = Utilities.CreateTempDirectory())
				{
					RunIlDasm(valueDisposable.Value);
					bool flag = new string[2] { "true", "yes" }.Any((string t) => t.Equals(InputValues.LeaveIntermediateFiles, StringComparison.InvariantCultureIgnoreCase));
					if (flag)
					{
						using (GetNotifier().CreateContextName(this, Resources.CopyBeforeContextName))
						{
							CopyDirectory(valueDisposable.Value, Path.Combine(Path.GetDirectoryName(InputValues.OutputFileName), "Before"), true);
						}
					}
					using (IlAsm ilAsm = PrepareIlAsm(valueDisposable.Value))
					{
						RunIlAsm(ilAsm);
					}
					if (flag)
					{
						using (GetNotifier().CreateContextName(this, Resources.CopyAfterContextName))
						{
							CopyDirectory(valueDisposable.Value, Path.Combine(Path.GetDirectoryName(InputValues.OutputFileName), "After"), true);
							return;
						}
					}
				}
			}
		}

		private IDllExportNotifier GetNotifier()
		{
			return base.ServiceProvider.GetService<IDllExportNotifier>();
		}

		private static string GetCleanedDirectoryPath(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetFullPath(path));
			if (directoryInfo.Parent == null)
			{
				return directoryInfo.FullName;
			}
			return Path.Combine(directoryInfo.Parent.FullName, directoryInfo.Name);
		}

		private static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool overwrite = false)
		{
			sourceDirectory = GetCleanedDirectoryPath(sourceDirectory);
			destinationDirectory = GetCleanedDirectoryPath(destinationDirectory);
			if (Directory.Exists(destinationDirectory) && !overwrite)
			{
				throw new IOException(Resources.The_destination_directory_already_exists_);
			}
			if (!Directory.Exists(destinationDirectory))
			{
				Directory.CreateDirectory(destinationDirectory);
			}
			int startIndex = sourceDirectory.Length + 1;
			string[] directories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);
			for (int i = 0; i < directories.Length; i++)
			{
				string path = Path.Combine(destinationDirectory, directories[i].Substring(startIndex));
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
			string[] files = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
			foreach (string text in files)
			{
				File.Copy(text, Path.Combine(destinationDirectory, text.Substring(startIndex)), overwrite);
			}
		}

		private void RunIlAsm(IlAsm ilAsm)
		{
			using (GetNotifier().CreateContextName(this, "RunIlAsm"))
			{
				if (InputValues.Cpu == CpuPlatform.AnyCpu)
				{
					string text = Path.GetDirectoryName(InputValues.OutputFileName) ?? "";
					string fileName = Path.GetFileName(InputValues.OutputFileName);
					if (!Directory.Exists(text))
					{
						throw new DirectoryNotFoundException(string.Format(Resources.Directory_0_does_not_exist, text));
					}
					GetNotifier().Notify(1, DllExportLogginCodes.CreatingBinariesForEachPlatform, Resources.Platform_is_0_creating_binaries_for_each_CPU_platform_in_a_separate_subfolder, InputValues.Cpu);
					ilAsm.ReassembleFile(Path.Combine(Path.Combine(text, "x86"), fileName), ".x86", CpuPlatform.X86);
					ilAsm.ReassembleFile(Path.Combine(Path.Combine(text, "x64"), fileName), ".x64", CpuPlatform.X64);
				}
				else
				{
					ilAsm.ReassembleFile(InputValues.OutputFileName, "", InputValues.Cpu);
				}
			}
		}

		private IlAsm PrepareIlAsm(string tempDirectory)
		{
			using (GetNotifier().CreateContextName(this, "PrepareIlAsm"))
			{
				IlAsm ilAsm2 = new IlAsm(this, InputValues);
				ilAsm2.Timeout = Timeout;
				return ilAsm2.TryInitialize(delegate(IlAsm ilAsm)
				{
					ilAsm.TempDirectory = tempDirectory;
					ilAsm.Exports = Exports;
				});
			}
		}

		private void RunIlDasm(string tempDirectory)
		{
			using (GetNotifier().CreateContextName(this, "RunIlDasm"))
			{
				IlDasm ilDasm = new IlDasm(this, InputValues);
				ilDasm.Timeout = Timeout;
				using (IlDasm ilDasm2 = ilDasm)
				{
					ilDasm2.TempDirectory = tempDirectory;
					ilDasm2.Run();
				}
			}
		}
	}
}
