using System;

namespace NppPlugin.DllExport.Parsing
{
	public abstract class HasServiceProvider : IDisposable, IServiceProvider
	{
		private readonly IServiceProvider _ServiceProvider;

		public IServiceProvider ServiceProvider
		{
			get
			{
				return _ServiceProvider;
			}
		}

		protected HasServiceProvider(IServiceProvider serviceProvider)
		{
			_ServiceProvider = serviceProvider;
		}

		public void Dispose()
		{
			IDisposable disposable = _ServiceProvider as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return _ServiceProvider.GetService(serviceType);
		}
	}
}
