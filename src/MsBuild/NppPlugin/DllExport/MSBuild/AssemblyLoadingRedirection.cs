using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NppPlugin.DllExport.MSBuild.Properties;

namespace NppPlugin.DllExport.MSBuild
{
	internal static class AssemblyLoadingRedirection
	{
		public static readonly bool IsSetup;

		public static void EnsureSetup()
		{
			if (!IsSetup)
			{
				throw new InvalidOperationException(string.Format(Resources.AssemblyRedirection_for_0_has_not_been_setup_, typeof(AssemblyLoadingRedirection).FullName));
			}
		}

		static AssemblyLoadingRedirection()
		{
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				if (new string[2] { "Mono.Cecil", "NppPlugin.DllExport" }.Contains(assemblyName.Name))
				{
					string text = Path.Combine(Path.GetDirectoryName(new Uri(typeof(AssemblyLoadingRedirection).Assembly.EscapedCodeBase).AbsolutePath), assemblyName.Name + ".dll");
					if (File.Exists(text))
					{
						return Assembly.LoadFrom(text);
					}
				}
				return null;
			};
			IsSetup = true;
		}
	}
}
