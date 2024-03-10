using System;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NppPlugin.DllExport.MSBuild
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class DllExportTask : Task, IDllExportTask, IInputValues, IServiceProvider
	{
		private readonly ExportTaskImplementation<DllExportTask> _ExportTaskImplementation;

		private IServiceProvider _ServiceProvider
		{
			get
			{
				return _ExportTaskImplementation;
			}
		}

		public string MethodAttributes
		{
			get
			{
				return _ExportTaskImplementation.MethodAttributes;
			}
			set
			{
				_ExportTaskImplementation.MethodAttributes = value;
			}
		}

		bool? IDllExportTask.SkipOnAnyCpu
		{
			get
			{
				return _ExportTaskImplementation.SkipOnAnyCpu;
			}
			set
			{
				_ExportTaskImplementation.SkipOnAnyCpu = value;
			}
		}

		public string SkipOnAnyCpu
		{
			get
			{
				return Convert.ToString(_ExportTaskImplementation.SkipOnAnyCpu);
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_ExportTaskImplementation.SkipOnAnyCpu = null;
				}
				else
				{
					_ExportTaskImplementation.SkipOnAnyCpu = Convert.ToBoolean(value);
				}
			}
		}

		public string TargetFrameworkVersion
		{
			get
			{
				return _ExportTaskImplementation.TargetFrameworkVersion;
			}
			set
			{
				_ExportTaskImplementation.TargetFrameworkVersion = value;
			}
		}

		public string Platform
		{
			get
			{
				return _ExportTaskImplementation.Platform;
			}
			set
			{
				_ExportTaskImplementation.Platform = value;
			}
		}

		public string PlatformTarget
		{
			get
			{
				return _ExportTaskImplementation.PlatformTarget;
			}
			set
			{
				_ExportTaskImplementation.PlatformTarget = value;
			}
		}

		public string CpuType
		{
			get
			{
				return _ExportTaskImplementation.CpuType;
			}
			set
			{
				_ExportTaskImplementation.CpuType = value;
			}
		}

		public string ProjectDirectory
		{
			get
			{
				return _ExportTaskImplementation.ProjectDirectory;
			}
			set
			{
				_ExportTaskImplementation.ProjectDirectory = value;
			}
		}

		public string AssemblyKeyContainerName
		{
			get
			{
				return _ExportTaskImplementation.AssemblyKeyContainerName;
			}
			set
			{
				_ExportTaskImplementation.AssemblyKeyContainerName = value;
			}
		}

		public int Timeout
		{
			get
			{
				return _ExportTaskImplementation.Timeout;
			}
			set
			{
				_ExportTaskImplementation.Timeout = value;
			}
		}

		public CpuPlatform Cpu
		{
			get
			{
				return _ExportTaskImplementation.Cpu;
			}
			set
			{
				_ExportTaskImplementation.Cpu = value;
			}
		}

		public bool EmitDebugSymbols
		{
			get
			{
				return _ExportTaskImplementation.EmitDebugSymbols;
			}
			set
			{
				_ExportTaskImplementation.EmitDebugSymbols = value;
			}
		}

		public string LeaveIntermediateFiles
		{
			get
			{
				return _ExportTaskImplementation.LeaveIntermediateFiles;
			}
			set
			{
				_ExportTaskImplementation.LeaveIntermediateFiles = value;
			}
		}

		public string FileName
		{
			get
			{
				return _ExportTaskImplementation.FileName;
			}
			set
			{
				_ExportTaskImplementation.FileName = value;
			}
		}

		[Required]
		public string FrameworkPath
		{
			get
			{
				return _ExportTaskImplementation.FrameworkPath;
			}
			set
			{
				_ExportTaskImplementation.FrameworkPath = value;
			}
		}

		public string LibToolPath
		{
			get
			{
				return _ExportTaskImplementation.LibToolPath;
			}
			set
			{
				_ExportTaskImplementation.LibToolPath = value;
			}
		}

		public string LibToolDllPath
		{
			get
			{
				return _ExportTaskImplementation.LibToolDllPath;
			}
			set
			{
				_ExportTaskImplementation.LibToolDllPath = value;
			}
		}

		[Required]
		public string InputFileName
		{
			get
			{
				return _ExportTaskImplementation.InputFileName;
			}
			set
			{
				_ExportTaskImplementation.InputFileName = value;
			}
		}

		public string KeyContainer
		{
			get
			{
				return _ExportTaskImplementation.KeyContainer;
			}
			set
			{
				_ExportTaskImplementation.KeyContainer = value;
			}
		}

		public string KeyFile
		{
			get
			{
				return _ExportTaskImplementation.KeyFile;
			}
			set
			{
				_ExportTaskImplementation.KeyFile = value;
			}
		}

		public string OutputFileName
		{
			get
			{
				return _ExportTaskImplementation.OutputFileName;
			}
			set
			{
				_ExportTaskImplementation.OutputFileName = value;
			}
		}

		public string RootDirectory
		{
			get
			{
				return _ExportTaskImplementation.RootDirectory;
			}
			set
			{
				_ExportTaskImplementation.RootDirectory = value;
			}
		}

		[Required]
		public string SdkPath
		{
			get
			{
				return _ExportTaskImplementation.SdkPath;
			}
			set
			{
				_ExportTaskImplementation.SdkPath = value;
			}
		}

		public string DllExportAttributeFullName
		{
			get
			{
				return _ExportTaskImplementation.DllExportAttributeFullName;
			}
			set
			{
				_ExportTaskImplementation.DllExportAttributeFullName = value;
			}
		}

		public string DllExportAttributeAssemblyName
		{
			get
			{
				return _ExportTaskImplementation.DllExportAttributeAssemblyName;
			}
			set
			{
				_ExportTaskImplementation.DllExportAttributeAssemblyName = value;
			}
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return _ServiceProvider.GetService(serviceType);
		}

		public IDllExportNotifier GetNotifier()
		{
			return _ExportTaskImplementation.GetNotifier();
		}

		public void Notify(int severity, string code, string message, params object[] values)
		{
			_ExportTaskImplementation.Notify(severity, code, message, values);
		}

		public void Notify(int severity, string code, string fileName, SourceCodePosition? startPosition, SourceCodePosition? endPosition, string message, params object[] values)
		{
			_ExportTaskImplementation.Notify(severity, code, fileName, startPosition, endPosition, message, values);
		}

		static DllExportTask()
		{
			AssemblyLoadingRedirection.EnsureSetup();
		}

		public DllExportTask()
		{
			_ExportTaskImplementation = new ExportTaskImplementation<DllExportTask>(this);
		}

		public DllExportTask(ResourceManager taskResources)
			: base(taskResources)
		{
			_ExportTaskImplementation = new ExportTaskImplementation<DllExportTask>(this);
		}

		public DllExportTask(ResourceManager taskResources, string helpKeywordPrefix)
			: base(taskResources, helpKeywordPrefix)
		{
			_ExportTaskImplementation = new ExportTaskImplementation<DllExportTask>(this);
		}

		[CLSCompliant(false)]
		public AssemblyBinaryProperties InferAssemblyBinaryProperties()
		{
			return _ExportTaskImplementation.InferAssemblyBinaryProperties();
		}

		public void InferOutputFile()
		{
			_ExportTaskImplementation.InferOutputFile();
		}

		public override bool Execute()
		{
			return _ExportTaskImplementation.Execute();
		}

        [SpecialName]
        TaskLoggingHelper IDllExportTask.Log
        {
            get
            {
                return base.Log;
            }
        }
    }
}
