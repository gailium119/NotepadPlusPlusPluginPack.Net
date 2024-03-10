using System;
using System.Collections.Generic;
using System.Globalization;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport
{
	public class DllExportNotifier : IDllExportNotifier, IDisposable
	{
		private sealed class ContextScope : IDisposable
		{
			private readonly DllExportNotifier _Notifier;

			public NotificationContext Context { get; private set; }

			public ContextScope(DllExportNotifier notifier, NotificationContext context)
			{
				Context = context;
				_Notifier = notifier;
				Stack<NotificationContext> contextScopes = _Notifier._ContextScopes;
				lock (contextScopes)
				{
					contextScopes.Push(context);
				}
			}

			public void Dispose()
			{
				Stack<NotificationContext> contextScopes = _Notifier._ContextScopes;
				lock (contextScopes)
				{
					if (contextScopes.Peek() != Context)
					{
						throw new InvalidOperationException(string.Format(Resources.Current_Notifier_Context_is___0____it_should_have_been___1___, contextScopes.Peek(), Context.Name));
					}
					contextScopes.Pop();
				}
			}
		}

		private readonly Stack<NotificationContext> _ContextScopes = new Stack<NotificationContext>();

		public string ContextName
		{
			get
			{
				NotificationContext context = Context;
				if (!(context != null))
				{
					return null;
				}
				return context.Name;
			}
		}

		public object ContextObject
		{
			get
			{
				NotificationContext context = Context;
				if (!(context != null))
				{
					return null;
				}
				return context.Context;
			}
		}

		public NotificationContext Context
		{
			get
			{
				try
				{
					return _ContextScopes.Peek();
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		public event EventHandler<DllExportNotificationEventArgs> Notification;

		public void Dispose()
		{
			this.Notification = null;
		}

		public IDisposable CreateContextName(object context, string name)
		{
			return new ContextScope(this, new NotificationContext(name, context));
		}

		public void Notify(DllExportNotificationEventArgs e)
		{
			OnNotification(Context ?? new NotificationContext(null, this), e);
		}

		private void OnNotification(object sender, DllExportNotificationEventArgs e)
		{
			EventHandler<DllExportNotificationEventArgs> notification = this.Notification;
			if (notification != null)
			{
				notification(sender, e);
			}
		}

		public void Notify(int severity, string code, string message, params object[] values)
		{
			Notify(severity, code, null, null, null, message, values);
		}

		public void Notify(int severity, string code, string fileName, SourceCodePosition? startPosition, SourceCodePosition? endPosition, string message, params object[] values)
		{
			DllExportNotificationEventArgs dllExportNotificationEventArgs = new DllExportNotificationEventArgs();
			dllExportNotificationEventArgs.Severity = severity;
			dllExportNotificationEventArgs.Code = code;
			dllExportNotificationEventArgs.Context = Context;
			dllExportNotificationEventArgs.FileName = fileName;
			dllExportNotificationEventArgs.StartPosition = startPosition;
			dllExportNotificationEventArgs.EndPosition = endPosition;
			DllExportNotificationEventArgs dllExportNotificationEventArgs2 = dllExportNotificationEventArgs;
			dllExportNotificationEventArgs2.Message = ((values.NullSafeCall(() => values.Length) == 0) ? message : string.Format(CultureInfo.InvariantCulture, message, values));
			if (!string.IsNullOrEmpty(dllExportNotificationEventArgs2.Message))
			{
				Notify(dllExportNotificationEventArgs2);
			}
		}
	}
}
