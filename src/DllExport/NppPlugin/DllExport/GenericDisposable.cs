using System;

namespace NppPlugin.DllExport
{
	public class GenericDisposable : IDisposable
	{
		private readonly Action _Action;

		public GenericDisposable(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			_Action = action;
		}

		public void Dispose()
		{
			_Action();
		}
	}
}
