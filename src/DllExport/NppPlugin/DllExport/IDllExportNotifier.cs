using System;

namespace NppPlugin.DllExport
{
	public interface IDllExportNotifier
	{
		event EventHandler<DllExportNotificationEventArgs> Notification;

		void Notify(int severity, string code, string message, params object[] values);

		void Notify(int severity, string code, string fileName, SourceCodePosition? startPosition, SourceCodePosition? endPosition, string message, params object[] values);

		void Notify(DllExportNotificationEventArgs e);

		IDisposable CreateContextName(object context, string name);
	}
}
