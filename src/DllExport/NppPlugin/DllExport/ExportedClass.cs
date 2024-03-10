using System.Collections.Generic;

namespace NppPlugin.DllExport
{
	public class ExportedClass
	{
		private readonly List<ExportedMethod> _Methods = new List<ExportedMethod>();

		private readonly Dictionary<string, List<ExportedMethod>> _MethodsByName = new Dictionary<string, List<ExportedMethod>>();

		public string FullTypeName { get; private set; }

		public bool HasGenericContext { get; private set; }

		internal Dictionary<string, List<ExportedMethod>> MethodsByName
		{
			get
			{
				return _MethodsByName;
			}
		}

		internal List<ExportedMethod> Methods
		{
			get
			{
				return _Methods;
			}
		}

		public ExportedClass(string fullTypeName, bool hasGenericContext)
		{
			FullTypeName = fullTypeName;
			HasGenericContext = hasGenericContext;
		}

		internal void Refresh()
		{
			lock (this)
			{
				MethodsByName.Clear();
				foreach (ExportedMethod method in Methods)
				{
					List<ExportedMethod> value;
					if (!MethodsByName.TryGetValue(method.Name, out value))
					{
						MethodsByName.Add(method.Name, value = new List<ExportedMethod>());
					}
					value.Add(method);
				}
			}
		}
	}
}
