using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport.Parsing
{
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public sealed class IlAsm : IlToolBase
	{
		private static readonly Regex _NormalizeIlErrorLineRegex = new Regex("(?:\\n|\\s|\\t|\\r|\\-|\\:|\\,)+", RegexOptions.Compiled);

		public AssemblyExports Exports { get; set; }

		public IlAsm(IServiceProvider serviceProvider, IInputValues inputValues)
			: base(serviceProvider, inputValues)
		{
		}

		public int ReassembleFile(string outputFile, string ilSuffix, CpuPlatform cpu)
		{
			string currentDirectory = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(base.TempDirectory);
			try
			{
				string directoryName = Path.GetDirectoryName(outputFile);
				if (directoryName != null && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				using (IlParser ilParser = new IlParser(base.ServiceProvider))
				{
					ilParser.Exports = Exports;
					ilParser.InputValues = base.InputValues;
					ilParser.TempDirectory = base.TempDirectory;
					List<string> list = new List<string>(ilParser.GetLines(cpu));
					if (list.Count > 0)
					{
						string text = list[list.Count - 1];
						if (!text.NullSafeCall((string l) => l.EndsWith("\\r") || l.EndsWith("\\n")))
						{
							list[list.Count - 1] = text + Environment.NewLine;
						}
					}
					using (FileStream stream = new FileStream(Path.Combine(base.TempDirectory, base.InputValues.FileName + ilSuffix + ".il"), FileMode.Create))
					{
						using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.Unicode))
						{
							foreach (string item in list)
							{
								streamWriter.WriteLine(item);
							}
						}
					}
				}
				return Run(outputFile, ilSuffix, cpu);
			}
			finally
			{
				Directory.SetCurrentDirectory(currentDirectory);
			}
		}

		private int Run(string outputFile, string ilSuffix, CpuPlatform cpu)
		{
			StringBuilder stringBuilder = new StringBuilder(100);
			string[] files = Directory.GetFiles(base.TempDirectory, "*.res");
			foreach (string text in files)
			{
				if (string.Equals(Path.GetExtension(text).NullSafeTrimStart('.'), "res", StringComparison.OrdinalIgnoreCase))
				{
					stringBuilder.Append(" \"/resource=").Append(text).Append("\" ");
				}
			}
			string text2 = stringBuilder.ToString();
			if (string.IsNullOrEmpty(text2))
			{
				text2 = " ";
			}
			string text3 = "";
			if (string.Equals(base.InputValues.InputFileName, outputFile, StringComparison.OrdinalIgnoreCase))
			{
				string text4 = base.InputValues.InputFileName + ".bak";
				int num = 1;
				do
				{
					text3 = text4 + num;
					num++;
				}
				while (File.Exists(text3));
				File.Move(base.InputValues.InputFileName, text3);
			}
			try
			{
				return RunCore(cpu, outputFile, text2, ilSuffix);
			}
			finally
			{
				if (!string.IsNullOrEmpty(text3) && File.Exists(text3))
				{
					File.Delete(text3);
				}
			}
		}

		private int RunCore(CpuPlatform cpu, string fileName, string ressourceParam, string ilSuffix)
		{
			string text = null;
			if (!string.IsNullOrEmpty(base.InputValues.KeyFile))
			{
				text = Path.GetFullPath(base.InputValues.KeyFile);
			}
			if (!string.IsNullOrEmpty(text) && !File.Exists(text))
			{
				if (!string.IsNullOrEmpty(base.InputValues.RootDirectory) && Directory.Exists(base.InputValues.RootDirectory))
				{
					text = Path.Combine(base.InputValues.RootDirectory, base.InputValues.KeyFile);
				}
				if (!File.Exists(text))
				{
					throw new FileNotFoundException(string.Format(Resources.Provided_key_file_0_cannot_be_found, text));
				}
			}
			string fullPath = Path.GetFullPath(Path.GetDirectoryName(fileName));
			int num = IlParser.RunIlTool(arguments: GetCommandLineArguments(cpu, fileName, ressourceParam, ilSuffix, text), installPath: base.InputValues.FrameworkPath, toolFileName: "ILAsm.exe", requiredPaths: null, workingDirectory: null, settingsName: "ILAsmPath", toolLoggingCode: DllExportLogginCodes.IlAsmLogging, verboseLoggingCode: DllExportLogginCodes.VerboseToolLogging, notifier: base.Notifier, timeout: base.Timeout, suppressErrorOutputLine: delegate(string line)
			{
				int num2 = line.IndexOf(": ");
				if (num2 > 0)
				{
					line = line.Substring(num2 + 1);
				}
				return _NormalizeIlErrorLineRegex.Replace(line, "").ToLowerInvariant().StartsWith("warningnonvirtualnonabstractinstancemethodininterfacesettosuch");
			});
			if (num == 0)
			{
				RunLibTool(cpu, fileName, fullPath);
			}
			return num;
		}

		private int RunLibTool(CpuPlatform cpu, string fileName, string directory)
		{
			if (string.IsNullOrEmpty(base.InputValues.LibToolPath))
			{
				return 0;
			}
			string text = CreateDefFile(cpu, directory, GetLibraryFileNameRoot(fileName));
			try
			{
				return RunLibToolCore(cpu, directory, text);
			}
			catch (Exception ex)
			{
				base.Notifier.Notify(1, DllExportLogginCodes.LibToolLooging, Resources.An_error_occurred_while_calling_0_1_, "lib.exe", ex.Message);
				return -1;
			}
			finally
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
		}

		[Localizable(false)]
		private int RunLibToolCore(CpuPlatform cpu, string directory, string defFileName)
		{
			string text = Path.Combine(directory, Path.GetFileNameWithoutExtension(base.InputValues.OutputFileName)) + ".lib";
			try
			{
				return IlParser.RunIlTool(requiredPaths: (!string.IsNullOrEmpty(base.InputValues.LibToolDllPath) && Directory.Exists(base.InputValues.LibToolDllPath)) ? base.InputValues.LibToolDllPath : null, installPath: base.InputValues.LibToolPath, toolFileName: "Lib.exe", workingDirectory: null, settingsName: "LibToolPath", arguments: string.Format("\"/def:{0}\" /machine:{1} \"/out:{2}\"", defFileName, cpu, text), toolLoggingCode: DllExportLogginCodes.LibToolLooging, verboseLoggingCode: DllExportLogginCodes.LibToolVerboseLooging, notifier: base.Notifier, timeout: base.Timeout);
			}
			catch (Exception)
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				throw;
			}
		}

		private string CreateDefFile(CpuPlatform cpu, string directory, string libraryName)
		{
			string text = Path.Combine(directory, string.Concat(libraryName, ".", cpu, ".def"));
			try
			{
				using (FileStream stream = new FileStream(text, FileMode.Create))
				{
					using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8))
					{
						streamWriter.WriteLine("LIBRARY {0}.dll", libraryName);
						streamWriter.WriteLine();
						streamWriter.WriteLine("EXPORTS");
						foreach (ExportedClass value in Exports.ClassesByName.Values)
						{
							foreach (ExportedMethod method in value.Methods)
							{
								streamWriter.WriteLine(method.ExportName);
							}
						}
					}
				}
				return text;
			}
			catch (Exception)
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				throw;
			}
		}

		private static string GetLibraryFileNameRoot(string fileName)
		{
			fileName = ((!string.Equals(Path.GetExtension(fileName).TrimStart('.'), "dll", StringComparison.InvariantCultureIgnoreCase)) ? Path.GetFileName(fileName) : Path.GetFileNameWithoutExtension(fileName));
			return fileName;
		}

		[Localizable(false)]
		private string GetCommandLineArguments(CpuPlatform cpu, string fileName, string ressourceParam, string ilSuffix, string keyFile)
        {
            string archstr = "";
            if (cpu == CpuPlatform.X64 || cpu == CpuPlatform.ARM64 || cpu == CpuPlatform.Itanium)
            {
                archstr += "/PE64 ";
            }
            if (cpu == CpuPlatform.X64)
            {
                archstr += "/X64";
            }
            if (cpu == CpuPlatform.ARM)
            {
                archstr += "/ARM";
            }
            if (cpu == CpuPlatform.ARM64)
            {
                archstr += "/ARM64";
            }
            return string.Format(CultureInfo.InvariantCulture, "/nologo \"/out:{0}\" \"{1}.il\" /DLL{2} {3} {4} {5}", fileName, Path.Combine(base.TempDirectory, Path.GetFileNameWithoutExtension(base.InputValues.InputFileName)) + ilSuffix, ressourceParam, base.InputValues.EmitDebugSymbols ? "/debug" : "/optimize", archstr, (!string.IsNullOrEmpty(keyFile)) ? ("\"/Key=" + keyFile + '"') : ((!string.IsNullOrEmpty(base.InputValues.KeyContainer)) ? ("\"/Key=@" + base.InputValues.KeyContainer + "\"") : null));
		}
	}
}
