using System;

namespace NppPlugin.DllExport
{
	public class DllExportNotificationEventArgs : EventArgs
	{
		public NotificationContext Context { get; set; }

		public string Message { get; set; }

		public int Severity { get; set; }

		public string Code { get; set; }

		public string FileName { get; set; }

		public SourceCodePosition? StartPosition { get; set; }

		public SourceCodePosition? EndPosition { get; set; }
	}
}
