using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NppPlugin.DllExport
{
	public sealed class AssemblyExports
	{
		internal static readonly Dictionary<CallingConvention, string> ConventionTypeNames;

		private readonly Dictionary<string, ExportedClass> _ClassesByName = new Dictionary<string, ExportedClass>();

		private readonly List<DuplicateExports> _DuplicateExportMethods = new List<DuplicateExports>();

		private readonly Dictionary<object, ExportedMethod> _DuplicateExportMethodsbyFullName = new Dictionary<object, ExportedMethod>();

		private readonly Dictionary<string, ExportedMethod> _MethodsByExportName = new Dictionary<string, ExportedMethod>();

		private readonly ReadOnlyCollection<DuplicateExports> _ReadOnlyDuplicateExportMethods;

		public ReadOnlyCollection<DuplicateExports> DuplicateExportMethods
		{
			get
			{
				return _ReadOnlyDuplicateExportMethods;
			}
		}

		public IInputValues InputValues { get; set; }

		public string DllExportAttributeAssemblyName
		{
			get
			{
				if (InputValues == null)
				{
					return Utilities.DllExportAttributeAssemblyName;
				}
				return InputValues.DllExportAttributeAssemblyName;
			}
		}

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

		internal Dictionary<string, ExportedMethod> MethodsByExportName
		{
			get
			{
				return _MethodsByExportName;
			}
		}

		internal Dictionary<string, ExportedClass> ClassesByName
		{
			get
			{
				return _ClassesByName;
			}
		}

		public int Count
		{
			get
			{
				return MethodsByExportName.Count;
			}
		}

		static AssemblyExports()
		{
			ConventionTypeNames = new Dictionary<CallingConvention, string>
			{
				{
					CallingConvention.Cdecl,
					typeof(CallConvCdecl).FullName
				},
				{
					CallingConvention.FastCall,
					typeof(CallConvFastcall).FullName
				},
				{
					CallingConvention.StdCall,
					typeof(CallConvStdcall).FullName
				},
				{
					CallingConvention.ThisCall,
					typeof(CallConvThiscall).FullName
				},
				{
					CallingConvention.Winapi,
					typeof(CallConvStdcall).FullName
				}
			};
		}

		public AssemblyExports()
		{
			_ReadOnlyDuplicateExportMethods = new ReadOnlyCollection<DuplicateExports>(_DuplicateExportMethods);
		}

		internal void Refresh()
		{
			int num = 0;
			MethodsByExportName.Clear();
			_DuplicateExportMethods.Clear();
			Dictionary<string, DuplicateExports> dictionary = new Dictionary<string, DuplicateExports>();
			foreach (ExportedClass value2 in ClassesByName.Values)
			{
				List<ExportedMethod> list = new List<ExportedMethod>(value2.Methods.Count);
				foreach (ExportedMethod method in value2.Methods)
				{
					DuplicateExports value;
					if (!dictionary.TryGetValue(method.ExportName, out value))
					{
						method.VTableOffset = num++;
						MethodsByExportName.Add(method.MemberName, method);
						dictionary.Add(method.ExportName, new DuplicateExports(method));
					}
					else
					{
						list.Add(method);
						value.Duplicates.Add(method);
					}
				}
				ExportedClass exportClassCopy = value2;
				list.ForEach(delegate(ExportedMethod m)
				{
					exportClassCopy.Methods.Remove(m);
				});
				value2.Refresh();
			}
			foreach (DuplicateExports value3 in dictionary.Values)
			{
				if (value3.Duplicates.Count <= 0)
				{
					continue;
				}
				_DuplicateExportMethods.Add(value3);
				foreach (ExportedMethod duplicate in value3.Duplicates)
				{
					_DuplicateExportMethodsbyFullName.Add(GetKey(duplicate.ExportedClass.FullTypeName, duplicate.MemberName), duplicate);
				}
			}
			_DuplicateExportMethods.Sort((DuplicateExports l, DuplicateExports r) => string.CompareOrdinal(l.UsedExport.ExportName, r.UsedExport.ExportName));
		}

		public ExportedMethod GetDuplicateExport(string fullTypeName, string memberName)
		{
			ExportedMethod exportedMethod;
			if (!TryGetDuplicateExport(fullTypeName, memberName, out exportedMethod))
			{
				return null;
			}
			return exportedMethod;
		}

		public bool TryGetDuplicateExport(string fullTypeName, string memberName, out ExportedMethod exportedMethod)
		{
			return _DuplicateExportMethodsbyFullName.TryGetValue(GetKey(fullTypeName, memberName), out exportedMethod);
		}

		private static object GetKey(string fullTypeName, string memberName)
		{
			return new { fullTypeName, memberName };
		}
	}
}
