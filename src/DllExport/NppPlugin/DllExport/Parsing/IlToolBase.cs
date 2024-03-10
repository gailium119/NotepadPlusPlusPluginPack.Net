using System;

namespace NppPlugin.DllExport.Parsing
{
	public abstract class IlToolBase : HasServiceProvider
	{
		protected IDllExportNotifier Notifier
		{
			get
			{
				return base.ServiceProvider.GetService<IDllExportNotifier>();
			}
		}

		public int Timeout { get; set; }

		public IInputValues InputValues { get; private set; }

		public string TempDirectory { get; set; }

		protected IlToolBase(IServiceProvider serviceProvider, IInputValues inputValues)
			: base(serviceProvider)
		{
			if (inputValues == null)
			{
				throw new ArgumentNullException("inputValues");
			}
			InputValues = inputValues;
		}
	}
}
