using System;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[ParserStateAction(ParserState.MethodProperties)]
	public sealed class MethodPropertiesParserAction : IlParser.ParserStateAction
	{
		public override void Execute(ParserStateValues state, string trimmedLine)
		{
			if (trimmedLine.StartsWith(".custom instance void ", StringComparison.Ordinal) && trimmedLine.Contains(base.Parser.DllExportAttributeIlAsmFullName))
			{
				state.AddLine = false;
				state.State = ParserState.DeleteExportAttribute;
				base.Notifier.Notify(-2, DllExportLogginCodes.RemovingDllExportAttribute, Resources.Removing_0_from_1_, Utilities.DllExportAttributeFullName, state.ClassNames.Peek() + "." + state.Method.Name);
			}
			else if (trimmedLine.StartsWith("// Code", StringComparison.Ordinal))
			{
				state.State = ParserState.Method;
				if (state.MethodPos != 0)
				{
					state.Result.Insert(state.MethodPos, state.Method.Declaration);
				}
			}
		}
	}
}
