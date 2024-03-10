using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace NppPlugin.DllExport.MSBuild.Properties
{
	[CompilerGenerated]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	public class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(resourceMan, null))
				{
					resourceMan = new ResourceManager("NppPlugin.DllExport.MSBuild.Properties.Resources", typeof(Resources).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		public static string AssemblyRedirection_for_0_has_not_been_setup_
		{
			get
			{
				return ResourceManager.GetString("AssemblyRedirection_for_0_has_not_been_setup_", resourceCulture);
			}
		}

		public static string Both_key_values_KeyContainer_0_and_KeyFile_0_are_present_only_one_can_be_specified
		{
			get
			{
				return ResourceManager.GetString("Both_key_values_KeyContainer_0_and_KeyFile_0_are_present_only_one_can_be_specified", resourceCulture);
			}
		}

		public static string Cannot_find_ilasm_exe_in_0_
		{
			get
			{
				return ResourceManager.GetString("Cannot_find_ilasm_exe_in_0_", resourceCulture);
			}
		}

		public static string Cannot_find_ilasm_exe_without_a_FrameworkPath
		{
			get
			{
				return ResourceManager.GetString("Cannot_find_ilasm_exe_without_a_FrameworkPath", resourceCulture);
			}
		}

		public static string Cannot_find_lib_exe_in_0_
		{
			get
			{
				return ResourceManager.GetString("Cannot_find_lib_exe_in_0_", resourceCulture);
			}
		}

		public static string Cannot_get_a_reference_to_ToolLocationHelper
		{
			get
			{
				return ResourceManager.GetString("Cannot_get_a_reference_to_ToolLocationHelper", resourceCulture);
			}
		}

		public static string Input_file_0_does_not_exist__cannot_create_unmanaged_exports
		{
			get
			{
				return ResourceManager.GetString("Input_file_0_does_not_exist__cannot_create_unmanaged_exports", resourceCulture);
			}
		}

		public static string Input_file_0_is_not_a_DLL_hint
		{
			get
			{
				return ResourceManager.GetString("Input_file_0_is_not_a_DLL_hint", resourceCulture);
			}
		}

		public static string Input_file_is_empty__cannot_create_unmanaged_exports
		{
			get
			{
				return ResourceManager.GetString("Input_file_is_empty__cannot_create_unmanaged_exports", resourceCulture);
			}
		}

		public static string Output_assembly_was_signed_however_neither_keyfile_nor_keycontainer_could_be_inferred__Reading_those_values_from_assembly_attributes_is_not__yet__supported__they_have_to_be_defined_inside_the_MSBuild_project_file
		{
			get
			{
				return ResourceManager.GetString("Output_assembly_was_signed_however_neither_keyfile_nor_keycontainer_could_be_inferred__Reading_those_values_from_assembly_attributes_is_not__yet__supported__they_have_to_be_defined_inside_the_MSBuild_project_file", resourceCulture);
			}
		}

		public static string SdkPath_is_empty_continuing_with_0_
		{
			get
			{
				return ResourceManager.GetString("SdkPath_is_empty_continuing_with_0_", resourceCulture);
			}
		}

		public static string Skipped_Method_Exports
		{
			get
			{
				return ResourceManager.GetString("Skipped_Method_Exports", resourceCulture);
			}
		}

		public static string ToolLocationHelperTypeName_could_not_find_1
		{
			get
			{
				return ResourceManager.GetString("ToolLocationHelperTypeName_could_not_find_1", resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}
