using System.Collections.Generic;

namespace NppPlugin.DllExport
{
	public sealed class DuplicateExports
	{
		private readonly List<ExportedMethod> _Duplicates = new List<ExportedMethod>();

		private readonly ExportedMethod _UsedExport;

		public ExportedMethod UsedExport
		{
			get
			{
				return _UsedExport;
			}
		}

		public ICollection<ExportedMethod> Duplicates
		{
			get
			{
				return _Duplicates;
			}
		}

		internal DuplicateExports(ExportedMethod usedExport)
		{
			_UsedExport = usedExport;
		}
	}
}
