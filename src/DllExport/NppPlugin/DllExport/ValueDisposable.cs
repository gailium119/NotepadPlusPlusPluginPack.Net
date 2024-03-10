using System;

namespace NppPlugin.DllExport
{
	public class ValueDisposable<T> : GenericDisposable
	{
		public T Value { get; private set; }

		public ValueDisposable(T value, Action<T> action)
			: base(delegate
			{
				action(value);
			})
		{
			Value = value;
		}
	}
}
