using System;

namespace NppPlugin.DllExport.Parsing
{
	public abstract class DllExportNotifierWrapper : IDllExportNotifier, IDisposable
	{
		protected virtual IDllExportNotifier Notifier { get; private set; }

		protected virtual bool OwnsNotifier
		{
			get
			{
				return false;
			}
		}

		event EventHandler<DllExportNotificationEventArgs> IDllExportNotifier.Notification
		{
			add
			{
				Notifier.Notification += value;
			}
			remove
			{
				Notifier.Notification -= value;
			}
		}

		protected DllExportNotifierWrapper(IDllExportNotifier notifier)
		{
			Notifier = notifier;
		}

		public IDisposable CreateContextName(object context, string name)
		{
			return Notifier.CreateContextName(context, name);
		}

		public void Notify(DllExportNotificationEventArgs e)
		{
			Notifier.Notify(e);
		}

		public void Notify(int severity, string code, string message, params object[] values)
		{
			Notifier.Notify(severity, code, message, values);
		}

		public void Notify(int severity, string code, string fileName, SourceCodePosition? startPosition, SourceCodePosition? endPosition, string message, params object[] values)
		{
			Notifier.Notify(severity, code, fileName, startPosition, endPosition, message, values);
		}

		public void Dispose()
		{
			if (OwnsNotifier)
			{
				IDisposable disposable = Notifier as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
