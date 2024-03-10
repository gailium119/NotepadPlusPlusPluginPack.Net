using System;
using System.Runtime.InteropServices;

namespace NppPlugin.DllExport
{
	[Serializable]
	public class ExportInfo : IExportInfo
	{
		public virtual string ExportName { get; set; }

		public CallingConvention CallingConvention { get; set; }

		public bool IsStatic { get; set; }

		public bool IsGeneric { get; set; }

		public void AssignFrom(IExportInfo info)
		{
			if (info != null)
			{
				CallingConvention = ((info.CallingConvention != 0) ? info.CallingConvention : CallingConvention.StdCall);
				ExportName = info.ExportName;
			}
		}
	}
}
