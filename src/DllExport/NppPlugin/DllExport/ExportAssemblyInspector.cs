using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Mono.Cecil;

namespace NppPlugin.DllExport
{
	internal class ExportAssemblyInspector : MarshalByRefObject, IExportAssemblyInspector
	{
		public string DllExportAttributeFullName
		{
			get
			{
				if (InputValues == null)
				{
					return Utilities.DllExportAttributeFullName;
				}
				return InputValues.DllExportAttributeFullName;
			}
		}

		public IInputValues InputValues { get; private set; }

		public ExportAssemblyInspector(IInputValues inputValues)
		{
			if (inputValues == null)
			{
				throw new ArgumentNullException("inputValues");
			}
			InputValues = inputValues;
		}

		public AssemblyExports ExtractExports(AssemblyDefinition assemblyDefinition)
		{
			return ExtractExports(assemblyDefinition, TryExtractExport);
		}

		public AssemblyExports ExtractExports()
		{
			return ExtractExports(LoadAssembly(InputValues.InputFileName));
		}

		public AssemblyExports ExtractExports(string fileName)
		{
			return ExtractExports(LoadAssembly(fileName));
		}

		private IList<TypeDefinition> TraverseNestedTypes(ICollection<TypeDefinition> types)
		{
			List<TypeDefinition> list = new List<TypeDefinition>(types.Count);
			foreach (TypeDefinition type in types)
			{
				list.Add(type);
				if (type.HasNestedTypes)
				{
					list.AddRange(TraverseNestedTypes(type.NestedTypes));
				}
			}
			return list;
		}

		public AssemblyExports ExtractExports(AssemblyDefinition assemblyDefinition, ExtractExportHandler exportFilter)
		{
			IList<TypeDefinition> list = TraverseNestedTypes(assemblyDefinition.Modules.SelectMany((ModuleDefinition m) => m.Types).ToList());
			AssemblyExports result = new AssemblyExports
			{
				InputValues = InputValues
			};
			foreach (TypeDefinition item in list)
			{
				List<ExportedMethod> list2 = new List<ExportedMethod>();
				foreach (MethodDefinition method in item.Methods)
				{
					TypeDefinition typeRefCopy = item;
					CheckForExportedMethods(() => new ExportedMethod(GetExportedClass(typeRefCopy, result)), exportFilter, list2, method);
				}
				foreach (ExportedMethod item2 in list2)
				{
					GetExportedClass(item, result).Methods.Add(item2);
				}
			}
			result.Refresh();
			return result;
		}

		private ExportedClass GetExportedClass(TypeDefinition td, AssemblyExports result)
		{
			ExportedClass value;
			if (!result.ClassesByName.TryGetValue(td.FullName, out value))
			{
				TypeDefinition typeDefinition = td;
				while (!typeDefinition.HasGenericParameters && typeDefinition.IsNested)
				{
					typeDefinition = typeDefinition.DeclaringType;
				}
				value = new ExportedClass(td.FullName, typeDefinition.HasGenericParameters);
				result.ClassesByName.Add(value.FullTypeName, value);
			}
			return value;
		}

		public AssemblyExports ExtractExports(string fileName, ExtractExportHandler exportFilter)
		{
			string currentDirectory = Directory.GetCurrentDirectory();
			try
			{
				Directory.SetCurrentDirectory(Path.GetDirectoryName(fileName));
				return ExtractExports(LoadAssembly(fileName), exportFilter);
			}
			finally
			{
				Directory.SetCurrentDirectory(currentDirectory);
			}
		}

		public AssemblyBinaryProperties GetAssemblyBinaryProperties(string assemblyFileName)
		{
			if (!File.Exists(assemblyFileName))
			{
				return AssemblyBinaryProperties.GetEmpty();
			}
			AssemblyDefinition assemblyDefinition = LoadAssembly(assemblyFileName);
			ModuleDefinition mainModule = assemblyDefinition.MainModule;
			string text = null;
			string text2 = null;
			foreach (CustomAttribute customAttribute in assemblyDefinition.CustomAttributes)
			{
				switch (customAttribute.Constructor.DeclaringType.FullName)
				{
				case "System.Reflection.AssemblyKeyFileAttribute":
					text = Convert.ToString(customAttribute.ConstructorArguments[0], CultureInfo.InvariantCulture);
					break;
				case "System.Reflection.AssemblyKeyNameAttribute":
					text2 = Convert.ToString(customAttribute.ConstructorArguments[0], CultureInfo.InvariantCulture);
					break;
				}
				if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
				{
					break;
				}
			}
			return new AssemblyBinaryProperties(mainModule.Attributes, mainModule.Architecture, assemblyDefinition.Name.HasPublicKey, text, text2);
		}

		public AssemblyDefinition LoadAssembly(string fileName)
		{
			return AssemblyDefinition.ReadAssembly(fileName);
		}

		public bool SafeExtractExports(string fileName, Stream stream)
		{
			AssemblyExports assemblyExports = ExtractExports(fileName);
			if (assemblyExports.Count == 0)
			{
				return false;
			}
			new BinaryFormatter().Serialize(stream, assemblyExports);
			return true;
		}

		private void CheckForExportedMethods(Func<ExportedMethod> createExportMethod, ExtractExportHandler exportFilter, List<ExportedMethod> exportMethods, MethodDefinition mi)
		{
			IExportInfo exportInfo;
			if (!exportFilter(mi, out exportInfo))
			{
				return;
			}
			ExportedMethod exportedMethod = createExportMethod();
			exportedMethod.IsStatic = mi.IsStatic;
			exportedMethod.IsGeneric = mi.HasGenericParameters;
			StringBuilder stringBuilder = new StringBuilder(mi.Name, mi.Name.Length + 5);
			if (mi.HasGenericParameters)
			{
				stringBuilder.Append("<");
				int num = 0;
				foreach (GenericParameter genericParameter in mi.GenericParameters)
				{
					num++;
					if (num > 1)
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(genericParameter.Name);
				}
				stringBuilder.Append(">");
			}
			exportedMethod.MemberName = stringBuilder.ToString();
			exportedMethod.AssignFrom(exportInfo);
			if (string.IsNullOrEmpty(exportedMethod.ExportName))
			{
				exportedMethod.ExportName = mi.Name;
			}
			if (exportedMethod.CallingConvention == (CallingConvention)0)
			{
				exportedMethod.CallingConvention = CallingConvention.Winapi;
			}
			exportMethods.Add(exportedMethod);
		}

		public bool TryExtractExport(ICustomAttributeProvider memberInfo, out IExportInfo exportInfo)
		{
			exportInfo = null;
			foreach (CustomAttribute customAttribute in memberInfo.CustomAttributes)
			{
				if (!(customAttribute.Constructor.DeclaringType.FullName == DllExportAttributeFullName))
				{
					continue;
				}
				exportInfo = new ExportInfo();
				IExportInfo ei = exportInfo;
				int num = -1;
				foreach (CustomAttributeArgument constructorArgument in customAttribute.ConstructorArguments)
				{
					num++;
					SetParamValue(ei, customAttribute.Constructor.Parameters[num].ParameterType.FullName, constructorArgument.Value);
				}
				foreach (var item in (from arg in customAttribute.Fields.Concat(customAttribute.Properties)
					select new
					{
						arg.Name,
						arg.Argument.Value
					}).Distinct())
				{
					SetFieldValue(ei, item.Name, item.Value);
				}
				break;
			}
			return exportInfo != null;
		}

		private static void SetParamValue(IExportInfo ei, string name, object value)
		{
			if (name == null)
			{
				return;
			}
			if (name != "System.String")
			{
				if (name == "System.Runtime.InteropServices.CallingConvention")
				{
					ei.CallingConvention = (CallingConvention)value;
				}
			}
			else
			{
				ei.ExportName = value.NullSafeCall((object v) => v.ToString());
			}
		}

		private static void SetFieldValue(IExportInfo ei, string name, object value)
		{
			string text = name.NullSafeToUpperInvariant();
			if (text == null)
			{
				return;
			}
			if (text != "NAME" && text != "EXPORTNAME")
			{
				if (text == "CALLINGCONVENTION" || text == "CONVENTION")
				{
					ei.CallingConvention = (CallingConvention)value;
				}
			}
			else
			{
				ei.ExportName = value.NullSafeToString();
			}
		}
	}
}
