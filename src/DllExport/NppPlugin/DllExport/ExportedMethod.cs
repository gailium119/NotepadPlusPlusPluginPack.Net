namespace NppPlugin.DllExport
{
	public sealed class ExportedMethod : ExportInfo
	{
		private readonly ExportedClass _ExportedClass;

		public string Name
		{
			get
			{
				return MemberName;
			}
		}

		public ExportedClass ExportedClass
		{
			get
			{
				return _ExportedClass;
			}
		}

		public string MemberName { get; set; }

		public int VTableOffset { get; set; }

		public override string ExportName
		{
			get
			{
				return base.ExportName ?? Name;
			}
			set
			{
				base.ExportName = value;
			}
		}

		public ExportedMethod(ExportedClass exportedClass)
		{
			_ExportedClass = exportedClass;
		}
	}
}
