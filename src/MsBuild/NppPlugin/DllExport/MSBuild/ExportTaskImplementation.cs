using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using NppPlugin.DllExport.MSBuild.Properties;

namespace NppPlugin.DllExport.MSBuild
{
	public class ExportTaskImplementation<TTask> : IInputValues, IServiceContainer, IServiceProvider where TTask : IDllExportTask, ITask
	{
		private sealed class DllExportNotifierWithTask : DllExportNotifier
		{
			public TTask ActualTask { get; private set; }

			public DllExportNotifierWithTask(TTask actualTask)
			{
				ActualTask = actualTask;
			}
		}

		private const string ToolLocationHelperTypeName = "Microsoft.Build.Utilities.ToolLocationHelper";

		private const string UndefinedPropertyValue = "*Undefined*";

		private readonly IServiceContainer _ServiceProvider = new ServiceContainer();

		private readonly TTask _ActualTask;

		private readonly Dictionary<object, string> _LoggedMessages = new Dictionary<object, string>();

		private readonly IInputValues _Values;

		private int _ErrorCount;

		private int _Timeout = 45000;

		private static readonly Version _VersionUsingToolLocationHelper;

		private static readonly IDictionary<string, Func<Version, string, string>> _GetFrameworkToolPathByMethodName;

		private static readonly MethodInfo WrapGetToolPathCallMethodInfo;

		public string MethodAttributes
		{
			get
			{
				return _Values.MethodAttributes;
			}
			set
			{
				_Values.MethodAttributes = value;
			}
		}

		public CpuPlatform Cpu
		{
			get
			{
				return _Values.Cpu;
			}
			set
			{
				_Values.Cpu = value;
			}
		}

		public string TargetFrameworkVersion { get; set; }

		public bool? SkipOnAnyCpu { get; set; }

		public bool EmitDebugSymbols
		{
			get
			{
				return _Values.EmitDebugSymbols;
			}
			set
			{
				_Values.EmitDebugSymbols = value;
			}
		}

		public string LeaveIntermediateFiles
		{
			get
			{
				return _Values.LeaveIntermediateFiles;
			}
			set
			{
				_Values.LeaveIntermediateFiles = value;
			}
		}

		public string FileName
		{
			get
			{
				return _Values.FileName;
			}
			set
			{
				_Values.FileName = value;
			}
		}

		[Required]
		public string FrameworkPath
		{
			get
			{
				return _Values.FrameworkPath;
			}
			set
			{
				_Values.FrameworkPath = value;
			}
		}

		public string LibToolPath
		{
			get
			{
				return _Values.LibToolPath;
			}
			set
			{
				_Values.LibToolPath = value;
			}
		}

		public string LibToolDllPath
		{
			get
			{
				return _Values.LibToolDllPath;
			}
			set
			{
				_Values.LibToolDllPath = value;
			}
		}

		[Required]
		public string InputFileName
		{
			get
			{
				return _Values.InputFileName;
			}
			set
			{
				_Values.InputFileName = value;
			}
		}

		public string KeyContainer
		{
			get
			{
				return _Values.KeyContainer;
			}
			set
			{
				_Values.KeyContainer = value;
			}
		}

		public string KeyFile
		{
			get
			{
				return _Values.KeyFile;
			}
			set
			{
				_Values.KeyFile = value;
			}
		}

		public string OutputFileName
		{
			get
			{
				return _Values.OutputFileName;
			}
			set
			{
				_Values.OutputFileName = value;
			}
		}

		public string RootDirectory
		{
			get
			{
				return _Values.RootDirectory;
			}
			set
			{
				_Values.RootDirectory = value;
			}
		}

		public string SdkPath
		{
			get
			{
				return _Values.SdkPath;
			}
			set
			{
				_Values.SdkPath = value;
			}
		}

		public string DllExportAttributeFullName
		{
			get
			{
				return _Values.DllExportAttributeFullName;
			}
			set
			{
				_Values.DllExportAttributeFullName = value;
			}
		}

		public string DllExportAttributeAssemblyName
		{
			get
			{
				return _Values.DllExportAttributeAssemblyName;
			}
			set
			{
				_Values.DllExportAttributeAssemblyName = value;
			}
		}

		public string Platform { get; set; }

		public string PlatformTarget { get; set; }

		public string CpuType { get; set; }

		public string ProjectDirectory
		{
			get
			{
				return _Values.RootDirectory;
			}
			set
			{
				_Values.RootDirectory = value;
			}
		}

		public string AssemblyKeyContainerName
		{
			get
			{
				return _Values.KeyContainer;
			}
			set
			{
				_Values.KeyContainer = value;
			}
		}

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

		private static Func<Version, string, string> GetFrameworkToolPath
		{
			get
			{
				return GetGetToolPath("GetPathToDotNetFrameworkFile");
			}
		}

		private static Func<Version, string, string> GetSdkToolPath
		{
			get
			{
				return GetGetToolPath("GetPathToDotNetFrameworkSdkFile");
			}
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return _ServiceProvider.GetService(serviceType);
		}

		void IServiceContainer.AddService(Type serviceType, object serviceInstance)
		{
			_ServiceProvider.AddService(serviceType, serviceInstance);
		}

		void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
		{
			_ServiceProvider.AddService(serviceType, serviceInstance, promote);
		}

		void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
		{
			_ServiceProvider.AddService(serviceType, callback);
		}

		void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
		{
			_ServiceProvider.AddService(serviceType, callback, promote);
		}

		void IServiceContainer.RemoveService(Type serviceType)
		{
			_ServiceProvider.RemoveService(serviceType);
		}

		void IServiceContainer.RemoveService(Type serviceType, bool promote)
		{
			_ServiceProvider.RemoveService(serviceType, promote);
		}

		public IDllExportNotifier GetNotifier()
		{
			return this.GetService<IDllExportNotifier>();
		}

		public void Notify(int severity, string code, string message, params object[] values)
		{
			GetNotifier().Notify(severity, code, message, values);
		}

		public void Notify(int severity, string code, string fileName, SourceCodePosition? startPosition, SourceCodePosition? endPosition, string message, params object[] values)
		{
			GetNotifier().Notify(severity, code, fileName, startPosition, endPosition, message, values);
		}

		public ExportTaskImplementation(TTask actualTask)
		{
			_ActualTask = actualTask;
			this.AddServiceFactory((Func<IServiceProvider, IDllExportNotifier>)((IServiceProvider sp) => new DllExportNotifierWithTask(_ActualTask)));
			_Values = new InputValuesCore(this);
			GetNotifier().Notification += OnDllWrapperNotification;
		}

		static ExportTaskImplementation()
		{
			_VersionUsingToolLocationHelper = new Version(4, 5);
			_GetFrameworkToolPathByMethodName = new Dictionary<string, Func<Version, string, string>>();
			WrapGetToolPathCallMethodInfo = Utilities.GetMethodInfo(() => WrapGetToolPathCall<int>(null)).GetGenericMethodDefinition();
			AssemblyLoadingRedirection.EnsureSetup();
		}

		[CLSCompliant(false)]
		public AssemblyBinaryProperties InferAssemblyBinaryProperties()
		{
			return _Values.InferAssemblyBinaryProperties();
		}

		public void InferOutputFile()
		{
			_Values.InferOutputFile();
		}

		public bool Execute()
		{
			_ErrorCount = 0;
			if (ValidateInputValues())
			{
				_Values.Cpu = Utilities.ToCpuPlatform(Platform);
				try
				{
					if ((SkipOnAnyCpu ?? true) && _Values.Cpu == CpuPlatform.AnyCpu)
					{
						TTask actualTask = _ActualTask;
						actualTask.Log.LogMessage(Resources.Skipped_Method_Exports);
						return true;
					}
					AssemblyBinaryProperties assemblyBinaryProperties = _Values.InferAssemblyBinaryProperties();
					if (string.IsNullOrEmpty(KeyFile) && !string.IsNullOrEmpty(assemblyBinaryProperties.KeyFileName))
					{
						KeyFile = assemblyBinaryProperties.KeyFileName;
					}
					if (string.IsNullOrEmpty(KeyContainer) && !string.IsNullOrEmpty(assemblyBinaryProperties.KeyContainer))
					{
						KeyContainer = assemblyBinaryProperties.KeyContainer;
					}
					_Values.InferOutputFile();
					ValidateKeyFiles(assemblyBinaryProperties.IsSigned);
					DllExportWeaver dllExportWeaver = new DllExportWeaver(this);
					dllExportWeaver.Timeout = Timeout;
					using (DllExportWeaver dllExportWeaver2 = dllExportWeaver)
					{
						dllExportWeaver2.InputValues = _ActualTask;
						dllExportWeaver2.Run();
					}
					return _ErrorCount == 0;
				}
				catch (Exception ex)
				{
					TTask actualTask2 = _ActualTask;
					actualTask2.Log.LogErrorFromException(ex);
					TTask actualTask3 = _ActualTask;
					actualTask3.Log.LogMessage(ex.StackTrace);
				}
				_LoggedMessages.Clear();
			}
			return false;
		}

		private void OnDllWrapperNotification(object sender, DllExportNotificationEventArgs e)
		{
			MessageImportance messageImportance = GetMessageImportance(e.Severity);
			string text;
			if (!string.IsNullOrEmpty(e.FileName))
			{
				text = e.FileName;
			}
			else
			{
				TTask actualTask = _ActualTask;
				text = actualTask.BuildEngine.ProjectFileOfTaskNode;
			}
			string text2 = text;
			SourceCodePosition startPos = e.StartPosition ?? new SourceCodePosition(0, 0);
			SourceCodePosition endPos = e.EndPosition ?? new SourceCodePosition(0, 0);
			string text3 = e.Message;
			if (e.Severity > 0 && e.Context != null && e.Context.Name != null)
			{
				text3 = e.Context.Name + ": " + text3;
			}
			var key = new
			{
				startPos = startPos,
				endPos = endPos,
				fileName = text2,
				Severity = e.Severity,
				Code = e.Code,
				Message = text3
			};
			if (!_LoggedMessages.ContainsKey(key))
			{
				_LoggedMessages.Add(key, text3);
				if (e.Severity == 1)
				{
					TTask actualTask2 = _ActualTask;
					actualTask2.Log.LogWarning("Export", e.Code, null, text2, startPos.Line, startPos.Character, endPos.Line, endPos.Character, text3);
				}
				else if (e.Severity > 1)
				{
					_ErrorCount++;
					TTask actualTask3 = _ActualTask;
					actualTask3.Log.LogError("Export", e.Code, null, text2, startPos.Line, startPos.Character, endPos.Line, endPos.Character, text3);
				}
				else
				{
					TTask actualTask4 = _ActualTask;
					actualTask4.Log.LogMessage((MessageImportance)messageImportance, text3);
				}
			}
		}

		private static MessageImportance GetMessageImportance(int severity)
		{
			MessageImportance result = MessageImportance.Normal;
			if (severity < -1)
			{
				result = MessageImportance.Low;
			}
			else if (severity == 0)
			{
				result = MessageImportance.High;
			}
			return result;
		}

		private bool ValidateInputValues()
		{
			bool flag = ValidateLibToolPath() & ValidateFrameworkPath() & ValidateSdkPath();
			if (!string.IsNullOrEmpty(CpuType) && (string.IsNullOrEmpty(Platform) || string.Equals(Platform, "anycpu", StringComparison.OrdinalIgnoreCase)))
			{
				Platform = CpuType;
			}
			if (!string.IsNullOrEmpty(PlatformTarget) && (string.IsNullOrEmpty(Platform) || string.Equals(Platform, "anycpu", StringComparison.OrdinalIgnoreCase)))
			{
				Platform = PlatformTarget;
			}
			return flag & ValidateFileName();
		}

		private void ValidateKeyFiles(bool isSigned)
		{
			if (isSigned && string.IsNullOrEmpty(KeyContainer) && string.IsNullOrEmpty(KeyFile))
			{
				TTask actualTask = _ActualTask;
				actualTask.Log.LogWarning(Resources.Output_assembly_was_signed_however_neither_keyfile_nor_keycontainer_could_be_inferred__Reading_those_values_from_assembly_attributes_is_not__yet__supported__they_have_to_be_defined_inside_the_MSBuild_project_file);
			}
			if (!string.IsNullOrEmpty(_Values.KeyContainer) && !string.IsNullOrEmpty(_Values.KeyFile))
			{
				TTask actualTask2 = _ActualTask;
				actualTask2.Log.LogError(Resources.Both_key_values_KeyContainer_0_and_KeyFile_0_are_present_only_one_can_be_specified, _Values.KeyContainer, _Values.KeyFile);
			}
		}

		private static Func<Version, string, string> GetGetToolPath(string methodName)
		{
			lock (_GetFrameworkToolPathByMethodName)
			{
				Func<Version, string, string> value;
				if (!_GetFrameworkToolPathByMethodName.TryGetValue(methodName, out value))
				{
					_GetFrameworkToolPathByMethodName.Add(methodName, value = GetGetToolPathInternal(methodName));
				}
				return value;
			}
		}

		private static Func<string, int, string> WrapGetToolPathCall<TTargetDotNetFrameworkVersion>(MethodInfo methodInfo)
		{
			Func<string, TTargetDotNetFrameworkVersion, string> actualCall = (Func<string, TTargetDotNetFrameworkVersion, string>)Delegate.CreateDelegate(typeof(Func<string, TTargetDotNetFrameworkVersion, string>), methodInfo);
			return delegate(string fileName, int versionValue)
			{
				TTargetDotNetFrameworkVersion arg = (TTargetDotNetFrameworkVersion)Enum.ToObject(typeof(TTargetDotNetFrameworkVersion), versionValue);
				return actualCall(fileName, arg);
			};
		}

		private static Func<Version, string, string> GetGetToolPathInternal(string methodName)
		{
			Type type = Type.GetType("Microsoft.Build.Utilities.ToolLocationHelper") ?? Type.GetType("Microsoft.Build.Utilities.ToolLocationHelper, Microsoft.Build.Utilities.v4.0, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
			if (type == null)
			{
				Assembly assembly;
				try
				{
					assembly = Assembly.Load("Microsoft.Build.Utilities.v4.0, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				}
				catch (FileNotFoundException)
				{
					assembly = null;
				}
				if (assembly != null)
				{
					type = assembly.GetType("Microsoft.Build.Utilities.ToolLocationHelper");
				}
			}
			if (type == null)
			{
				return null;
			}
			Type targetDotNetFrameworkVersionType = type.Assembly.GetType("Microsoft.Build.Utilities.TargetDotNetFrameworkVersion");
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, new Type[2]
			{
				typeof(string),
				targetDotNetFrameworkVersionType
			}, null);
			if (method == null)
			{
				return null;
			}
			Func<string, int, string> getToolPathCore = (Func<string, int, string>)WrapGetToolPathCallMethodInfo.MakeGenericMethod(targetDotNetFrameworkVersionType).Invoke(null, new object[1] { method });
			Func<string, int, string> getToolPath = delegate(string n, int v)
			{
				try
				{
					return getToolPathCore(n, v);
				}
				catch (ArgumentException)
				{
					return null;
				}
			};
			return delegate(Version version, string toolName)
			{
				int num = (int)Enum.Parse(targetDotNetFrameworkVersionType, "Version" + version.Major + version.Minor);
				string text;
				for (text = getToolPath(toolName, num); text == null; text = getToolPath(toolName, num))
				{
					num++;
					if (!Enum.IsDefined(targetDotNetFrameworkVersionType, num))
					{
						return null;
					}
				}
				while (text != null && !File.Exists(text))
				{
					num--;
					if (!Enum.IsDefined(targetDotNetFrameworkVersionType, num))
					{
						return null;
					}
					text = getToolPath(toolName, num);
				}
				return (text != null && !File.Exists(text)) ? null : text;
			};
		}

		private bool ValidateFrameworkPath()
		{
			string foundPath;
			if (!ValidateToolPath("ilasm.exe", FrameworkPath, GetFrameworkToolPath, out foundPath))
			{
				return false;
			}
			FrameworkPath = foundPath;
			return true;
		}

		private bool ValidateToolPath(string toolFileName, string currentValue, Func<Version, string, string> getToolPath, out string foundPath)
		{
			if (PropertyHasValue(TargetFrameworkVersion))
			{
				string toolDirectory = currentValue;
				if (TryToGetToolDirForFxVersion(toolFileName, getToolPath, ref toolDirectory))
				{
					foundPath = toolDirectory;
					return true;
				}
			}
			if (PropertyHasValue(currentValue) && TrySearchToolPath(currentValue, toolFileName, out foundPath))
			{
				return true;
			}
			foundPath = null;
			return false;
		}

		private bool TryToGetToolDirForFxVersion(string toolFileName, Func<Version, string, string> getToolPath, ref string toolDirectory)
		{
			Version frameworkVersion = GetFrameworkVersion();
			if (getToolPath != null)
			{
				string text = getToolPath(frameworkVersion, toolFileName);
				if (text != null && File.Exists(text))
				{
					toolDirectory = Path.GetDirectoryName(text);
					return true;
				}
				TTask actualTask = _ActualTask;
				actualTask.Log.LogError(string.Format(Resources.ToolLocationHelperTypeName_could_not_find_1, "Microsoft.Build.Utilities.ToolLocationHelper", toolFileName));
				return false;
			}
			if (frameworkVersion >= _VersionUsingToolLocationHelper)
			{
				TTask actualTask2 = _ActualTask;
				actualTask2.Log.LogError(string.Format(Resources.Cannot_get_a_reference_to_ToolLocationHelper, "Microsoft.Build.Utilities.ToolLocationHelper"));
				return false;
			}
			return false;
		}

		private Version GetFrameworkVersion()
		{
			string targetFrameworkVersion = TargetFrameworkVersion;
			if (PropertyHasValue(targetFrameworkVersion))
			{
				return new Version(targetFrameworkVersion.TrimStart('v', 'V'));
			}
			return null;
		}

		private bool ValidateSdkPath()
		{
			string foundPath;
			if (!ValidateToolPath("ildasm.exe", SdkPath, GetSdkToolPath, out foundPath))
			{
				return false;
			}
			SdkPath = foundPath;
			return true;
		}

		private static bool PropertyHasValue(string propertyValue)
		{
			if (!string.IsNullOrEmpty(propertyValue))
			{
				return !propertyValue.Contains("*Undefined*");
			}
			return false;
		}

		private bool ValidateLibToolPath()
		{
			string value = null;
			if (PropertyHasValue(LibToolPath))
			{
				if (!TrySearchToolPath(LibToolPath, "lib.exe", out value))
				{
					TTask actualTask = _ActualTask;
					actualTask.Log.LogMessage(MessageImportance.Normal, Resources.Cannot_find_lib_exe_in_0_, LibToolPath);
					LibToolPath = null;
					return false;
				}
				LibToolPath = value;
			}
			else
			{
				LibToolPath = null;
				string text = Environment.GetEnvironmentVariable("DevEnvDir").NullIfEmpty() ?? GetVsPath();
				if (PropertyHasValue(text))
				{
					if (!TrySearchToolPath(text, "lib.exe", out value) && !TrySearchToolPath(Path.Combine(text, "VC"), "lib.exe", out value) && !TrySearchToolPath(Path.Combine(Path.Combine(text, "VC"), "bin"), "lib.exe", out value))
					{
						TTask actualTask2 = _ActualTask;
						actualTask2.Log.LogMessage(MessageImportance.Low, Resources.Cannot_find_lib_exe_in_0_, LibToolPath);
						LibToolPath = null;
						return true;
					}
					LibToolPath = value;
				}
			}
			if (!PropertyHasValue(value))
			{
				value = null;
			}
			if (!PropertyHasValue(LibToolDllPath))
			{
				if (!PropertyHasValue(value))
				{
					LibToolDllPath = null;
				}
				else
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(value);
					while (directoryInfo != null && !Directory.Exists(Path.Combine(Path.Combine(directoryInfo.FullName, "Common7"), "IDE")))
					{
						directoryInfo = directoryInfo.Parent;
					}
					if (directoryInfo != null)
					{
						string text2 = Path.Combine(Path.Combine(directoryInfo.FullName, "Common7"), "IDE");
						if (Directory.Exists(text2))
						{
							LibToolDllPath = text2;
						}
					}
				}
			}
			return true;
		}

		private static string GetVsPath()
		{
			string text = Environment.GetEnvironmentVariable("VisualStudioVersion").NullIfEmpty();
			if (text == null)
			{
				return null;
			}
			Version version = new Version(text);
			string text2 = Environment.GetEnvironmentVariable(string.Format("VS{0}{1}COMNTOOLS", version.Major, version.Minor)).NullIfEmpty();
			if (text2 == null)
			{
				return null;
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(text2);
			if (directoryInfo.Name.Equals("tools", StringComparison.InvariantCultureIgnoreCase) && directoryInfo.Exists)
			{
				DirectoryInfo parent = directoryInfo.Parent;
				if (parent != null && parent.Parent != null && parent.Name.Equals("common7", StringComparison.InvariantCultureIgnoreCase))
				{
					return parent.Parent.FullName;
				}
			}
			return null;
		}

		public static bool TrySearchToolPath(string toolPath, string toolFilename, out string value)
		{
			value = null;
			while (toolPath.Contains("\\\\"))
			{
				toolPath = toolPath.Replace("\\\\", "\\");
			}
			string[] array = toolPath.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					string text2 = text;
					if (File.Exists(Path.Combine(text2, toolFilename)))
					{
						value = text2;
						return true;
					}
					text2 = Path.GetFullPath(text);
					if (File.Exists(Path.Combine(text2, toolFilename)))
					{
						value = text2;
						return true;
					}
				}
			}
			return false;
		}

		private bool ValidateFileName()
		{
			bool result = false;
			if (string.IsNullOrEmpty(_Values.InputFileName))
			{
				TTask actualTask = _ActualTask;
				actualTask.Log.LogWarning(Resources.Input_file_is_empty__cannot_create_unmanaged_exports);
			}
			else if (!File.Exists(_Values.InputFileName))
			{
				TTask actualTask2 = _ActualTask;
				actualTask2.Log.LogWarning(Resources.Input_file_0_does_not_exist__cannot_create_unmanaged_exports, _Values.InputFileName);
			}
			else
			{
				if (!string.Equals(Path.GetExtension(_Values.InputFileName).TrimStart('.'), "dll", StringComparison.OrdinalIgnoreCase))
				{
					TTask actualTask3 = _ActualTask;
					actualTask3.Log.LogMessage(Resources.Input_file_0_is_not_a_DLL_hint, _Values.InputFileName);
				}
				result = true;
			}
			return result;
		}
	}
}
