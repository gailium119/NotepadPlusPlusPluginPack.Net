using Mono.Cecil;

namespace NppPlugin.DllExport
{
	public sealed class AssemblyBinaryProperties
	{
		public static readonly AssemblyBinaryProperties EmptyNotScanned = new AssemblyBinaryProperties((ModuleAttributes)0, TargetArchitecture.I386, false, null, null, false);

		private readonly bool _BinaryWasScanned;

		private readonly bool _IsSigned;

		private readonly string _KeyContainer;

		private readonly string _KeyFileName;

		private readonly TargetArchitecture _MachineKind;

		private readonly ModuleAttributes _PeKind;

		public string KeyFileName
		{
			get
			{
				return _KeyFileName;
			}
		}

		public string KeyContainer
		{
			get
			{
				return _KeyContainer;
			}
		}

		public bool IsIlOnly
		{
			get
			{
				return PeKind.Contains(ModuleAttributes.ILOnly);
			}
		}

		public CpuPlatform CpuPlatform
		{
			get
			{
				if (PeKind.Contains(ModuleAttributes.ILOnly))
				{
					if (!MachineKind.Contains(TargetArchitecture.IA64))
					{
						return CpuPlatform.X64;
					}
					return CpuPlatform.Itanium;
				}
				if (!PeKind.Contains(ModuleAttributes.Required32Bit))
				{
					return CpuPlatform.AnyCpu;
				}
				return CpuPlatform.X86;
			}
		}

		internal ModuleAttributes PeKind
		{
			get
			{
				return _PeKind;
			}
		}

		internal TargetArchitecture MachineKind
		{
			get
			{
				return _MachineKind;
			}
		}

		public bool IsSigned
		{
			get
			{
				return _IsSigned;
			}
		}

		public bool BinaryWasScanned
		{
			get
			{
				return _BinaryWasScanned;
			}
		}

		internal AssemblyBinaryProperties(ModuleAttributes peKind, TargetArchitecture machineKind, bool isSigned, string keyFileName, string keyContainer, bool binaryWasScanned)
		{
			_PeKind = peKind;
			_MachineKind = machineKind;
			_IsSigned = isSigned;
			_KeyFileName = keyFileName;
			_KeyContainer = keyContainer;
			_BinaryWasScanned = binaryWasScanned;
		}

		internal AssemblyBinaryProperties(ModuleAttributes peKind, TargetArchitecture machineKind, bool isSigned, string keyFileName, string keyContainer)
			: this(peKind, machineKind, isSigned, keyFileName, keyContainer, true)
		{
		}

		public static AssemblyBinaryProperties GetEmpty()
		{
			return EmptyNotScanned;
		}
	}
}
