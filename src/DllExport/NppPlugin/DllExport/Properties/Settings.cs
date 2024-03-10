using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NppPlugin.DllExport.Properties
{
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	[CompilerGenerated]
	public sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default
		{
			get
			{
				return defaultInstance;
			}
		}

		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		[ApplicationScopedSetting]
		public string ILDasmPath
		{
			get
			{
				return (string)this["ILDasmPath"];
			}
		}

		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		[ApplicationScopedSetting]
		public string ILAsmPath
		{
			get
			{
				return (string)this["ILAsmPath"];
			}
		}
	}
}
