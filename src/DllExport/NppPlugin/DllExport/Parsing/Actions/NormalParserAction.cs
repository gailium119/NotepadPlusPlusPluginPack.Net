using System;
using System.Collections.Generic;
using System.Globalization;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[ParserStateAction(ParserState.Normal)]
	public sealed class NormalParserAction : IlParser.ParserStateAction
	{
		public override void Execute(ParserStateValues state, string trimmedLine)
		{
			string assemblyName;
			string aliasName;
			if (trimmedLine.StartsWith(".corflags", StringComparison.Ordinal))
			{
				state.Result.Add(string.Format(CultureInfo.InvariantCulture, ".corflags 0x{0}", Utilities.GetCoreFlagsForPlatform(state.Cpu).ToString("X8", CultureInfo.InvariantCulture)));
				state.AddLine = false;
			}
			else if (trimmedLine.StartsWith(".class", StringComparison.Ordinal))
			{
				state.State = ParserState.ClassDeclaration;
				state.AddLine = true;
				state.ClassDeclaration = trimmedLine;
			}
			else if (IsExternalAssemblyReference(trimmedLine, out assemblyName, out aliasName))
			{
				state.RegisterMsCorelibAlias(assemblyName, aliasName);
			}
		}

		private bool IsExportAttributeAssemblyReference(string trimmedLine)
		{
			return trimmedLine.StartsWith(".assembly extern '" + base.DllExportAttributeAssemblyName + "'", StringComparison.Ordinal);
		}

		private bool IsExternalAssemblyReference(string trimmedLine, out string assemblyName, out string aliasName)
		{
			assemblyName = null;
			aliasName = null;
			if (trimmedLine.Length < ".assembly extern ".Length || !trimmedLine.StartsWith(".assembly extern ", StringComparison.Ordinal))
			{
				return false;
			}
			List<string> identifiers = new List<string>();
			IlParsingUtils.ParseIlSnippet(trimmedLine.Substring(".assembly extern ".Length), ParsingDirection.Forward, delegate(IlParsingUtils.IlSnippetLocation current)
			{
				if (!current.WithinString && current.CurrentChar == '\'' && current.LastIdentifier != null)
				{
					identifiers.Add(current.LastIdentifier);
					if (identifiers.Count > 1)
					{
						return false;
					}
				}
				return true;
			});
			if (identifiers.Count == 0)
			{
				return false;
			}
			if (identifiers.Count > 0)
			{
				assemblyName = identifiers[0];
			}
			aliasName = ((identifiers.Count > 1) ? identifiers[1] : identifiers[0]);
			return true;
		}
	}
}
