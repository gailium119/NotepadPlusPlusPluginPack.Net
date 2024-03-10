using System;
using System.IO;
using System.Threading;
using NppPlugin.DllExport.Parsing;

namespace NppPlugin.DllExport
{
	public class InputValuesCore : HasServiceProvider, IInputValues
	{
		private string _DllExportAttributeAssemblyName = Utilities.DllExportAttributeAssemblyName;

		private string _DllExportAttributeFullName = Utilities.DllExportAttributeFullName;

		private string _Filename;

		public CpuPlatform Cpu { get; set; }

		public string LeaveIntermediateFiles { get; set; }

		public bool EmitDebugSymbols { get; set; }

		public string FrameworkPath { get; set; }

		public string InputFileName { get; set; }

		public string KeyContainer { get; set; }

		public string KeyFile { get; set; }

		public string OutputFileName { get; set; }

		public string RootDirectory { get; set; }

		public string SdkPath { get; set; }

		public string MethodAttributes { get; set; }

		public string LibToolPath { get; set; }

		public string LibToolDllPath { get; set; }

		public string DllExportAttributeFullName
		{
			get
			{
				return _DllExportAttributeFullName;
			}
			set
			{
				_DllExportAttributeFullName = value;
			}
		}

		public string DllExportAttributeAssemblyName
		{
			get
			{
				return _DllExportAttributeAssemblyName;
			}
			set
			{
				_DllExportAttributeAssemblyName = value;
			}
		}

		public string FileName
		{
			get
			{
				Monitor.Enter(this);
				try
				{
					if (string.IsNullOrEmpty(_Filename))
					{
						_Filename = Path.GetFileNameWithoutExtension(InputFileName);
					}
				}
				finally
				{
					Monitor.Exit(this);
				}
				return _Filename;
			}
			set
			{
				Monitor.Enter(this);
				try
				{
					_Filename = value;
				}
				finally
				{
					Monitor.Exit(this);
				}
			}
		}

		public InputValuesCore(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public AssemblyBinaryProperties InferAssemblyBinaryProperties()
		{
			AssemblyBinaryProperties assemblyBinaryProperties = Utilities.CreateAssemblyInspector(this).GetAssemblyBinaryProperties(InputFileName);
			if (Cpu == CpuPlatform.None && assemblyBinaryProperties.BinaryWasScanned)
			{
				Cpu = assemblyBinaryProperties.CpuPlatform;
			}
			return assemblyBinaryProperties;
		}

		public void InferOutputFile()
		{
			if (string.IsNullOrEmpty(OutputFileName))
			{
				OutputFileName = InputFileName;
			}
		}
	}
}
